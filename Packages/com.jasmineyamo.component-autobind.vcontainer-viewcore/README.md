# Component Auto Bind - VContainer ViewCore

This optional companion package connects Component Auto Bind with VContainer.

Install the core package first, then install this package. VContainer is intentionally
not declared in `package.json` because Unity Package Manager requires dependencies to
use a specific version. Install any compatible release of
`jp.hadashikick.vcontainer` separately.

When VContainer is unavailable, the integration runtime assembly is skipped and the
Unity Console reports one warning per Editor session. The core Component Auto Bind
package remains available.

The package exposes `ViewLifetimeScope` and `ViewBundle` in the
`JasmineYamo.ComponentAutoBind.ViewCore` namespace.

## Create a View and Presenter

With VContainer installed, right-click an empty area in the Project window and
choose **Create > C# Scripts VContainer View**. A modal creation window accepts
either a base name such as `AutoBindTest` or a View name such as
`AutoBindTestView` and previews both script names:

```text
AutoBindTestView.cs
AutoBindTestPresenter.cs
```

The View registers the Presenter with `RegisterCommon`, while the Presenter
receives the concrete `AutoBindTestView.UIView` and the shared `ViewBundle`
through constructor injection. If the global Component Auto Bind setting has a
namespace, both scripts use it.

The two scripts compile before binding code exists because `ViewLifetimeScope`
supplies empty inherited `UIView` and `view` placeholders. Generate bindings in
the normal Component Auto Bind workflow; the generated nested type and field
then hide those placeholders.

Clicking **Create** writes exactly those two scripts from the package templates.
No placeholder or temporary script is created. If either target file already
exists, neither file is created or overwritten. An unexpected write or import
failure rolls back only files created by that attempt. Success and errors are
written directly to the Unity Console without another dialog. When VContainer
is missing, the menu remains visible but disabled.
