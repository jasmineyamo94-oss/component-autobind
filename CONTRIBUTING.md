# Contributing

This repository is a platform-neutral publisher for two Unity Package Manager
packages. AI-platform state such as `.trellis/`, `.codex/`, `.agents/`, and a
generated `AGENTS.md` is local authoring state and must not be committed.

## Package boundaries

- `com.jasmineyamo.component-autobind` owns runtime binding data, Editor
  hierarchy scanning, validation, deterministic source generation, tests, and
  the Basic UGUI sample. It must not reference VContainer, TextMeshPro, or a
  consumer project assembly.
- `com.jasmineyamo.simple-ui-vcontainer` owns VContainer View registration,
  navigation, lifecycle behavior, Editor templates, tests, and the Resources
  demo. It depends on the core package but intentionally does not pin a
  VContainer version.
- `Samples~` demonstrates replaceable adapters. Sample loaders, scenes, and
  Presenters must not become hidden Runtime dependencies.

Generated `*.BindComponents.cs` files belong to the consumer project. Changes
to generated type shape, `IAutoBindHost`, nested `UIView`, or field naming need
an explicit regeneration or migration note.

## Validation

Run from the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File ./Tools/ValidatePackage.ps1
```

The validator checks manifests, package boundaries, required assemblies and
samples, deterministic Editor behavior, legacy references, release URLs, and
repository hygiene. Unity behavior changes additionally require the matching
Runtime, Editor, or PlayMode tests.

## Release contract

The package manifests are the version source of truth. The two independent tag
streams are:

| Package | Current version | Planned tag |
| --- | --- | --- |
| `com.jasmineyamo.component-autobind` | `0.2.0` | `component-autobind-v0.2.0` |
| `com.jasmineyamo.simple-ui-vcontainer` | `0.1.0` | `simple-ui-vcontainer-v0.1.0` |

Before publishing either tag:

1. Require a clean worktree and a passing validator.
2. Compile and run the relevant tests in a temporary Unity 2022.3 consumer.
3. Confirm the README URL, package path, manifest version, and tag agree.
4. Create an annotated tag on the verified release commit.
5. Push or create a GitHub Release only after explicit release approval.
6. Verify the Git URL in a fresh temporary consumer after the tag is remote.

Two tags may point at the same release commit, but each tag represents only its
named package stream. Never move a published tag; publish a new version instead.
