# ECMS.Contracts

## 1. Purpose

`ECMS.Contracts` contains the shared data contracts used by the different ECMS projects.

Its main responsibility is to provide common data definitions without containing application logic or implementation details.

> Define the data structures shared between ECMS components.

---

## 2. Responsibilities

`ECMS.Contracts` contains shared definitions such as:

- collected device data
- device-specific data structures
- configuration objects
- request and response contracts when shared between projects

The contracts are used by the projects that need to exchange or consume the corresponding data.

`ECMS.Contracts` should remain focused on **data definitions** and should not contain business logic.

---

## 3. Configuration Contracts

Configuration object types used to pass configuration between `ECMS.Host` and other projects are defined in `ECMS.Contracts`.

For example:

```text
ECMS.Contracts
└── CollectionConfiguration
```

---

## 4. Collected Data Contracts

`ECMS.Contracts` defines the standardized data format produced by `ECMS.Collection`.

The common data envelope identifies the device and its type while containing the device-specific data.

Example:

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
- `data` — contains the type-specific data

This allows `ECMS.Collection` to provide a consistent format to `ECMS.Core`.

---

## 5. Organization

The exact organization may evolve as the application grows.

The current structure is :

```text
ECMS.Contracts
├── CollectedData
├── CollectionConfiguration
├── CommunicationtionConfiguration
├── CorectionConfiguration
├── DeviceConfiguration
└── Devices
    ├── RfPowerSensorData
    ├── RfPowerAmplifierData
    ├── RfMonitoringSystemData
    └── RfMonitoringChannelData
```

The individual contracts are intentionally not documented here. Their properties and semantics should be self-explanatory from the code and from the specifications of the corresponding functionality.

---

## 6. Non-Responsibilities

`ECMS.Contracts` does not:

- implement business logic
- perform data processing
- access the database
- communicate with devices
- handle HTTP communication
- load configuration
- manage application lifecycle
- contain service implementations

It defines **what data looks like**, not **what the application does with that data**.

---

## 7. Summary

`ECMS.Contracts` is the shared definition layer of ECMS.

It provides common:

- data contracts
- device data structures
- configuration object types
- other shared request/response contracts when required

The key question answered by this project is:

> **What data structures are shared between ECMS components?**
