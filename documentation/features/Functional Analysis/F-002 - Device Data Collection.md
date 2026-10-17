# F-002 — Device Data Collection

## 1. Description

ECMS periodically collects data from the devices defined in its validated configuration.

ECMS.Collection is responsible for requesting device data, interpreting the responses according to the corresponding device type, and making standardized collection outcomes available to ECMS.Core.

Each device is polled independently. A slow or unresponsive device must not prevent ECMS from continuing to poll other devices.

The collection process must support both successful data collection and collection failures.

## 2. Functional Requirements

### FR-002-001 — Collect data from configured devices

ECMS must collect data from each device defined in the validated configuration.

Each device is identified by its unique ID and its configured device kind.

Only device definitions from a successfully validated configuration may be used for collection.

### FR-002-002 — Communicate with devices

ECMS.Collection must use HTTP GET requests to request data from configured devices.

The communication process must identify the device associated with each request so that its response or failure can be attributed to the correct device.

### FR-002-003 — Poll devices independently

ECMS must manage polling independently for each configured device.

A slow response, timeout, or communication failure involving one device must not block the polling of other devices.

A request to a device must not overlap with another request to that same device. If a scheduled poll occurs while the previous request to that device is still in progress, the missed poll must be skipped rather than queued.

An unresponsive device must not prevent future polling attempts to that device indefinitely.

### FR-002-004 — Apply the polling interval

The `collection.pollingIntervalSeconds` setting defines the recurring polling interval for each device independently.

The interval is measured between scheduled polling requests for the same device, rather than between requests to different devices or between complete collection cycles.

The configured interval applies equally to all devices. Each device may nevertheless have a different effective data refresh rate because response times and failures can differ.

The polling interval is a target cadence; ECMS does not guarantee that every device's data will be refreshed at precisely that interval.

### FR-002-005 — Standardize collected data

ECMS.Collection must interpret successful device responses according to the device kind and produce a standardized collection outcome for ECMS.Core.

The standardized outcome must identify the device and its kind and provide its collected data in the format expected by ECMS.Core.

Each device kind supported by ECMS must have a corresponding process for interpreting and standardizing its response.

Device-specific response formats and detailed data schemas are defined separately from this feature.

### FR-002-006 — Report collection failures

If ECMS cannot collect data from a device, including because of a timeout, a network error, an unreachable device, or an unsuccessful response, ECMS.Collection must produce a failure outcome identifying the affected device.

A collection failure must not be treated as successful device data, nor must it prevent collection from other devices.

### FR-002-007 — Make collection outcomes available to ECMS.Core

ECMS.Collection must make collection outcomes available to ECMS.Core, including both successful standardized data and failures.

Each outcome must contain enough information for ECMS.Core to identify the affected device and process the result appropriately.

ECMS.Core is responsible for updating device state, persisting accepted data, and managing device availability according to F-005 — Device Availability Management.

### FR-002-008 — Continue periodic collection

ECMS must continue attempting to collect data from configured devices during normal operation.

A collection failure for one device must not terminate the overall collection process. Subsequent polling attempts must continue according to the device's polling schedule.

## 3. Dependencies

- F-001 — Configuration Management

## 4. Dependents

- F-005 — Device Availability Management