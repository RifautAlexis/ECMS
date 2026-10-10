# ECMS — Project Responsibilities and Architecture

## 1. Purpose

This document describes the responsibilities and boundaries of the projects composing the ECMS solution.

The architecture separates the application into four main responsibilities:

1. starting and managing the application
2. collecting and standardizing data from monitored devices
3. processing and storing the collected data and evaluating alarms
4. exposing ECMS data and functionality to external systems

The solution is composed of five projects:

```text
ECMS.sln

├── ECMS.Host
├── ECMS.Collection
├── ECMS.Core
├── ECMS.Communication
└── ECMS.Contracts
```

---

# 2. ECMS.Host

## Responsibility

`ECMS.Host` is the entry point and runtime host of ECMS.

It starts the application and initializes the different components required for the monitoring system to operate.

`ECMS.Host` is implemented as a .NET Worker Service and is intended to run as a long-running process on the embedded Linux system.

## Main responsibilities

`ECMS.Host` is responsible for:

- starting the ECMS application
- configuring dependency injection
- loading application configuration
- configuring logging
- registering application components
- starting and stopping the different services
- managing the application lifecycle

`ECMS.Host` should contain as little application logic as possible. Its primary role is to assemble and run the other components.

## Application flow

```text
ECMS.Host
    │
    ├── starts ECMS.Collection
    ├── starts ECMS.Core
    └── starts ECMS.Communication
```

---

# 3. ECMS.Collection

## Responsibility

`ECMS.Collection` is responsible for collecting data from the devices monitored by ECMS.

Devices may provide data through different communication mechanisms and in different formats. The Collection layer handles these device-specific details and converts the collected information into a standardized ECMS data representation.

The standardized data is then sent to `ECMS.Core` for processing.

## Main responsibilities

`ECMS.Collection` is responsible for:

- identifying the devices registered in ECMS
- communicating with monitored devices
- collecting device data
- handling device-specific protocols and formats
- interpreting raw device data
- standardizing collected data
- sending standardized data to `ECMS.Core`

## Data flow

```text
Monitored Device
       │
       ▼
Device-specific communication
       │
       ▼
Raw device data
       │
       ▼
Interpretation
       │
       ▼
Standardized ECMS data
       │
       ▼
ECMS.Core
```

The Collection layer should not contain monitoring business rules such as alarm evaluation.

Its responsibility is to answer:

> How do we collect and standardize data from this device?

---

# 4. ECMS.Core

## Responsibility

`ECMS.Core` is responsible for processing the standardized data received from `ECMS.Collection`.

It contains the central monitoring logic of ECMS.

When new standardized data is received, Core processes the data, updates the corresponding monitoring state, stores the data in the database and evaluates the alarms enabled for the affected device or data.

## Main responsibilities

`ECMS.Core` is responsible for:

- receiving standardized data from `ECMS.Collection`
- processing collected data
- updating device and monitoring state
- writing monitoring data to the database
- evaluating enabled alarms
- creating, updating or clearing alarm states
- providing the data and operations required by `ECMS.Communication`

## Data flow

```text
ECMS.Collection
       │
       │ Standardized data
       ▼
   ECMS.Core
       │
       ├── Process data
       │
       ├── Update state
       │
       ├── Write data to database
       │
       └── Check enabled alarms
```

The Core should contain the logic that defines **what ECMS does with collected data**.

It should not contain protocol-specific implementation for external communication such as HTTP or SNMP.

---

# 5. ECMS.Communication

## Responsibility

`ECMS.Communication` is responsible for exposing ECMS data and functionality to external systems.

It provides the communication layer between ECMS and systems outside the embedded application.

The initial implementation will expose an HTTP API. Other communication protocols may be added in the future.

## Main responsibilities

`ECMS.Communication` is responsible for:

- exposing ECMS data through an API
- receiving requests from external systems
- translating protocol-specific requests into ECMS operations
- retrieving data managed by `ECMS.Core`
- returning protocol-specific responses
- handling communication-specific concerns

## Initial protocol

The first implementation will support:

```text
HTTP
```

Future implementations may support additional protocols, such as:

```text
HTTP
SNMP
...
```

## Communication flow

```text
External System
       │
       │ HTTP
       ▼
ECMS.Communication
       │
       │ Request / operation
       ▼
ECMS.Core
       │
       │ Data
       ▼
ECMS.Communication
       │
       │ HTTP response
       ▼
External System
```

`ECMS.Communication` should not implement monitoring business rules.

For example, an HTTP endpoint should not independently determine whether a measurement triggers an alarm. It should delegate this responsibility to `ECMS.Core`.

---

# 6. Overall Data Flow

The main application flow follows the order of the projects:

```text
┌─────────────┐
│ ECMS.Host   │
│             │
│ Start       │
│ application │
└──────┬──────┘
       │
       ▼
┌─────────────────┐
│ ECMS.Collection │
│                 │
│ Collect data    │
│ Standardize     │
└──────┬──────────┘
       │
       │ Standardized data
       ▼
┌─────────────┐
│ ECMS.Core   │
│             │
│ Process     │
│ Persist     │
│ Check alarms│
└──────┬──────┘
       │
       │ Data / operations
       ▼
┌──────────────────────┐
│ ECMS.Communication   │
│                      │
│ Expose through API   │
└──────────┬───────────┘
           │
           ▼
    External Systems
```

This creates a clear separation of responsibilities:

| Project              | Main question it answers                           |
| -------------------- | -------------------------------------------------- |
| `ECMS.Host`          | **How is the application started and managed?**    |
| `ECMS.Collection`    | **How do we collect and standardize device data?** |
| `ECMS.Core`          | **What do we do with the collected data?**         |
| `ECMS.Communication` | **How do external systems access ECMS?**           |

The architecture therefore follows the natural flow of the application:

**Start → Collect → Process → Expose**
