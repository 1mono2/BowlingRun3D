using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
[VolumeComponentMenu("Bag Effect")]
public class BagPostProcessVolume : VolumeComponent
{
    public BoolParameter isActive = new BoolParameter(false);
    public FloatParameter splitX = new FloatParameter(5);
    public FloatParameter splitY = new FloatParameter(5);
    [Range(0, 1)]
    public FloatParameter shift = new FloatParameter(0.1f);
    [Range(0, 20)]
    public FloatParameter frec = new FloatParameter(1);
    [Range(-1, 1)]
    public FloatParameter colorGap = new FloatParameter(1);
    [Range(0, 1)]
    public FloatParameter ratio = new FloatParameter(0.1f);
    [Range(0, 1)]
    public FloatParameter strength = new FloatParameter(0.5f);
    [Range(0, 1)]
    public FloatParameter blur = new FloatParameter(0.5f);
}