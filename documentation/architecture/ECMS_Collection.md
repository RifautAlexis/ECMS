# ECMS.Collection

## Purpose and boundaries

`ECMS.Collection` polls configured devices, isolates protocol communication, maps successful payloads to the shared device data contracts, and publishes success or failure outcomes to `ECMS.Core`. `ECMS.Core` owns device availability, device state, business rules, and persistence. `ECMS.Host` loads startup configuration and controls worker lifecycle.

HTTP is the initial protocol. Each configured device is polled with `GET http://{ipAddress}:{port}/`. Its JSON body must match the data shape for its configured `kind`: `1` (`RfPowerSensorData`), `2` (`RfPowerAmplifierData`), or `3` (`RfMonitoringSystemData`).

## Data flow

```text
Host startup configuration ──┐
                             ▼
Core future updates ──► Device configuration update boundary
                             │
                             ▼
                    Collection orchestrator
                     ├── per-device HTTP poll loops
                     ├── bounded raw-results channel
                     ├── kind-specific data mapping
                     └── bounded standardized-results channel
                                      │
                                      ▼
                                  Core worker
```

The standardized `CollectionResult` contract is in `ECMS.Contracts`. It includes device ID, kind, attempt timestamp, success status, optional `CollectedData`, and an error for failed attempts. The raw response model and raw channel are internal to `ECMS.Collection`.

## Polling and failure handling

Each device has an independent poll loop. A loop makes one request at a time and waits for the global `pollingIntervalSeconds` after that attempt completes. Request duration is capped by that device's `requestTimeoutSeconds`. `maxConcurrentRequests` limits active HTTP requests across devices. Communication, timeout, and payload-mapping failures are published as failed `CollectionResult` values; a failed request does not terminate the device loop.

Both result channels are bounded and use wait-based backpressure. Collection observes cancellation while requesting, waiting to publish, and delaying between attempts. On shutdown, device loops and raw processing are canceled, device loops are awaited, the raw channel is completed, and the standardized channel writer is completed. Results still queued during shutdown may not be delivered.

## Device list ownership and updates

Host currently seeds collection with the configured device list. `IDeviceConfigurationUpdatePublisher` and `IDeviceConfigurationUpdateSource`, defined in `ECMS.Contracts`, form the handoff boundary for Core to publish later device-list snapshots.

Each update is a complete desired device set. The update boundary keeps the latest pending snapshot. Reconciliation starts loops for added devices, stops removed devices, restarts devices whose configuration changed, and leaves unaffected loops running. Device IDs must be positive and unique within a snapshot.

## Configuration

```json
{
  "collection": {
    "pollingIntervalSeconds": 1,
    "maxConcurrentRequests": 32,
    "devicesConfiguration": [
      {
        "id": 1,
        "kind": 1,
        "ipAddress": "192.168.1.1",
        "port": 8080,
        "protocol": "Http",
        "requestTimeoutSeconds": 5
      }
    ]
  }
}
```

The interval, concurrency limit, and request timeout must be positive. Ports must be in the range 1–65535. The initial implementation accepts only the `Http` protocol and the three device kinds listed above. An empty device list is valid.
