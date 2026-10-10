# Configuration file

This document describes the properties defined in `config.json`, which is read by the application during startup.



The configuration file is located at `./config.json`, relative to the application's working directory.



At startup, `ECMS` reads and validates the configuration file. If the file contains an invalid structure or invalid data, the application will fail to start and report an error. If the configuration is valid, the application loads the configuration values and uses them to initialize and run the application accordingly.



## File Structure & Description

The following example illustrates the structure of the configuration file :

```json
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
        "kind": 2
      },
      {
        "id": 3,
        "kind": 3
      },
      {
        "id": 4,
        "kind": 3
      },
      {
        "id": 5,
        "kind": 1
      },
      {
        "id": 6,
        "kind": 2
      }
    ]
  },
  "core": {},
  "communication": {}
}

```

The configuration file contains three top-level properties: `collection`, `core`, and `communication`.

Each property defines the configuration for a specific component of `ECMS`.



### Collection

The `collection` section configures the component responsible for collecting and standardizing data from devices.

| Property               | Value Type                     | Description                                                         |
| ---------------------- | ------------------------------ | ------------------------------------------------------------------- |
| pollingIntervalSeconds | Integer                        | Interval, in seconds, between successive device polling cycles.     |
| devicesConfiguration   | Array of `DeviceConfiguration` | List of devices to simulate when physical equipment is unavailable. |



#### DeviceConfiguration

Each `DeviceConfiguration` entry defines a device to simulate when physical equipment is unavailable.

| Property | Value Type | Description                               |
| -------- | ---------- | ----------------------------------------- |
| id       | Integer    | Unique identifier assigned to the device. |
| kind     | Integer    | Type of device to simulate.               |



The `kind` property must contain one of the following values :

| Value | Equipment Name       |
| ----- | -------------------- |
| 1     | RF Power Sensor      |
| 2     | RF Power Amplifier   |
| 3     | RF Monitoring System |



### Core

The `core` section configures the component responsible for processing standardized data received from `collection`, persisting device data in the database, and managing alarms.

| Property | Value Type | Description               |
| -------- | ---------- | ------------------------- |
| TBD      | TBD        | Properties to be defined. |



### Communication

The `communication` section configures the component responsible for handling requests from the user interface (UI) and external systems.

| Property | Value Type | Description               |
| -------- | ---------- | ------------------------- |
| TBD      | TBD        | Properties to be defined. |
