using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class OutlineRenderPass : ScriptableRenderPass
{
    const string k_PassName = "Outline Pass";
    Material m_BlitOutlineMaterial = null;

    public class PassData
    {
        public TextureHandle copySourceTexture;
        public Material material;
    }

    public OutlineRenderPass(OutlineRendererFeature.Settings settings)
    {
        renderPassEvent = settings.renderPassEvent;
        m_BlitOutlineMaterial = settings.blitMaterial;

        requiresIntermediateTexture = true;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        using (var builder = renderGraph.AddRasterRenderPass<PassData>(k_PassName, out var passData))
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            passData.copySourceTexture = resourceData.activeColorTexture;
            passData.material = m_BlitOutlineMaterial;

            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
            desc.msaaSamples = 1;
            desc.depthBufferBits = 0;

            TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "CopyTexture", false);

            builder.UseTexture(passData.copySourceTexture);
            builder.SetRenderAttachment(destination, 0);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));

            resourceData.cameraColor = destination;
        }
    }

    static void ExecutePass(PassData data, RasterGraphContext context)
    {
        Blitter.BlitTexture(context.cmd, data.copySourceTexture, new Vector4(1, 1, 0, 0), data.material, 0);
    }
}
