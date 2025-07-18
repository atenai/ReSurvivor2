using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XInputTriggerHandler
{
	private bool previous;
	public bool IsPressed;
	public bool Down;
	public bool Up;

	public void Update(float axis, float threshold = 0.5f)
	{
		IsPressed = axis > threshold;
		Down = IsPressed && !previous;
		Up = !IsPressed && previous;
		previous = IsPressed;
	}
}
