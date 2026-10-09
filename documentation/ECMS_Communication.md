# ECMS.Communication

## 1. Purpose

`ECMS.Communication` exposes ECMS data and functionality to external systems.

Its main responsibility is to provide an external interface to the application while keeping communication-specific concerns separate from the monitoring and business logic implemented in `ECMS.Core`.

> Expose ECMS functionality and data to external systems through HTTP.

---

## 2. Responsibilities

`ECMS.Communication` is responsible for:

- exposing ECMS data and functionality
- receiving HTTP requests
- validating communication-level input
- calling the appropriate services provided by `ECMS.Core`
- converting Core results into HTTP responses
- handling HTTP-specific concerns
- handling communication-level authentication and authorization where required

It is not responsible for:

- collecting data from devices
- communicating with physical devices
- monitoring logic
- alarm evaluation
- data persistence
- reading the application configuration file
- implementing business logic

---

## 3. Configuration Received from ECMS.Host

`ECMS.Host` is responsible for reading the application configuration and providing the required configuration to `ECMS.Communication`.

The configuration is provided as a single configuration object.

The configuration object type is defined in `ECMS.Contracts`.

The configuration object contains the information required by the `ECMS.Communication` layer. For HTTP, this may include information such as:

- listening address
- port
- HTTP settings
- authentication configuration

The exact configuration model will be defined when the HTTP implementation is implemented.

---

## 4. Communication with ECMS.Core

`ECMS.Communication` and `ECMS.Core` run in the same process.

There is therefore no need for a communication protocol between the two projects.

`ECMS.Communication` calls services provided by `ECMS.Core` directly using normal C# method calls.

For example, Core may expose application services such as:

```csharp
public interface IDeviceService
{
    IReadOnlyList<DeviceStatus> GetDevices();
    DeviceStatus? GetDevice(int id);
}
```

or:

```csharp
public interface IAlarmService
{
    IReadOnlyList<Alarm> GetActiveAlarms();
}
```

These services represent **Core functionality**, not HTTP functionality. They are not interfaces specifically designed to reproduce the Communication layer.

The relationship is therefore:

```text
External System
       │
       │ HTTP
       ▼
ECMS.Communication
       │
       │ C# service calls
       ▼
ECMS.Core
```

For example:

```text
HTTP Request
     │
     ▼
Communication endpoint
     │
     │ IDeviceService.GetDevice(...)
     ▼
ECMS.Core
     │
     ▼
Result
     │
     ▼
HTTP Response
```

This keeps the HTTP implementation in `ECMS.Communication` while the actual application functionality remains in `ECMS.Core`.

---

## 5. External Communication

The current and only external communication protocol is HTTP.

`ECMS.Communication` exposes ECMS functionality through HTTP endpoints.



Other external protocols may be added in the future if required.

---

## 6. Error Handling

`ECMS.Communication` translates Core results and errors into appropriate HTTP responses.

For example:

- successful Core operation → appropriate HTTP success response
- requested resource not found → appropriate HTTP error response
- invalid HTTP input → client error response
- Core/application error → appropriate HTTP error response

The HTTP representation of errors belongs to `ECMS.Communication`.

The underlying business logic remains in `ECMS.Core`.

---

## 7. Authentication and Authorization

Authentication and authorization related to external HTTP requests are handled by `ECMS.Communication`.

For protected endpoints, Communication determines whether the external request is authenticated and authorized to access the requested functionality.

Authorization that depends on application or monitoring business rules may require cooperation with `ECMS.Core`.

The exact authentication and authorization mechanism will be defined during the HTTP implementation.

---

## 8. Non-Responsibilities

`ECMS.Communication` does not:

- communicate with physical devices
- collect device data
- generate mock device data
- implement monitoring logic
- evaluate alarms
- manage persistence
- access SQLite directly
- read the application configuration file
- implement Core business logic
- define how collected data is processed

Its role is to provide an external interface to functionality managed by the rest of the application.

---

## 9. Summary

`ECMS.Communication` is the external interface of ECMS.

It:

1. receives HTTP requests
2. exposes ECMS data and functionality through HTTP
3. calls services provided by `ECMS.Core`
4. converts Core results into HTTP responses
5. handles HTTP-specific concerns
6. receives its configuration from `ECMS.Host`
7. keeps external communication concerns separate from Core application logic

The resulting flow is:

```text
External System
       │
       │ HTTP
       ▼
ECMS.Communication
       │
       │ C# service calls
       ▼
ECMS.Core
```

The key question answered by this project is:

> **How do external systems access ECMS?**
