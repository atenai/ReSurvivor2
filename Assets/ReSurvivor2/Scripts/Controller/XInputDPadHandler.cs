using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XInputDPadHandler
{
	private float prevX = 0f;
	private float prevY = 0f;

	public bool LeftDown { get; private set; }
	public bool RightDown { get; private set; }
	public bool UpDown { get; private set; }
	public bool DownDown { get; private set; }

	// 閾値（必要に応じて調整可能）
	private const float threshold = 0.5f;

	public void Update(float dpadX, float dpadY)
	{
		// 初期化
		LeftDown = RightDown = UpDown = DownDown = false;

		// 右（新規押下）
		if (dpadX > threshold && prevX <= threshold)
			RightDown = true;

		// 左（新規押下）
		if (dpadX < -threshold && prevX >= -threshold)
			LeftDown = true;

		// 上（新規押下）
		if (dpadY > threshold && prevY <= threshold)
			UpDown = true;

		// 下（新規押下）
		if (dpadY < -threshold && prevY >= -threshold)
			DownDown = true;

		// 前フレームの値を保存
		prevX = dpadX;
		prevY = dpadY;
	}
}
