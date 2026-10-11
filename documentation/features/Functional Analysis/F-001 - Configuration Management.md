# F-001 — Configuration Management

## Description

ECMS loads its configuration from `config.json`, located in the same directory as the application executable. Its location must not depend on the process's current working directory.

The application validates the configuration structure and its data before starting normal operation. If the configuration is missing, unreadable, structurally invalid, or contains invalid data, ECMS must report the error and must not start normal operation.

The configuration provides the settings required by the application's components, including device collection and device availability management.

The configuration file contains the following root-level properties:
- `collection`: device collection settings and monitored device definitions.
- `core`: core component configuration, currently an empty object.
- `communication`: communication component configuration, currently an empty object.

All three root-level properties are mandatory. The `collection` property must be an object. The `core` and `communication` properties must also be objects and must be present even when they contain no properties.

ECMS must comply with the cross-platform requirements defined in the Project Constraints document, particularly PC-001 — Supported operating systems, PC-002 — Cross-platform file paths, and PC-003 — Operating-system file permissions.

## Functional requirements

<details>
<summary>FR-001-001 — Load the configuration file</summary>

**Requirement**

ECMS must load `config.json` from the same directory as the application executable during startup, regardless of the process's current working directory.

**Acceptance criteria**

- Given that config.json exists and is readable in the executable's directory, when ECMS starts, then it loads the file for validation.
- Given that ECMS is launched with a working directory different from the executable's directory, when ECMS starts, then it still resolves config.json relative to the executable's directory.
- Given that config.json does not exist in the executable's directory, when ECMS starts, then it reports an error and does not start normal operation.
- Given that config.json cannot be read, when ECMS starts, then it reports an error and does not start normal operation.

</details>

<details>
<summary>FR-001-002 — Validate the configuration structure</summary>

**Requirement**

ECMS must validate that the configuration file conforms to the expected configuration structure.

The root-level properties `collection`, `core`, and `communication` must all be present.

The `core` and `communication` objects may be empty.

**Acceptance criteria**

- Given a configuration containing all mandatory root-level properties with the expected structure, when structural validation is performed, then the structure passes validation.
- Given a configuration missing any mandatory root-level property, when structural validation is performed, then the configuration is rejected and an error is reported.
- Given a configuration in which collection, core, or communication is not an object, when structural validation is performed, then the configuration is rejected and an error is reported.
- Given empty objects for both core and communication, when structural validation is performed, then these properties are accepted.
- Given a configuration file that cannot be parsed, when loading or validation is performed, then the configuration is rejected and an error is reported.

</details>

<details>
<summary>FR-001-003 — Validate configuration data</summary>

**Requirement**

ECMS must validate configuration values against their expected data types and applicable constraints.

A structurally valid configuration must not be accepted if its values are invalid.

**Acceptance criteria**

- Given a configuration containing valid values that satisfy all applicable requirements, when data validation is performed, then the values pass validation.
- Given a configuration containing a value of an unexpected type or a value that violates an applicable constraint, when data validation is performed, then the configuration is rejected and an error is reported.
- Given a configuration containing multiple independently detectable invalid values, when data validation is performed, then ECMS reports the errors according to FR-001-009.

</details>

<details>
<summary>FR-001-004 — Prevent normal startup with an invalid configuration</summary>

**Requirement**

ECMS must not start normal operation if its configuration is missing, unreadable, structurally invalid, or contains invalid data.

**Acceptance criteria**

- Given a missing, unreadable, or invalid configuration, when ECMS starts, then it reports the configuration error and does not begin normal operation.
- Given a configuration that passes all applicable validation, when ECMS starts, then it may proceed with application startup.
- Given a configuration validation failure, when startup is attempted, then application components must not begin normal operation using that invalid configuration.

</details>

<details>
<summary>FR-001-005 — Configure the device collection interval</summary>

**Requirement**

The configuration must provide `collection.pollingIntervalSeconds`, which defines the interval between device collection cycles.

The value must be a positive integer.

**Acceptance criteria**

- Given a positive integer polling interval, when the configuration is validated, then the value is accepted.
- Given a missing, non-integer, zero, or negative polling interval, when the configuration is validated, then the configuration is rejected and an error is reported.

</details>

<details>
<summary>FR-001-006 — Configure the monitored devices</summary>

**Requirement**

The configuration must provide `collection.devicesConfiguration`, an array defining the devices to be monitored.

Each entry must identify a device using its `id` and `kind`.

Device IDs must be unique across the array. Multiple entries may share the same `kind`.

The array may be empty.

**Acceptance criteria**

- Given valid device definitions with unique IDs, when the configuration is validated, then the definitions are accepted.
- Given two or more entries with the same device ID, when the configuration is validated, then the configuration is rejected and the duplicated ID is reported.
- Given multiple configured devices with different IDs but the same `kind`, when the configuration is validated, then the repeated kind is accepted.
- Given an empty `devicesConfiguration` array, when the configuration is validated, then the array is accepted, provided all other applicable requirements are satisfied.
- Given a device entry missing a required field or containing a value that violates an applicable validation rule, when the configuration is validated, then the configuration is rejected and an error is reported.

</details>

<details>
<summary>FR-001-007 — Configure the device availability failure threshold</summary>

**Requirement**

The configuration must provide `collection.unavailableAfterFailures`, defining the number of consecutive collection failures required for a device to become `Unavailable`.

This setting applies globally to all monitored devices.

The value must be a positive integer.

**Acceptance criteria**

- Given a positive integer value for `collection.unavailableAfterFailures`, when the configuration is validated, then the value is accepted.
- Given a missing, non-integer, zero, or negative value for `collection.unavailableAfterFailures`, when the configuration is validated, then the configuration is rejected and an error is reported.
- Given a valid threshold, when ECMS manages device availability, then the same configured threshold applies to every monitored device.

</details>

<details>
<summary>FR-001-008 — Make validated configuration available to the application</summary>

**Requirement**

After successful validation, ECMS must make the validated configuration available to the application components that require it.

**Acceptance criteria**

- Given a configuration that passes all validation, when application startup proceeds, then the relevant components can access their configured settings.
- Given a configuration that fails validation, when application startup is attempted, then application components must not begin normal operation using that invalid configuration.

</details>

<details>
<summary>FR-001-009 — Report configuration errors</summary>

**Requirement**

When ECMS encounters a configuration error, it must report sufficient information to identify the problem and support corrective action.

Where applicable and available, the error report must include :
- The configuration file path.
- The affected property or configuration entry.
- A description of the problem.
- The expected value, type, or constraint.
- The actual value, when available and useful.
- The line and column of the error, when reliably available.
- A clear indication that normal startup has been prevented.

When duplicate device IDs are detected, the error report must identify the duplicated ID.

ECMS should report multiple independent configuration errors in one validation attempt when practical. A file-reading or parsing error that prevents further validation may be reported on its own.

**Acceptance criteria**

- Given a configuration containing an invalid value, when validation fails, then the error report identifies the affected property and describes the validation failure.
- Given duplicate device IDs, when validation fails, then the error report identifies the duplicated ID.
- Given a configuration file that cannot be read or parsed, when loading or validation fails, then the error report describes the failure and identifies the file where possible.
- Given any fatal configuration error, when ECMS starts, then the error report makes clear that normal operation will not begin.
- Given multiple independent validation errors that can be detected in the same validation attempt, when validation fails, then ECMS reports those errors together when practical.

</details>

## Open questions

None currently identified for the agreed scope of this feature.

Additional configuration sections and properties may be specified as the application evolves. Their structure, data types, validation rules, and error-reporting requirements must be documented when they are introduced.

## Dependencies

None.

## Dependents

- **F-002 — Device Data Collection:** consumes the polling interval and configured device definitions.
- **F-005 — Device Availability Management:** consumes the global consecutive-failure threshold.