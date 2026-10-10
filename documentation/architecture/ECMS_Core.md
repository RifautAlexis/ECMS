# ECMS.Core

## 1. Purpose

`ECMS.Core` contains the core application logic of ECMS.

It receives standardized data from `ECMS.Collection`, maintains the current state of monitored devices, evaluates alarms, and persists data to SQLite.

It also exposes application services used by `ECMS.Communication`.

> Process collected device data and provide the core monitoring functionality of ECMS.

---

## 2. Responsibilities

`ECMS.Core` is responsible for:

- receiving and processing collected device data
- maintaining the current state of monitored devices in memory
- detecting changes in device data
- persisting changed data to SQLite
- evaluating monitoring conditions and alarms
- exposing application services to other components

Core owns the monitoring state and business logic of the application.

---

## 3. Configuration Received from ECMS.Host

`ECMS.Host` reads the application configuration from `./config.json` and provides the required configuration to `ECMS.Core`.

The configuration is provided as a single object, whose type is defined in `ECMS.Contracts`.

`ECMS.Core` does not read the configuration file directly.

The configuration may contain Core-specific settings, such as monitoring and persistence options. Its exact structure will evolve as the application requirements are defined.

```text
./config.json
      │
      ▼
  ECMS.Host
      │
      │ Core configuration object
      ▼
   ECMS.Core
```

---

## 4. Data Received from ECMS.Collection

`ECMS.Core` receives standardized device data produced by `ECMS.Collection`.

The common data envelope contains:

```json
{
  "id": 42,
  "kind": 1,
  "data": {
    "forwardPower": 1250.2,
    "reflectedPower": 12.4,
    "temperature": 43.7
  }
}
```

The envelope provides:

- `id` — identifies the device instance
- `kind` — identifies the device type
- `data` — contains the device-specific measurements and properties

Core uses this information to identify the corresponding device and process its data.

The data structure is defined in `ECMS.Contracts`.

---

## 5. Communication with ECMS.Collection

`ECMS.Collection` and `ECMS.Core` run in the same process.

Collected data is transferred asynchronously using a `System.Threading.Channels` channel.

`ECMS.Collection` acts as the producer, while `ECMS.Core` acts as the consumer.

```text
ECMS.Collection
       │
       │ Channel<CollectedData>
       ▼
   ECMS.Core
```

The channel is an implementation detail and is not part of the shared data contract.

Core consumes the incoming data independently of the device collection mechanisms.

---

## 6. Device State and Data Persistence

`ECMS.Core` maintains the current state of each monitored device in memory and persists that state to SQLite.

When new data is received, Core identifies the corresponding device by its `id` and compares the incoming values with its current in-memory state.

Only properties whose values have changed are updated in memory and written to the database.

The processing flow is:

```text
New CollectedData
        │
        ▼
Identify Device by ID
        │
        ▼
Compare with In-Memory State
        │
        ▼
Identify Changed Properties
        │
        ▼
Update In-Memory State
        │
        ▼
Persist Changed Properties to SQLite
```

The following rules apply:

- Each monitored device has its own in-memory state.
- The `id` identifies the device instance, and `kind` identifies its type.
- Incoming data is compared against the current state of the corresponding device.
- Only properties whose values have changed are updated in memory.
- Only changed properties are written to SQLite.
- If no properties have changed, no device data update is required in SQLite.

This approach avoids unnecessary database writes when devices are polled frequently but their values remain unchanged.

The in-memory state represents the latest known values for each device, while SQLite provides persistent storage.

---

## 7. Data Processing

`ECMS.Core` processes incoming data according to the device type and the application's monitoring requirements.

Processing may include:

- updating device state
- calculating derived values
- checking monitoring conditions
- detecting abnormal conditions
- triggering or recovering alarms
- persisting relevant changes

Device-specific interpretation and monitoring rules belong to Core, not to the collection or communication layers.

The exact processing rules depend on the supported device types and monitoring requirements.

---

## 8. Alarm Processing

`ECMS.Core` is responsible for evaluating and managing alarms.

An alarm may be triggered when a monitored value violates a configured condition.

Core is responsible for:

- evaluating alarm conditions
- detecting alarm activation
- detecting alarm recovery
- maintaining the current alarm state
- making alarm information available to other components
- persisting relevant alarm changes when required

Alarm evaluation uses the device data and monitoring rules available to Core.

The exact alarm model and evaluation rules will be defined as the monitoring functionality is implemented.

---

## 9. Data Persistence

`ECMS.Core` is responsible for storing and retrieving application data using SQLite.

SQLite is the current persistence mechanism for ECMS.

Core manages the database operations required by the monitoring functionality, including persisting changed device properties and relevant alarm information.

There is currently no separate persistence or infrastructure project. Direct SQLite access from Core is intentional for the current architecture.

---

## 10. Services Exposed to Other Components

`ECMS.Core` exposes application services that other components can consume.

`ECMS.Communication` uses these services to expose ECMS functionality through HTTP.

For example, Core may provide services such as:

```csharp
IDeviceService
IAlarmService
```

These services represent application capabilities and are not specific to HTTP.

```text
ECMS.Communication
       │
       │ C# service calls
       ▼
   ECMS.Core
```

Communication translates HTTP requests into service calls and converts their results into HTTP responses. Monitoring and business logic remain in Core.

---

## 11. Initialization

`ECMS.Core` initializes its monitoring state using the configuration provided by `ECMS.Host` and the data persisted in SQLite.

The initialization process must establish the current state of monitored devices before incoming measurements can be compared against it.

The exact initialization and recovery behavior will be defined alongside the device configuration and persistence models.

---

## 12. Error Handling

`ECMS.Core` handles errors related to its application logic, including failures during:

- data processing
- device state management
- alarm evaluation
- database operations
- application service execution

Errors are handled at the appropriate level. Errors that need to be exposed to external systems are translated into suitable HTTP responses by `ECMS.Communication`.

Core does not handle HTTP-specific error representation.

---

## 13. Non-Responsibilities

`ECMS.Core` does not:

- communicate directly with physical devices
- collect device data
- implement device communication protocols
- generate mock device data
- read `./config.json`
- expose HTTP endpoints
- handle HTTP-specific concerns
- manage application startup
- configure dependency injection
- manage the application lifetime

Its role is to process collected data, maintain monitoring state, apply business rules, and persist relevant changes.

---

## 14. Summary

`ECMS.Core` is the central monitoring and business logic layer of ECMS.

It:

1. receives standardized data from `ECMS.Collection`
2. maintains the current state of monitored devices in memory
3. detects changes in incoming device data
4. persists changed properties to SQLite
5. evaluates monitoring conditions and alarms
6. exposes application services to `ECMS.Communication`
7. receives its configuration from `ECMS.Host`

The key question answered by this project is:

> **What does ECMS do with the data it collects?**
