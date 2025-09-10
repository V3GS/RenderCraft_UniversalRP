using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;

public class EdgeDetectionPass : ScriptableRenderPass
{
    private Material material;

    private static readonly int OutlineThicknessProperty = Shader.PropertyToID("_OutlineThickness");
    private static readonly int OutlineColorProperty = Shader.PropertyToID("_OutlineColor");

    public EdgeDetectionPass()
    {
        profilingSampler = new ProfilingSampler(nameof(EdgeDetectionPass));
    }

    public void Setup(ref EdgeDetection.EdgeDetectionSettings settings, ref Material edgeDetectionMaterial)
    {
        material = edgeDetectionMaterial;
        renderPassEvent = settings.renderPassEvent;

        material.SetFloat(OutlineThicknessProperty, settings.outlineThickness);
        material.SetColor(OutlineColorProperty, settings.outlineColor);
    }

    private class PassData
    {
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var resourceData = frameData.Get<UniversalResourceData>();

        using var builder = renderGraph.AddRasterRenderPass<PassData>("Edge Detection", out _);

        builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
        builder.UseAllGlobalTextures(true);
        builder.AllowPassCulling(false);

        builder.SetRenderFunc((PassData _, RasterGraphContext context) => {
            Blitter.BlitTexture(context.cmd, Vector2.one, material, 0);
        });
    }
}