# Functional Analysis Template

A functional analysis describes a capability ECMS provides.



Below is the functional analysis template to use.

```markdown
# F-XXX — Feature name

## Description

Describe the capability provided by the feature and its expected behaviour.

## Functional requirements

<details>
<summary>FR-XXX-001 — Requirement name</summary>

**Requirement**

Describe one specific, unambiguous, and testable behaviour that ECMS must satisfy.

**Acceptance criteria**

- Given that XXX, when YYY, then ZZZ.
- Given that AAA, when BBB, then CCC.

</details>

<details>
<summary>FR-XXX-002 — Requirement name</summary>

**Requirement**

Describe one specific, unambiguous, and testable behaviour that ECMS must satisfy.

**Acceptance criteria**

- Given that XXX, when YYY, then ZZZ.
- Given that AAA, when BBB, then CCC.

</details>

## Acceptance criteria

Define observable conditions that demonstrate that the feature satisfies its functional requirements.

## Open questions

Record unresolved functional decisions that could affect the interpretation or implementation of the requirements.

## Dependencies

Reference other features or requirements that this feature relies on, where applicable.

## Dependents

Reference other features or requirements depend on this feature, where applicable.
```

## Tips

A few points about this template:

- **Description** provides the overall picture without repeating the requirements one by one.

- **Functional requirements** are the core of the document.

- **Acceptance** criteria make verification practical. They can be omitted as a separate section when the requirements themselves are sufficiently testable and the project doesn't need additional scenarios.

- **Open questions** prevent unresolved assumptions from silently becoming requirements.

- **Dependencies** and **Dependents** are useful when relevant, but shouldn't be filled with boilerplate.
