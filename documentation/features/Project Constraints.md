# Project Constraints

This document defines the constraints and established decisions that apply across the ECMS project.

These constraints guide the design and implementation of the application. They complement the functional requirements documented in the feature catalogue and should not be duplicated unnecessarily in individual feature documents.

A feature may reference a constraint by its identifier when that constraint affects its requirements or acceptance criteria.

## PC-001 — Supported operating systems

**Requirement**

ECMS must support both Windows and Linux.

**Acceptance criteria**

- Given a supported Windows environment, when ECMS is deployed and started, then the application must operate correctly.
- Given a supported Linux environment, when ECMS is deployed and started, then the application must operate correctly.

## PC-002 — Cross-platform file paths

**Requirement**

File paths must be compatible with both supported operating systems.

Application behavior must not depend on operating-system-specific path separators.

**Acceptance criteria**

- Given a supported Windows environment, when ECMS accesses a file using its configured or defined path, then the path must be handled correctly.
- Given a supported Linux environment, when ECMS accesses a file using its configured or defined path, then the path must be handled correctly.

## PC-003 — Operating-system file permissions

**Requirement**

ECMS must handle file-access permissions and access failures correctly on both supported operating systems.

**Acceptance criteria**

- Given that ECMS does not have sufficient permissions to access a required file, when it attempts to access that file, then the application must handle the failure according to the relevant functional requirements.
- Given that file-access permissions differ between Windows and Linux, when ECMS accesses a file in either environment, then it must use the operating system's supported file-access mechanisms without assuming identical permission models.