using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class OutputOutlineTexturesPass : ScriptableRenderPass
{
    private const string k_PassName = "OutputOutlineTexturesPass";
    private Material m_OutputOutlineMaterial;
    private LayerMask m_LayerMask;

    private FilteringSettings m_FilteringSettings;
    private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

    private const string k_NormalDepthTextureName = "_NormalDepthTexture";
    private const string k_ColorMaskTextureName = "_ColorMaskTexture";

    private RenderTextureDescriptor m_NormalDepthTextureDescriptor;
    private RenderTextureDescriptor m_ColorMaskTextureDescriptor;

    private int m_GlobalNormalDepthTextureID = Shader.PropertyToID(k_NormalDepthTextureName);
    private int m_GlobalColorMaskTextureID = Shader.PropertyToID(k_ColorMaskTextureName);

    // This class stores the data needed by the RenderGraph pass.
    // It is passed as a parameter to the delegate function that executes the RenderGraph pass.
    private class PassData
    {
        public RendererListHandle rendererListHandle;
    }

    public OutputOutlineTexturesPass(RenderPassEvent passRenderPassEvent, OutlineRendererFeature.OutputOulineSettings passSettings)
    {
        renderPassEvent = passRenderPassEvent;

        m_LayerMask = passSettings.layerMask;
        m_OutputOutlineMaterial = passSettings.outputOutlineMaterial;

        m_NormalDepthTextureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
        m_ColorMaskTextureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
    }

    private void InitRendererLists(ContextContainer frameData, UniversalCameraData cameraData, ref PassData passData, RenderGraph renderGraph)
    {
        // Access the relevant frame data from the Universal Render Pipeline
        UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
        UniversalLightData lightData = frameData.Get<UniversalLightData>();

        SortingCriteria sortFlags = cameraData.defaultOpaqueSortFlags;
        RenderQueueRange renderQueueRange = RenderQueueRange.opaque;
        m_FilteringSettings = new FilteringSettings(renderQueueRange, m_LayerMask);

        m_ShaderTagIdList.Clear();

        m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
        m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
        m_ShaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
        m_ShaderTagIdList.Add(new ShaderTagId("LightweightForward"));

        DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(m_ShaderTagIdList, universalRenderingData, cameraData, lightData, sortFlags);

        // Add the override material to the drawing settings
        drawSettings.overrideMaterial = m_OutputOutlineMaterial;

        var param = new RendererListParams(universalRenderingData.cullResults, drawSettings, m_FilteringSettings);
        passData.rendererListHandle = renderGraph.CreateRendererList(param);
    }

    private void CreateTextureDescriptor(UniversalCameraData cameraData, ref RenderTextureDescriptor descriptor)
    {
        descriptor.width = cameraData.cameraTargetDescriptor.width;
        descriptor.height = cameraData.cameraTargetDescriptor.height;
        descriptor.depthBufferBits = 0;
    }

    // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
    // FrameData is a context container through which URP resources can be accessed and managed.
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        using (var builder = renderGraph.AddRasterRenderPass<PassData>(k_PassName, out var passData))
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            // Set the normal, depth, and color texture descriptor of the same size as the camera target.
            CreateTextureDescriptor(cameraData, ref m_NormalDepthTextureDescriptor);
            CreateTextureDescriptor(cameraData, ref m_ColorMaskTextureDescriptor);

            // Create texture handles
            TextureHandle normalDepthDestination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, m_NormalDepthTextureDescriptor, k_NormalDepthTextureName, false);
            TextureHandle colorMaskDestination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, m_ColorMaskTextureDescriptor, k_ColorMaskTextureName, false);

            InitRendererLists(frameData, cameraData, ref passData, renderGraph);

            builder.UseRendererList(passData.rendererListHandle);
            
            builder.SetRenderAttachment(normalDepthDestination, 0);
            builder.SetRenderAttachment(colorMaskDestination, 1);

            builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);

            // Make global the Normal, Depth, and Color textures
            builder.SetGlobalTextureAfterPass(normalDepthDestination, m_GlobalNormalDepthTextureID);
            builder.SetGlobalTextureAfterPass(colorMaskDestination, m_GlobalColorMaskTextureID);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
        }
    }

    static void ExecutePass(PassData data, RasterGraphContext context)
    {
        // Clear the render target to black
        context.cmd.ClearRenderTarget(false, false, Color.clear);

        // Draw the objects in the list
        context.cmd.DrawRendererList(data.rendererListHandle);
    }
}
