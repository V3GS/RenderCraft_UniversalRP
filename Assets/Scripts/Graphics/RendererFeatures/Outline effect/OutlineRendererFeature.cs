using UnityEngine;
using UnityEngine.Rendering.Universal;

public class OutlineRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public bool isEnabled = true;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

        public Material blitMaterial = null;
    }

    private OutlineRenderPass m_OutlinePass;

    [SerializeField]
    private Settings m_passSettings = new Settings();

    public Settings PassSettings
    {
        set { m_passSettings = value; }
        get { return m_passSettings; }
    }

    public override void Create()
    {
        m_OutlinePass = new OutlineRenderPass(m_passSettings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!m_passSettings.isEnabled) return;

        if (m_passSettings.blitMaterial == null)
        {
            Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
            return;
        }

        // By checking the following, the ScriptableRendererFeature will only rendered in the GameView
        if (!renderingData.cameraData.isSceneViewCamera && !renderingData.cameraData.isPreviewCamera)
        {
            renderer.EnqueuePass(m_OutlinePass);
        }
    }
}
