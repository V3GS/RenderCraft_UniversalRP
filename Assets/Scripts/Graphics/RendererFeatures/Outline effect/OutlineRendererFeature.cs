using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class OutlineRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public bool isEnabled = true;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        
        // Material for generating the output buffers (Normal, Depth, and Color)
        public Material outputOutlineMaterial = null;
        // Material that grabs the aforementioned buffers and performs the outline calculations
        public Material blitMaterial = null;

        // Property that indicates the objects that will be affected by the outline effect
        public LayerMask layerMask;
    }

    public Settings PassSettings
    {
        set { m_passSettings = value; }
        get { return m_passSettings; }
    }

    [SerializeField]
    private Settings m_passSettings = new Settings();

    private OutputOutlineTexturesPass m_OutputOutlinePass;
    private OutlineRenderPass m_OutlinePass;

    public override void Create()
    {
        m_OutputOutlinePass = new OutputOutlineTexturesPass(m_passSettings);
        m_OutlinePass = new OutlineRenderPass(m_passSettings.renderPassEvent, m_passSettings.blitMaterial);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // If the pass is not enabled
        if (!m_passSettings.isEnabled) return;

        // If the Material is not correctly setup, it won't be enqueue the pass 
        if (m_passSettings.outputOutlineMaterial == null)
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
