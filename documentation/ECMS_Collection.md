# ECMS.Collection

## 1. Purpose

`ECMS.Collection` is responsible for collecting data from devices and transforming that data into the standardized contracts used by the rest of the ECMS application.

The project currently uses **mock device generators** because the concrete physical devices and their communication protocols have not yet been defined.

The main responsibility of `ECMS.Collection` is therefore:

> **Collect device data and provide it to `ECMS.Core` using the contracts defined in `ECMS.Contracts`.**

The current configuration received from `ECMS.Host` is specifically intended to configure the **mock device generator**. It should not be considered the final configuration model for the physical device collection implementation.

---

## 2. Responsibilities

`ECMS.Collection` is responsible for:

- Collecting data from devices
- Maintaining the collection mechanisms required for each device
- Producing standardized device data using contracts from `ECMS.Contracts`
- Sending collected data asynchronously to `ECMS.Core`
- Currently, generating semi-realistic data through mock devices

The final implementation may introduce additional responsibilities related to:

- Device communication
- Device connection management
- Protocol-specific handling
- Timeouts and retries
- Device discovery or initialization

These aspects will be defined once the physical devices and their communication mechanisms are known.

`ECMS.Collection` is **not** responsible for:

- Application startup or dependency injection configuration
- Reading the configuration file directly
- Monitoring business logic
- Alarm processing
- Persisting data to SQLite
- Exposing data through an external API
- Deciding how collected data should be processed or displayed

---

## 3. Configuration Received from ECMS.Host

`ECMS.Host` is responsible for reading the application configuration and providing the configuration required by `ECMS.Collection`.

The configuration is provided as a single object.

The type of this configuration object is defined in `ECMS.Contracts`.

The configuration object contains the information required by `ECMS.Collection` for its current implementation.

---

## 4. Device Identity and Kind

Each device is identified by two distinct concepts:

- **`id`** identifies a specific device instance.
- **`kind`** identifies the type of device.

The distinction is:

> **`id` answers "Which device is this?"**  
> **`kind` answers "What type of device is this?"**

For example:

```json
{
    "id": 42,
    "kind": 2,
    "data": {
        "forwardPower": 1250.2,
        "reflectedPower": 12.4,
        "temperature": 43.7
    }
}
```

In this example:

- `id = 42` identifies the individual device.
- `kind = 2` identifies the device type.
- `data` contains the data specific to that device type.

The `id` is therefore required to distinguish multiple devices having the same `kind`.

---

## 5. Current Generator Configuration

During the current mock implementation, `ECMS.Host` provides a configuration used specifically to configure the mock device generator.

The configuration contains a list of devices:

```json
{
    "pollingIntervalSeconds": 1,
    "devicesConfiguration": [
        {
            "id": 1,
            "kind": 1
        },
        {
            "id": 2,
            "kind": 2
        },
        {
            "id": 3,
            "kind": 2
        },
        {
            "id": 4,
            "kind": 3
        }
    ]
}
```

The implemented configuration property is named `devicesConfiguration`:

```json
{
    "pollingIntervalSeconds": 1,
    "devicesConfiguration": [
        { "id": 1, "kind": 1 },
        { "id": 2, "kind": 2 },
        { "id": 3, "kind": 3 }
    ]
}
```

The polling interval must be positive. Device IDs must be positive and unique, and the currently supported kinds are `1` (RF power sensor), `2` (RF power amplifier), and `3` (multi-channel RF monitoring system). An empty device list is valid.

This configuration allows the generator to know:

- Which mock device instances must be created
- The identity of each simulated device
- Which device type must be simulated

The number of configured devices is not fixed

The configuration may contain:

- Zero devices
- One device
- Multiple devices
- Multiple instances of the same `kind`

For example:

```json
{
    "pollingIntervalSeconds": 1,
    "devicesConfiguration": [
        {
            "id": 10,
            "kind": 2
        },
        {
            "id": 11,
            "kind": 2
        },
        {
            "id": 12,
            "kind": 2
        }
    ]
}
```

This represents three independent simulated devices of kind `2`.

Each instance must maintain its own simulated state.

---

## 6. Current Implementation

There are currently no concrete physical devices defined for ECMS.

Consequently, `ECMS.Collection` initially uses **mock device generators**.

The mock implementation allows the rest of the application to be developed and tested without requiring physical RF equipment.

Each configured device gets its own simulated state.

For example:

```text
Configured devices

Device ID 1 / Kind 1 ──► Mock Device Instance A
Device ID 2 / Kind 2 ──► Mock Device Instance B
Device ID 3 / Kind 2 ──► Mock Device Instance C
Device ID 4 / Kind 3 ──► Mock Device Instance D
```

Although Device 2 and Device 3 have the same `kind`, they are independent device instances.

---

## 7. Communication with ECMS.Core

`ECMS.Collection` and `ECMS.Core` run inside the same application process.

Communication between the two components uses an in-process asynchronous channel.

The proposed mechanism is `System.Threading.Channels`.

```text
ECMS.Collection
       │
       │ Channel<CollectedData>
       ▼
ECMS.Core
```

`ECMS.Collection` acts as the **producer**.

`ECMS.Core` acts as the **consumer**.

The channel is an implementation detail of the application architecture. It is not part of the standardized data contract.

This allows collection and processing to operate independently and asynchronously.

---

## 8. Collection Interval

Each mock device is collected periodically.

The collection interval is configurable for the generator.

Each simulated device periodically generates a new state and sends its current data to `ECMS.Core`.

Conceptually:

```text
Device
   │
   │ Wait collection interval
   ▼
Generate / collect data
   │
   ▼
Create ECMS.Contracts data
   │
   ▼
Send to ECMS.Core
   │
   ▼
Repeat
```

The exact default collection interval will be defined during implementation.

The collection interval is part of the current generator configuration and should not be considered a definitive requirement for physical device collection.

The current implementation publishes one `CollectedData` envelope per configured device at each polling interval. The in-process channel has bounded capacity and applies backpressure until a consumer reads the collected data.

---

## 9. Standardized Data Contract

The data produced by `ECMS.Collection` follows a common envelope:

```json
{
    "id": 42,
    "kind": 1,
    "data": {}
}
```

The properties have the following meaning:

| Property | Purpose                                                                   |
| -------- | ------------------------------------------------------------------------- |
| `id`     | Identifies the individual device instance                                 |
| `kind`   | Identifies the device type                                                |
| `data`   | Contains the data specific to the device type defined in `ECMS.Contracts` |

For example:

```json
{
    "id": 42,
    "kind": 1,
    "data": {
        "forwardPower": 82.4,
        "reflectedPower": 1.7,
        "vswr": 1.15,
        "temperature": 31.8
    }
}
```

Another device kind can have a completely different data structure:

```json
{
    "id": 43,
    "kind": 2,
    "data": {
        "forwardPower": 1250.2,
        "reflectedPower": 12.4,
        "temperature": 43.7,
        "supplyVoltage": 48.1,
        "current": 27.3,
        "gain": 38.2
    }
}
```

The standardized contract allows `ECMS.Core` to receive data without depending on the specific collection mechanism used to obtain it.

---

## 10. Mock Device Types

The mock implementation should simulate several different types of RF devices.

The objective is not to reproduce a specific physical device but to validate that ECMS can handle different device types and data structures.

### 10.1 RF Power Sensor

A simple RF power measurement device.

Example:

```json
{
    "id": 1,
    "kind": 1,
    "data": {
        "forwardPower": 82.4,
        "reflectedPower": 1.7,
        "vswr": 1.15,
        "temperature": 31.8
    }
}
```

---

### 10.2 RF Power Amplifier

A more complex device with a larger number of properties.

Example properties:

- Forward power

- Reflected power

- VSWR

- Temperature

- Supply voltage

- Current

- RF input power

- Gain

- Efficiency

- Enabled state

- RF output state

- PLL lock state

- Over-temperature alarm

- Over-current alarm

- VSWR alarm

Example:

```json
{
    "id": 2,
    "kind": 2,
    "data": {
        "forwardPower": 1250.2,
        "reflectedPower": 12.4,
        "vswr": 1.18,
        "temperature": 43.7,
        "supplyVoltage": 48.1,
        "current": 27.3,
        "rfInputPower": 32.7,
        "gain": 38.2,
        "efficiency": 81.4,
        "enabled": true,
        "rfOutputEnabled": true,
        "pllLocked": true,
        "overTemperature": false,
        "overCurrent": false,
    }
}
```

### 10.3 Multi-Channel RF Monitoring System

The third mock device represents a larger RF monitoring system with a significantly larger data structure.

It may contain:

- System-level properties.

- Multiple RF channels.

- Per-channel power measurements.

- Per-channel VSWR.

- Per-channel status.

- Per-channel alarms.

For example:

```json
{
    "id": 4,
    "kind": 3,
    "data": {
        "temperature": 38.2,
        "systemStatus": "Running",
        "channels": [
            {
                "forwardPower": 820.3,
                "reflectedPower": 7.2,
                "vswr": 1.12,
                "enabled": true,
            },
            {
                "forwardPower": 795.6,
                "reflectedPower": 9.1,
                "vswr": 1.16,
                "enabled": true,
            }
        ]
    }
}
```

---

## 11. Multiple Device Instances

The collection system must support any number of configured device instances.

For example:

```json
{
    "devices": [
        {
            "id": 1,
            "kind": 1
        },
        {
            "id": 2,
            "kind": 1
        },
        {
            "id": 3,
            "kind": 2
        },
        {
            "id": 4,
            "kind": 2
        },
        {
            "id": 5,
            "kind": 2
        },
        {
            "id": 6,
            "kind": 3
        }
    ]
}
```

This represents:

- Two devices of kind `1`.

- Three devices of kind `2`.

- One device of kind `3`.

Each instance must maintain its own state.

For example, two RF power amplifiers should not share:

- Temperature.

- Power values.

- Alarm states.

- Enabled state.

- Any other simulated device state.

---

## 12. Semi-Realistic Data Evolution

The mock data should not consist of completely independent random values.

Values should evolve over time in a way that resembles real equipment.

For example:

```
Temperature
    │
    ├── increases when the device operates at higher power
    │
    └── decreases progressively when power decreases
```

Similarly:

```
Forward Power
       │
       ▼
Higher amplifier load
       │
       ├── Higher current
       ├── Higher temperature
       └── Potentially higher reflected power
```

The goal is to produce data that is sufficiently realistic to exercise monitoring, storage, alarm processing, and communication components.

The mock implementation should therefore model relationships between properties rather than generating every property independently.

---

## 13. Future Physical Device Implementation

The concrete physical devices and their communication mechanisms will be defined later.

Potential communication mechanisms may include:

- Serial communication

- TCP/IP

- Modbus

- SNMP

- Vendor-specific protocols

- Other device-specific interfaces

The final implementation may therefore look conceptually like:

```
                    ┌── Mock Device Generator
                    │
ECMS.Collection ────┼── Serial Device
                    │
                    ├── TCP Device
                    │
                    ├── SNMP Device
                    │
                    └── Vendor-specific Device
                           │
                           ▼
                    ECMS.Contracts
                           │
                           ▼
                       ECMS.Core
```

The physical communication implementation should remain inside `ECMS.Collection`.

The rest of the application should consume the standardized contracts without needing to know how the data was obtained.

The current generator configuration should not constrain this future implementation.

---

## 14. Non-Responsibilities

`ECMS.Collection` does not contain:

- Application startup logic

- Configuration file loading

- SQLite persistence

- Monitoring business rules

- Alarm evaluation logic

- External API endpoints

- UI logic

- Device data presentation

The current generator configuration is also **not intended to define the final physical device configuration model**.

The role of `ECMS.Collection` is ultimately to collect device data, standardize it using `ECMS.Contracts`, and submit it to `ECMS.Core`.

---

## 15. Summary

`ECMS.Collection` is the data acquisition layer of ECMS.

Its current implementation uses mock devices because the physical devices and their communication protocols are not yet defined.

`ECMS.Host` owns application configuration loading. It provides each ECMS project with a single configuration object containing the information required by that project.

For `ECMS.Collection`, the current configuration object is specifically used to configure the mock device generator. It should not be considered the final configuration model for physical device collection.

The overall flow is:

```
Configuration File
       │
       ▼
ECMS.Host
       │
       │ CollectionConfiguration
       ▼
ECMS.Collection
       │
       │ Mock device generation
       │
       │ Standardized data
       ▼
ECMS.Contracts
       │
       ▼
ECMS.Core
```

The key responsibilities are:

1. Receive its configuration from `ECMS.Host`

2. Collect or simulate device data

3. Maintain independent device instances

4. Identify each device using its `id`

5. Identify the device type using its `kind`

6. Represent collected data using contracts defined in `ECMS.Contracts`

7. Send the data asynchronously to `ECMS.Core`

8. Eventually provide the concrete physical device collection mechanisms

9. Keep physical device communication details isolated from the rest of ECMS

`ECMS.Collection` therefore answers the question:

> **How do we collect and standardize device data?**
