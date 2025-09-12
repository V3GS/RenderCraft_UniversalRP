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

    private const string k_NormalTextureName = "_NormalTexture";
    private const string k_DepthTextureName = "_DepthTexture";
    private const string k_ColorTextureName = "_ColorTexture";

    private RenderTextureDescriptor m_NormalTextureDescriptor;
    private RenderTextureDescriptor m_DepthTextureDescriptor;
    private RenderTextureDescriptor m_ColorTextureDescriptor;

    private int m_GlobalNormalTextureID = Shader.PropertyToID(k_NormalTextureName);
    private int m_GlobalDepthTextureID = Shader.PropertyToID(k_DepthTextureName);
    private int m_GlobalColorTextureID = Shader.PropertyToID(k_ColorTextureName);

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
        m_DepthTextureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
        m_ColorTextureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
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
            CreateTextureDescriptor(cameraData, ref m_NormalTextureDescriptor);
            CreateTextureDescriptor(cameraData, ref m_DepthTextureDescriptor);
            CreateTextureDescriptor(cameraData, ref m_ColorTextureDescriptor);

            // Create texture handles
            TextureHandle normalDestination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, m_NormalTextureDescriptor, k_NormalTextureName, false);
            TextureHandle depthDestination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, m_DepthTextureDescriptor, k_DepthTextureName, false);
            TextureHandle colorDestination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, m_ColorTextureDescriptor, k_ColorTextureName, false);

            InitRendererLists(frameData, cameraData, ref passData, renderGraph);

            builder.UseRendererList(passData.rendererListHandle);
            
            builder.SetRenderAttachment(normalDestination, 0);
            builder.SetRenderAttachment(depthDestination, 1);
            builder.SetRenderAttachment(colorDestination, 2);

            builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);

            // Make global the Normal, Depth, and Color textures
            builder.SetGlobalTextureAfterPass(normalDestination, m_GlobalNormalTextureID);
            builder.SetGlobalTextureAfterPass(depthDestination, m_GlobalDepthTextureID);
            builder.SetGlobalTextureAfterPass(colorDestination, m_GlobalColorTextureID);

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
