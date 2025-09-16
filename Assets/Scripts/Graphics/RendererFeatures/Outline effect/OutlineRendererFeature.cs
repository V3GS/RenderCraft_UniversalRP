using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class OutlineRendererFeature : ScriptableRendererFeature
{
    [SerializeField]
    private bool m_IsEnabled = true;
    [SerializeField]
    private RenderPassEvent m_RenderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

    [System.Serializable]
    public class OutputOulineSettings
    {
        // Material for generating the output buffers (Normal, Depth, and Color)
        public Material outputOutlineMaterial = null;

        // Property that indicates the objects that will be affected by the outline effect
        public LayerMask layerMask;
    }

    [System.Serializable]
    public class OutlineSettings
    {
        // Material that grabs the aforementioned buffers and performs the outline calculations
        public Material blitMaterial = null;

        public Color outlineColor = Color.black;
        public Color hightlightColor = Color.black;

        public float scale = 4.15f;
        public float depthThreshold = 13.5f;
        public float normalThreshold = 0.337f;
        public float colorThreshold = 0.5f;
    }

    public OutputOulineSettings OutputOutlineSettings
    {
        set { m_OutputOutlineSettings = value; }
        get { return m_OutputOutlineSettings; }
    }

    [SerializeField]
    private OutputOulineSettings m_OutputOutlineSettings = new OutputOulineSettings();
    [SerializeField]
    private OutlineSettings m_OutlineSettings = new OutlineSettings();

    private OutputOutlineTexturesPass m_OutputOutlinePass;
    private OutlineRenderPass m_OutlinePass;

    public override void Create()
    {
        m_OutputOutlinePass = new OutputOutlineTexturesPass(m_RenderPassEvent, m_OutputOutlineSettings);
        m_OutlinePass = new OutlineRenderPass(m_RenderPassEvent, m_OutlineSettings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // If the pass is not enabled
        if (!m_IsEnabled) return;

        // If the Material is not correctly setup, it won't be enqueue the pass 
        if (m_OutputOutlineSettings.outputOutlineMaterial == null)
        {
            Debug.LogWarningFormat("Missing override material. {0} the example pass will not be executed. Check for missing reference in the assigned renderer.", GetType().Name);
            return;
        }

        // By checking the following, the ScriptableRendererFeature will only rendered in the GameView
        if (!renderingData.cameraData.isSceneViewCamera && !renderingData.cameraData.isPreviewCamera)
        {
            renderer.EnqueuePass(m_OutputOutlinePass);
            renderer.EnqueuePass(m_OutlinePass);
        }
    }
}
