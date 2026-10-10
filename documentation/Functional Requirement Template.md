# Functional Requirement Template

A functional requirement describe the behaviours and rules that define one or capabilities.



Below is the functional analysis template to use.

```markdown
# FR-XXX-001 — Requirement name

**Requirement**

Describe what the system must do, using precise and unambiguous language.

**Acceptance criteria**

- Given a specific initial condition,
- When a specific action or event occurs,
- Then the system produces the expected observable result..
```

## Tips

| Optional field | When to use it                                                                   |
| -------------- | -------------------------------------------------------------------------------- |
| Business rules | When domain-specific rules determine the expected behaviour.                     |
| Exceptions     | When the requirement has important failure or edge cases.                        |
| Constraints    | When a technical, regulatory, or project constraint limits acceptable solutions. |
| References     | When the requirement depends on another requirement or external specification.   |
| Open questions | When a decision is unresolved and must not be silently assumed.                  |

## Example

```markdown
### FR-003-004 — Update changed device properties

**Requirement**

When new data is received for a monitored device, ECMS must update the current device state for each property whose new value differs from its current value.

**Acceptance criteria**

- Given a device whose current temperature is `25`, when new data contains a temperature of `30`, then the current temperature becomes `30`.
- Given a device whose current temperature is `25`, when new data contains a temperature of `25`, then the current temperature remains unchanged.
- Given a device with several properties, when only some property values change, then only those properties are updated in the current device state.
```
