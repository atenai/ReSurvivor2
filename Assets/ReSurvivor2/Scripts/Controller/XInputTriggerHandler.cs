using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// XInputのトリガーの状態を管理するクラス
/// </summary>
public class XInputTriggerHandler
{
	bool previousRT = false;
	public bool IsPressedRT { get; private set; }
	public bool DownRT { get; private set; }
	public bool UpRT { get; private set; }

	bool previousLT = false;
	public bool IsPressedLT { get; private set; }
	public bool DownLT { get; private set; }
	public bool UpLT { get; private set; }

	// 入力を判定するためのしきい値
	const float threshold = 0.5f;

	/// <summary>
	/// トリガーのDown状態を更新
	/// </summary>
	public void UpdateRT()
	{
		IsPressedRT = threshold < Input.GetAxis("XInput RT");
		DownRT = IsPressedRT && !previousRT;
		UpRT = !IsPressedRT && previousRT;
		previousRT = IsPressedRT;
	}

	/// <summary>
	/// トリガーのDown状態を更新
	/// </summary>
	public void UpdateLT()
	{
		IsPressedLT = threshold < Input.GetAxis("XInput LT");
		DownLT = IsPressedLT && !previousLT;
		UpLT = !IsPressedLT && previousLT;
		previousLT = IsPressedLT;
	}
}
