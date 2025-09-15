using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
[VolumeComponentMenu("Custom/Outline Volume Component")]
public class OutlineVolumeComponent : VolumeComponent, IPostProcessComponent
{
    public enum VisualizeOption { OutlineColor, Outline, Normal, Depth, Color};

    // Color parameters
    public ColorParameter outlineColor = new ColorParameter(Color.black);

    // Float parameters
    public ClampedFloatParameter scale = new ClampedFloatParameter(4.0f, 0.0f, 10.0f);
    public ClampedFloatParameter depthThreshold = new ClampedFloatParameter(13.5f, 0.0f, 100.0f);
    public ClampedFloatParameter normalThreshold = new ClampedFloatParameter(0.337f, 0.0f, 1.0f);
    public ClampedFloatParameter colorThreshold = new ClampedFloatParameter(0.5f, 0.0f, 1.0f);

    public bool IsActive()
    {
        return scale.value > 0.0f;
    }
}
