# Component Auto Bind

`com.jasmineyamo.component-autobind` 是一个轻量级 Unity 包。它能够扫描 GameObject 层级、保存组件引用，并为目标 `MonoBehaviour` 生成强类型的 `UIView`。

该包面向 Unity `2022.3` LTS，默认键映射使用 Unity 内置的 UGUI 组件类型。核心包不包含 TextMeshPro；如需 VContainer 支持，可安装可选的配套包。

## 安装

在 Unity Package Manager 中选择 **Add package from git URL**，然后输入：

```text
https://github.com/jasmineyamo94-oss/component-autobind.git?path=/Packages/com.jasmineyamo.component-autobind#v0.2.0
```

该仓库为公开仓库，无需凭据，也无需配置 GitHub Package Registry。

## 快速开始

1. 打开 `Tools/Component Auto Bind/Create Global Settings`。
2. 打开 `Tools/Component Auto Bind/Create Key Map Settings`。
3. 将 `ComponentAutoBindTool` 添加到目标 `MonoBehaviour` 所在的 GameObject。
4. 在 Inspector 中指定目标脚本。
5. 使用组件键和后缀命名子对象，例如 `Btn_Submit` 或 `Txt_Status`。
6. 点击 `Auto Bind` 扫描层级。
7. 点击 `Generate Code` 进行验证并写入生成的源代码。

生成的文件默认写入 `Assets/Generated/ComponentAutoBindTool`。这些文件属于项目源代码，应提交到使用该包的项目中。两个操作按钮都会直接在 Unity Console 中报告成功、警告和错误，不会弹出确认对话框。

## 默认键

初始键映射包含 `Tf`、`OAni`、`NAni`、`Rtf`、`Cav`、`CGroup`、`VLGroup`、`HLGroup`、`GLGroup`、`TGroup`、`Btn`、`Img`、`RImg`、`Txt`、`Inf`、`Sld`、`Mask`、`Mask2D`、`Tog`、`Sbr`、`SRect` 和 `Drop`。

对于 `Btn_Submit` 这样的名称，扫描器会将 `Btn` 解析为 `UnityEngine.UI.Button`，并生成字段 `submit`。名称支持多个组件前缀，例如 `Btn_Txt_Submit` 会使用同一个后缀创建两个绑定。

自定义映射保存在使用方项目的 `Assets/Settings/ComponentAutoBindTool/AutoBindKeyMapSetting.asset` 资源中。每条映射包含一个键和一个组件类型名称；项目程序集不会被复制到该包中。

## 运行时约定

运行时程序集为 `JasmineYamo.ComponentAutoBind`，它公开：

- `ComponentAutoBindTool`：用于序列化组件引用和类型化查找。
- `IAutoBindHost`：用于初始化生成的目标。
- `IUiViewComponent`：作为生成的 `UIView` 类型的标记约定。

Editor 程序集独立存在，且仅包含在 Unity Editor 中。生成器会生成目标分部类、嵌套的 `UIView`、私有 `view` 字段，以及 `EnsureAutoBind(GameObject)` 实现。

## Simple UI - VContainer

可选包 `com.jasmineyamo.simple-ui-vcontainer` 将 Component Auto Bind、VContainer View 和 ViewManager 整合为一个轻量 UI 框架。它提供 `ViewLifetimeScope`、`ViewBundle`、Presenter EntryPoint、四层导航栈、等待队列，以及 View 缓存和延迟销毁。

请先安装核心包，再添加：

```text
https://github.com/jasmineyamo94-oss/component-autobind.git?path=/Packages/com.jasmineyamo.simple-ui-vcontainer#v0.2.0
```

请单独安装 `jp.hadashikick.vcontainer`。配套包不会固定 VContainer 版本。未安装 VContainer 时，其运行时程序集会被跳过，并且 Unity Console 会在每次 Editor 会话中报告一次警告；核心包仍可正常使用。

安装配套包和 VContainer 后，可在 Project 窗口中使用 **Create > C# Scripts VContainer View** 创建配套的 View 和 Presenter。例如，`AutoBindTestView` 会创建 `AutoBindTestView.cs` 和 `AutoBindTestPresenter.cs`。Presenter 通过构造函数注入具体的生成 `UIView` 和共享的 `ViewBundle`。可从该包的 Samples 页面导入 **Resources Demo** 查看完整运行示例。

## 示例与测试

从包的 Samples 标签页导入 **Basic UGUI**，即可体验纯代码示例。该示例会在运行时创建 Canvas、Button 和旧版 UGUI Text 组件，因此不依赖场景资源或 TextMeshPro。

各包在 `Tests/` 下包含 Runtime 和 Editor 测试。启用 `UNITY_INCLUDE_TESTS` 后，可通过 Unity Test Runner 运行这些测试。

## 范围

核心包有意不包含 VContainer 适配器、生成的游戏项目绑定、项目设置资源、旧示例场景或第三方泛型字典代码。可选的 Simple UI 包使核心运行时保持对 VContainer 的独立性。

配置详情请参阅 [Documentation~/GettingStarted.md](Documentation~/GettingStarted.md)，版本历史请参阅 [CHANGELOG.md](CHANGELOG.md)。

基于此库进行改动：[CatImmortal/ComponentAutoBindTool](https://github.com/CatImmortal/ComponentAutoBindTool)
