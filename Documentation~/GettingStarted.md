# Getting started

## Project assets

Settings are created in the consuming Unity project, not in the package repository:

```text
Assets/Settings/ComponentAutoBindTool/AutoBindGlobalSetting.asset
Assets/Settings/ComponentAutoBindTool/AutoBindKeyMapSetting.asset
```

The global setting controls the default namespace and generated-code directory. The default directory is:

```text
Assets/Generated/ComponentAutoBindTool
```

Enable **Use Target Script Folder** when each target should generate beside its target script. The resolved directory must remain under `Assets`.

## Hierarchy naming

The scanner splits each child GameObject name on `_`. Every segment before the last one is a component key, and the last segment is the binding suffix.

Examples:

```text
Btn_Submit       -> UnityEngine.UI.Button, field submit
Txt_Status       -> UnityEngine.UI.Text, field status
Rtf_Content      -> UnityEngine.RectTransform, field content
Btn_Txt_Submit   -> Button and Text, fields submit and submit
```

The generated field name removes underscores and lowercases the first character. Duplicate generated names are validation errors because they would create duplicate C# members.

## Generated code

Generation is deterministic: it has no timestamp and writes UTF-8 without a BOM. A generated file follows this shape:

```csharp
public partial class MyView : IAutoBindHost
{
    [Serializable]
    public class UIView : IUiViewComponent
    {
        public Button submit;
    }

    private UIView view;
    public void EnsureAutoBind(GameObject go) { ... }
}
```

Commit generated files in the consumer project. They are not part of this package because they depend on that project's target scripts and scene hierarchy.

## VContainer View templates

When the optional VContainer ViewCore package and VContainer are installed,
right-click the Project window and choose **Create > C# Scripts VContainer
View**. Enter `AutoBindTest` or `AutoBindTestView` in the modal creation window
to preview and create:

```text
AutoBindTestView.cs
AutoBindTestPresenter.cs
```

The command uses the namespace from the single `AutoBindGlobalSetting` asset
when configured. Clicking **Create** writes only the two template scripts; it
does not create a Project rename placeholder or temporary script. It does not
overwrite an existing View or Presenter.

The initial files compile before `AutoBindTestView.BindComponents.cs` exists.
After the View is attached and configured, run Auto Bind and Generate Code as
usual. The generated `UIView` and `view` members hide the empty placeholders
inherited from `ViewLifetimeScope`.

## Custom component mappings

Use the `Add Component To Auto Bind Key Map` context-menu command on a `MonoBehaviour`, or edit the key-map asset directly. A mapping must resolve to a concrete `UnityEngine.Component` type. Keys cannot contain underscores or whitespace because the scanner uses underscores as separators.

## Package boundaries

The core package contains only the reusable runtime component, editor workflow, default UGUI mappings, tests, and a minimal sample. The optional `com.jasmineyamo.component-autobind.vcontainer-viewcore` companion package supplies `ViewLifetimeScope`, `ViewBundle`, and the paired View/Presenter creation menu. Consumer settings, generated bindings, project-specific view management, and project-only dependencies stay outside both packages.
