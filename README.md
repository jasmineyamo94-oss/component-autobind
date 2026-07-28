# Component Auto Bind

`com.jasmineyamo.component-autobind` is a small Unity package that scans a GameObject hierarchy, stores component references, and generates a strongly typed component set for a target `MonoBehaviour`.

The package is intended for Unity `2022.3` LTS and uses the built-in UGUI component types for its default key map. TextMeshPro and dependency-injection integrations are deliberately outside the core package.

## Install

In Unity Package Manager, choose **Add package from git URL** and enter:

```text
https://github.com/jasmineyamo94-oss/component-autobind.git?path=/Packages/com.jasmineyamo.component-autobind#v0.1.0
```

The repository is public, so no credentials or GitHub package registry setup is required.

## Quick start

1. Open `Tools/Component Auto Bind/Create Global Settings`.
2. Open `Tools/Component Auto Bind/Create Key Map Settings`.
3. Add `ComponentAutoBindTool` to the same GameObject as the target `MonoBehaviour`.
4. Assign the target script in the Inspector.
5. Name child objects with a component key and suffix, for example `Btn_Submit` or `Txt_Status`.
6. Click `Auto Bind` to scan the hierarchy and review the validation report.
7. Click `Generate Code` after validation passes.

Generated files are written to `Assets/Generated/ComponentAutoBindTool` by default. Generated files are project source and should be committed to the consumer project.

## Default keys

The initial key map includes `Tf`, `OAni`, `NAni`, `Rtf`, `Cav`, `CGroup`, `VLGroup`, `HLGroup`, `GLGroup`, `TGroup`, `Btn`, `Img`, `RImg`, `Txt`, `Inf`, `Sld`, `Mask`, `Mask2D`, `Tog`, `Sbr`, `SRect`, and `Drop`.

For a name such as `Btn_Submit`, the scanner resolves `Btn` to `UnityEngine.UI.Button` and generates the field `submit`. Multiple component prefixes are supported, for example `Btn_Txt_Submit` creates both bindings using the same suffix.

Custom mappings are stored in the consumer project asset at `Assets/Settings/ComponentAutoBindTool/AutoBindKeyMapSetting.asset`. They store a key and a component type name; no project assembly is copied into this package.

## Runtime contracts

The runtime assembly is `JasmineYamo.ComponentAutoBind` and exposes:

- `ComponentAutoBindTool` for serialized component references and typed lookups.
- `IAutoBindTarget` for generated target initialization.
- `IAutoBindComponentSet` as the marker contract for generated component sets.

The editor assembly is separate and is included only in the Unity Editor. The generator emits a partial target class, a nested `AutoBindComponentSet`, and an `EnsureAutoBind(GameObject)` implementation.

## Sample and tests

Import **Basic UGUI** from the package Samples tab to try a code-only sample. It creates a Canvas, a Button, and a legacy UGUI Text component at runtime, so it does not depend on a scene asset or TextMeshPro.

The package includes Runtime and Editor tests under `Tests/`. Run them from Unity Test Runner with `UNITY_INCLUDE_TESTS` enabled.

## Scope

This package intentionally does not contain VContainer adapters, ViewCore interfaces, generated game-project bindings, project settings assets, old example scenes, or third-party generic dictionary code. An integration package can depend on this core package later without coupling the core runtime to a specific DI framework.

See [Documentation~/GettingStarted.md](Documentation~/GettingStarted.md) for configuration details and [CHANGELOG.md](CHANGELOG.md) for release history.

参考库：[CatImmortal/ComponentAutoBindTool](https://github.com/CatImmortal/ComponentAutoBindTool)
