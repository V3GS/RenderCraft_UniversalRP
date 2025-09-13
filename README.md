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
This example shows how to filter a set of renderers based on the [layermask](https://docs.unity3d.com/ScriptReference/LayerMask.html), and then, generate a set of render textures that will store the Depth, Normals, and Color buffer. Those buffers will be subsequently used for generating the outline effect.

![Outline shader filtering objects](Images/outline_effect.gif)

This example uses the [RenderGraph API](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/render-graph.html) to write a Scriptable Render Passes in the Universal Render Pipeline (URP). Taking advantage of this API, it was used the following features:
- Filter objects by using the [DrawRendererListCommandBuffer.DrawRendererList](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Rendering.CommandBuffer.DrawRendererList.html) method.
- Multiple Render Targets (MRT) to store the Depth, Normals, and Color buffer.
- [Transfer a set of textures between render passes in URP](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/render-graph-pass-textures-between-passes.html) to grab the aforementioned buffers to generate the outline effect.

# References
"If I have seen further it is by standing on the shoulders of giants" - Isaac Newton

## Edge detection
There are many articles on this subject, but the following are the ones I enjoyed the most.
The authors explain in detail how to achieved the edge detection effect in Unity, so before using this repository, I highly suggest to read the following articles.

* [Outline Shader](https://roystan.net/articles/outline-shader/) by Roystan
* [Edge Detection Outlines](https://ameye.dev/notes/edge-detection-outlines/) by Alexander Ameye
* [Outline Post Process in Unity Shader Graph (URP)](https://danielilett.com/2023-03-21-tut7-1-fullscreen-outlines/) by Daniel Ilett

# Resources
 * [Fat Marine, mGear Maya Rig (Guide and postScripts included)](https://milio-serrano.gumroad.com/l/hypza) by Emilio Serrano (https://milio-serrano.gumroad.com/).