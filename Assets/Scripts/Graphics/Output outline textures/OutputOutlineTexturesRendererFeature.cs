using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class OutputOutlineTexturesRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public bool isEnabled = true;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        public Material outputOutlineMaterial = null;

        public LayerMask layerMask;
    }

    public Settings PassSettings
    {
        set { m_passSettings = value; }
        get { return m_passSettings; }
    }

    [SerializeField]
    private Settings m_passSettings = new Settings();
    OutputOutlineTexturesPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new OutputOutlineTexturesPass(m_passSettings);
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
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }
}
