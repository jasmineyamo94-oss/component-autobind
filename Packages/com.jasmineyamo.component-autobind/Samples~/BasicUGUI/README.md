# Basic UGUI sample

This sample creates a Canvas, a Button, and a legacy UGUI Text component at runtime. It manually fills the runtime binding list, then uses the same generated binding shape that the editor creates.

To try it, create an empty GameObject in a scene, add `BasicUGUIExample`, and enter Play Mode. The sample has no scene asset and does not require TextMeshPro.

For editor-driven binding, add `ComponentAutoBindTool` to the target GameObject, create the project settings assets from `Tools/Component Auto Bind`, and use the `Auto Bind` button after naming child objects with keys such as `Btn_Submit` and `Txt_Status`.
