# Simple UI - VContainer

`com.jasmineyamo.simple-ui-vcontainer` combines Component Auto Bind with a
small VContainer-based UI navigation runtime.

Install `com.jasmineyamo.component-autobind` first, then install this package
and any compatible `jp.hadashikick.vcontainer` release. VContainer is
intentionally not declared in `package.json` because Unity Package Manager
requires a specific dependency version.

When VContainer is unavailable, the Runtime assembly is skipped, the creation
menu is disabled, and the Unity Console reports one warning per Editor session.

## Runtime setup

The public API is in `JasmineYamo.SimpleUI.VContainer`. Register a project
implementation of `IViewPrefabHelper`, then register the framework:

```csharp
protected override void Configure(IContainerBuilder builder)
{
    builder.Register<IViewPrefabHelper, ProjectViewPrefabHelper>(
        Lifetime.Singleton);
    builder.RegisterSimpleUI(this, viewRoot);
}
```

`ViewManager` maintains independent HUD, Normal, Top, and System stacks.
Opening a View pauses only the previous View on the same layer. Closing the
top resumes the next View on that layer. Non-immediate opens wait until their
target layer is empty.

`DestroyType.NonDestroy` caches closed instances for reuse.
`DestroyType.DelayDestroy` caches a closed instance until its configured
delay expires; reopening it before that deadline cancels destruction.

## Create a View and Presenter

Right-click the Project window and choose
**Create > C# Scripts VContainer View**. The modal window accepts a base name
or a name ending in `View` and previews the two scripts it will create.

The generated View inherits `ViewLifetimeScope`. The generated Presenter
receives the concrete nested `UIView` and the same `ViewBundle` through
constructor injection. Both scripts use the namespace from the single
`AutoBindGlobalSetting` asset when one is configured.

Creation writes exactly two scripts with `FileMode.CreateNew`. It never
overwrites an existing target, creates no placeholder script, and reports
success or failure directly to the Console.

## Resources Demo

Import **Resources Demo** from this package's Samples page. It includes:

- `ResourcesViewPrefabHelper`, loading `Resources/Views/{ViewName}`.
- A Bootstrap LifetimeScope using `RegisterSimpleUI()`.
- Home and Detail Views with generated bindings and Presenters.
- A runnable scene and PlayMode smoke test.

The Resources loader is sample-owned so projects can replace it without adding
an Addressables dependency to the package Runtime. An Addressables adapter must
load View prefabs asynchronously before `ShowView` can run, retain the handles,
and expose only a synchronous in-memory lookup from `IViewPrefabHelper`.

Do not call `WaitForCompletion`, `.Result`, or any equivalent blocking wait from
`GetViewPrefab`. The project adapter that owns the preload cache also owns handle
release and must fail with a diagnostic that names the missing View when lookup
occurs before preload has completed.
