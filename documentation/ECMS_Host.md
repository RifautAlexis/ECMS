# ECMS.Host

## 1. Purpose

`ECMS.Host` is the entry point of the ECMS application.

It is responsible for starting the application, loading its configuration, creating the required components, and wiring the ECMS projects together.

> Start and configure the ECMS application.

---

## 2. Responsibilities

`ECMS.Host` is responsible for:

- starting the application
- loading application configuration
- creating configuration objects for the different ECMS projects
- registering and configuring services
- configuring dependency injection
- starting and managing the application lifetime
- connecting the main ECMS components together

`ECMS.Host` acts as the **composition root** of the application.

---

## 3. Application Configuration

`ECMS.Host` reads the application configuration from:

```text
./config.json
```

The configuration is used to create the configuration objects required by the different ECMS projects.

The expected structure and contents of `config.json` are described in [Configuration File](#4-configuration-file).

The configuration object types are defined in `ECMS.Contracts`.

The individual projects therefore do not need to read `config.json` directly themselves.

---

## 4. Configuration File

The ECMS application configuration is stored in:

```text
./config.json
```

The file contains the configuration required by the different ECMS components.

The expected top-level structure is:

{
"collection": {
    "pollingIntervalSeconds": 1,
    "devicesConfiguration": [
        {
            "id": 1,
            "kind": 1
        },
        {
            "id": 2,
            "kind": 3
        },
        {
            "id": 3,
            "kind": 3
        },
        {
            "id": 4,
            "kind": 2
        },
    ]
  },
  "core": {},
  "communication": {}
}
}

Each section contains the configuration required by the corresponding project:

- `collection` — configuration for `ECMS.Collection`

- `core` — configuration for `ECMS.Core`

- `communication` — configuration for `ECMS.Communication`

`ECMS.Host` is responsible for reading these sections and creating the corresponding configuration objects defined in `ECMS.Contracts`.

---

## 4. Dependency Injection

`ECMS.Host` is responsible for configuring the dependency injection container and registering the services required by the application.

The Host connects the concrete implementations of the different projects and makes them available to the components that consume them.

The main project dependencies are:

```text
ECMS.Host
   │
   ├── ECMS.Collection ──────► ECMS.Contracts
   │
   ├── ECMS.Core ────────────► ECMS.Contracts
   │
   └── ECMS.Communication ───► ECMS.Core
                                │
                                ▼
                           ECMS.Contracts
```

`ECMS.Communication` uses services provided by `ECMS.Core` through dependency injection.

`ECMS.Collection` and `ECMS.Core` communicate through the collection channel configured by the Host.

The Host therefore acts as the place where the application dependencies are assembled. Individual projects should not be responsible for creating their own dependencies.

---

## 5. Application Startup

`ECMS.Host` starts the different application components and manages their lifetime.

The application is hosted as a .NET Worker Service.

The Host is responsible for creating the application environment in which it can operate.

```text
ECMS.Collection
       │
       │ collected data
       ▼
ECMS.Core
       │
       │ services
       ▼
ECMS.Communication
```

---

## 6. Project Dependencies

`ECMS.Host` is the composition root and may reference the other ECMS projects in order to configure and start them.

The other projects should not depend on `ECMS.Host`.

`ECMS.Host` is therefore responsible for assembling the application rather than implementing its business functionality.

---

## 7. Non-Responsibilities

`ECMS.Host` does not:

- collect device data
- communicate with physical devices
- implement monitoring logic
- evaluate alarms
- implement business logic
- provide HTTP endpoints
- manage device-specific data processing
- implement database operations

Its role is to **configure, compose, start, and manage the application**.

---

## 8. Summary

`ECMS.Host` is the entry point and composition root of ECMS.

It:

1. starts the application
2. reads `./config.json`
3. creates project-specific configuration objects
4. configures dependency injection
5. wires the different ECMS components together
6. manages the application lifetime

The key question answered by this project is:

> **How is the ECMS application started and managed?**
