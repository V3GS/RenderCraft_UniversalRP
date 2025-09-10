# Render craft - UniversalRP
Repository that will store a set of shaders and postprocesses to render toon, stylized, sketch, and other non-realistic effects.

###### Current support: Unity 6000.0.56f1 with UniversalRP 17.0.4


**Usage of the project**
* Clone the repository or download the zip to use this project locally.
* Load the project using Unity 6000.0.56f1 or later

**Project layout**
* `Scripts/Graphics/RendererFeatures`: This folder constains the Scriptable Render Features. Each Scriptable Render Feature is developed using two scripts, one concerning to the [ScriptableRendererFeature](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.4/api/UnityEngine.Rendering.Universal.ScriptableRendererFeature.html), and the other one, that has the [ScriptableRenderPass](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@10.6/api/UnityEngine.Rendering.Universal.ScriptableRenderPass.html) logic.
* `Shaders/Graphs` set of Shaders created using [Shader graph](https://unity.com/es/shader-graph), and `Shaders/Hand-written` will store a set of shaders using Unity's ShaderLab language.

# Examples
Below are shown the examples developed on this repository.

## Outline shader filtering objects
This example shows how to filter a set of renderers based on the [layermask](https://docs.unity3d.com/ScriptReference/LayerMask.html), and then replace their materials for a custom one (Depth, Normals, and Color). That information will be store in a few render textures that will be later used for generating the outline effect.

![Outline shader filtering objects](Images/outline_effect.gif)

# Resources
 * [Fat Marine, mGear Maya Rig (Guide and postScripts included)](https://milio-serrano.gumroad.com/l/hypza) by Emilio Serrano (https://milio-serrano.gumroad.com/).