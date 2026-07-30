# Resources Demo

This Sample demonstrates the Runtime `ViewManager` with synchronous Resources
loading. Unity copies it into the project's `Assets/Samples` folder, so the
loader and demo assets can be modified by the project.

## Run the demo

1. Install any compatible `jp.hadashikick.vcontainer` version. Development
   validation uses VContainer `1.17.0`.
2. Open `Demo/ViewManagerSample.unity`.
3. Enter Play Mode and use the launcher buttons.

`ResourcesViewPrefabHelper` loads `HomeView` and `DetailView` from
`Demo/Resources/Views/{ViewName}`. Both prefabs include generated binding code.

Create additional View and Presenter pairs with
**Create > C# Scripts VContainer View**, place the configured View prefab below
a `Resources/Views` folder, and keep its prefab name equal to the View name
passed to `IViewManager`.
