using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// XInputのDPadの状態を管理するクラス
/// </summary>
public class XInputDPadHandler
{
	public bool LeftDown { get; private set; }
	public bool RightDown { get; private set; }
	public bool UpDown { get; private set; }
	public bool DownDown { get; private set; }

	// 前フレームのDPadのX軸の値を保存する変数
	float prevDownX = 0f;
	// 前フレームのDPadのY軸の値を保存する変数
	float prevDownY = 0f;

	public bool LeftHold { get; private set; }
	public bool RightHold { get; private set; }
	public bool UpHold { get; private set; }
	public bool DownHold { get; private set; }


	public bool LeftUp { get; private set; }
	public bool RightUp { get; private set; }
	public bool UpUp { get; private set; }
	public bool DownUp { get; private set; }

	// 前フレームのDPadのX軸の値を保存する変数
	float prevUpX = 0f;
	// 前フレームのDPadのY軸の値を保存する変数
	float prevUpY = 0f;

	// 入力を判定するためのしきい値
	const float threshold = 0.5f;

	/// <summary>
	/// DPadのDown状態を更新
	/// </summary>
	void UpdateDPadDown()
	{
		// 初期化
		LeftDown = false;
		RightDown = false;
		UpDown = false;
		DownDown = false;

		// 右
		if (threshold < Input.GetAxis("XInput DPad Left&Right") && prevDownX <= threshold)
		{
			RightDown = true;
		}

		// 左
		if (Input.GetAxis("XInput DPad Left&Right") < -threshold && -threshold <= prevDownX)
		{
			LeftDown = true;
		}

		// 上
		if (threshold < Input.GetAxis("XInput DPad Up&Down") && prevDownY <= threshold)
		{
			UpDown = true;
		}

		// 下
		if (Input.GetAxis("XInput DPad Up&Down") < -threshold && -threshold <= prevDownY)
		{
			DownDown = true;
		}

		// 前フレームの値を保存
		prevDownX = Input.GetAxis("XInput DPad Left&Right");
		prevDownY = Input.GetAxis("XInput DPad Up&Down");
	}

	/// <summary>
	/// DPadのHold状態を更新
	/// </summary>
	void UpdateDPadHold()
	{
		LeftHold = Input.GetAxis("XInput DPad Left&Right") < -threshold;
		RightHold = threshold < Input.GetAxis("XInput DPad Left&Right");
		UpHold = threshold < Input.GetAxis("XInput DPad Up&Down");
		DownHold = Input.GetAxis("XInput DPad Up&Down") < -threshold;
	}

	/// <summary>
	/// DPadのUp状態を更新
	/// </summary>
	void UpdateDPadUp()
	{
		// 初期化
		LeftUp = false;
		RightUp = false;
		UpUp = false;
		DownUp = false;

		// 右
		if (threshold < prevUpX && Input.GetAxis("XInput DPad Left&Right") <= threshold)
		{
			RightUp = true;
		}

		// 左
		if (prevUpX < -threshold && Input.GetAxis("XInput DPad Left&Right") >= -threshold)
		{
			LeftUp = true;
		}

		// 上
		if (threshold < prevUpY && Input.GetAxis("XInput DPad Up&Down") <= threshold)
		{
			UpUp = true;
		}

		// 下
		if (prevUpY < -threshold && Input.GetAxis("XInput DPad Up&Down") >= -threshold)
		{
			DownUp = true;
		}

		prevUpX = Input.GetAxis("XInput DPad Left&Right");
		prevUpY = Input.GetAxis("XInput DPad Up&Down");
	}

	public void Update()
	{
		UpdateDPadDown();
		UpdateDPadHold();
		UpdateDPadUp();
	}
}
