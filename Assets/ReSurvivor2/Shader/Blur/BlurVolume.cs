using System;
using UnityEngine.Rendering;

[Serializable]
public class BlurVolume : VolumeComponent
{
	private BoolParameter _active = new(true, true);
	public bool IsActive => _active.overrideState;

	public ClampedFloatParameter HorizontalBlur = new(0.05f, 0, 0.5f);
	public ClampedFloatParameter VerticalBlur = new(0.05f, 0, 0.5f);
}