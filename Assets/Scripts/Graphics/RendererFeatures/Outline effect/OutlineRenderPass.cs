using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using static OutlineRendererFeature;
using static OutlineVolumeComponent;

public class OutlineRenderPass : ScriptableRenderPass
{
    const string k_PassName = "Outline Pass";
    Material m_BlitOutlineMaterial = null;

    static readonly int m_OutlineColorId = Shader.PropertyToID("_OutlineColor");
    static readonly int m_HighlightColorId = Shader.PropertyToID("_HighlightColor");
    static readonly int m_ScaleId = Shader.PropertyToID("_Scale");
    static readonly int m_DepthThresholdId = Shader.PropertyToID("_DepthThreshold");
    static readonly int m_NormalThresholdId = Shader.PropertyToID("_NormalThreshold");
    static readonly int m_ColorThresholdId = Shader.PropertyToID("_ColorThreshold");

    // Shader data
    private Color m_OutlineColor = Color.black;
    private Color m_HighlightColor = Color.black;
    private float m_Scale = 0.0f;
    private float m_DepthThreshold = 0.0f;
    private float m_NormalThreshold = 0.0f;
    private float m_ColorThreshold = 0.0f;

    private OutlineSettings m_OutlineSettings;
    private OutlineVolumeComponent m_VolumeComponent;

    public class PassData
    {
        public TextureHandle copySourceTexture;
        public Material material;
    }

    public OutlineRenderPass(RenderPassEvent passRenderPassEvent, OutlineSettings outlineSettings)
    {
        renderPassEvent = passRenderPassEvent;
        m_BlitOutlineMaterial = outlineSettings.blitMaterial;
        m_OutlineSettings = outlineSettings;

        requiresIntermediateTexture = true;

        m_VolumeComponent = VolumeManager.instance.stack.GetComponent<OutlineVolumeComponent>();
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        using (var builder = renderGraph.AddRasterRenderPass<PassData>(k_PassName, out var passData))
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            UpdateOutlineSettings();

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

    void UpdateOutlineSettings()
    {
        if (m_BlitOutlineMaterial == null) return;

        // If exist a volume component in charge of setup the outline properties
        if (m_VolumeComponent != null)
        {
            // If override state, retrieve the information from the volume property,  else retrieve the information from the the Renderer Feature settings
            m_OutlineColor = m_VolumeComponent.outlineColor.overrideState ? m_VolumeComponent.outlineColor.value : m_OutlineSettings.outlineColor;
            m_HighlightColor = m_VolumeComponent.highlightColor.overrideState ? m_VolumeComponent.highlightColor.value : m_OutlineSettings.hightlightColor;

            m_Scale = m_VolumeComponent.scale.overrideState ? m_VolumeComponent.scale.value : m_OutlineSettings.scale;
            m_DepthThreshold = m_VolumeComponent.depthThreshold.overrideState ? m_VolumeComponent.depthThreshold.value : m_OutlineSettings.depthThreshold;
            m_NormalThreshold = m_VolumeComponent.normalThreshold.overrideState ? m_VolumeComponent.normalThreshold.value : m_OutlineSettings.normalThreshold;
            m_ColorThreshold = m_VolumeComponent.colorThreshold.overrideState ? m_VolumeComponent.colorThreshold.value : m_OutlineSettings.colorThreshold;
        }
        // Otherwise, uses the settings from the Renderer Feature
        else
        {
            m_OutlineColor = m_OutlineSettings.outlineColor;
            m_HighlightColor = m_OutlineSettings.hightlightColor;

            m_Scale = m_OutlineSettings.scale;
            m_DepthThreshold = m_OutlineSettings.depthThreshold;
            m_NormalThreshold = m_OutlineSettings.normalThreshold;
            m_ColorThreshold = m_OutlineSettings.colorThreshold;
        }

        // Set any material properties based on our volume pass settings
        m_BlitOutlineMaterial.SetColor(m_OutlineColorId, m_OutlineColor);
        m_BlitOutlineMaterial.SetColor(m_HighlightColorId, m_HighlightColor);

        m_BlitOutlineMaterial.SetFloat(m_ScaleId, m_Scale);
        m_BlitOutlineMaterial.SetFloat(m_DepthThresholdId, m_DepthThreshold);
        m_BlitOutlineMaterial.SetFloat(m_NormalThresholdId, m_NormalThreshold);
        m_BlitOutlineMaterial.SetFloat(m_ColorThresholdId, m_ColorThreshold);
    }
}
