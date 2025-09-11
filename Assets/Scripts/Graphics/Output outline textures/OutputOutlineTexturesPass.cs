using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class OutputOutlineTexturesPass : ScriptableRenderPass
{
    private const string k_PassName = "OutputOutlineTexturesPass";
    private Material m_OutputOutlineMaterial;
    private LayerMask m_LayerMask;

    private FilteringSettings m_FilteringSettings;
    private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

    private const string k_NormalTextureName = "_NormalTexture";
    private RenderTextureDescriptor m_NormalTextureDescriptor;

    // This class stores the data needed by the RenderGraph pass.
    // It is passed as a parameter to the delegate function that executes the RenderGraph pass.
    private class PassData
    {
        public RendererListHandle rendererListHandle;
    }

    public OutputOutlineTexturesPass(OutputOutlineTexturesRendererFeature.Settings passSettings)
    {
        renderPassEvent = passSettings.renderPassEvent;

        m_LayerMask = passSettings.layerMask;
        m_OutputOutlineMaterial = passSettings.outputOutlineMaterial;

        m_NormalTextureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
    }

    private void InitRendererLists(ContextContainer frameData, ref PassData passData, RenderGraph renderGraph)
    {
        // Access the relevant frame data from the Universal Render Pipeline
        UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        UniversalLightData lightData = frameData.Get<UniversalLightData>();

        SortingCriteria sortFlags = cameraData.defaultOpaqueSortFlags;
        RenderQueueRange renderQueueRange = RenderQueueRange.opaque;
        FilteringSettings filterSettings = new FilteringSettings(renderQueueRange, m_LayerMask);

        m_ShaderTagIdList.Clear();

        m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
        m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
        m_ShaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
        m_ShaderTagIdList.Add(new ShaderTagId("LightweightForward"));

        DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(m_ShaderTagIdList, universalRenderingData, cameraData, lightData, sortFlags);

        // Add the override material to the drawing settings
        drawSettings.overrideMaterial = m_OutputOutlineMaterial;

        var param = new RendererListParams(universalRenderingData.cullResults, drawSettings, filterSettings);
        passData.rendererListHandle = renderGraph.CreateRendererList(param);
    }

    // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
    // FrameData is a context container through which URP resources can be accessed and managed.
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        using (var builder = renderGraph.AddRasterRenderPass<PassData>(k_PassName, out var passData))
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            // Set the normal texture size to be the same as the camera target size.
            m_NormalTextureDescriptor.width = cameraData.cameraTargetDescriptor.width;
            m_NormalTextureDescriptor.height = cameraData.cameraTargetDescriptor.height;
            m_NormalTextureDescriptor.depthBufferBits = 0;

            TextureHandle normalDestination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, m_NormalTextureDescriptor, k_NormalTextureName, false);

            InitRendererLists(frameData, ref passData, renderGraph);

            builder.UseRendererList(passData.rendererListHandle);
            builder.SetRenderAttachment(normalDestination, 0);
            builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
        }
    }

    static void ExecutePass(PassData data, RasterGraphContext context)
    {
        // Clear the render target to black
        context.cmd.ClearRenderTarget(false, false, Color.black);

        // Draw the objects in the list
        context.cmd.DrawRendererList(data.rendererListHandle);
    }
}
