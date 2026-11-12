using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// コンピューターメニューの操作の入力（キーボードとパッド）
/// どのキー・ボタンで操作するかは、ここだけを見れば分かるようにする
/// </summary>
public static class ComputerMenuInput
{
	/// <summary>
	/// 上（↑キー・Wキー・十字キーの上）を押したか？
	/// </summary>
	public static bool IsUpPressed()
	{
		return Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || XInputManager.SingletonInstance.XInputDPadHandler.UpDown;
	}

	/// <summary>
	/// 下（↓キー・Sキー・十字キーの下）を押したか？
	/// </summary>
	public static bool IsDownPressed()
	{
		return Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) || XInputManager.SingletonInstance.XInputDPadHandler.DownDown;
	}

	/// <summary>
	/// 左（←キー・Aキー・十字キーの左）を押したか？
	/// </summary>
	public static bool IsLeftPressed()
	{
		return Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || XInputManager.SingletonInstance.XInputDPadHandler.LeftDown;
	}

	/// <summary>
	/// 右（→キー・Dキー・十字キーの右）を押したか？
	/// </summary>
	public static bool IsRightPressed()
	{
		return Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) || XInputManager.SingletonInstance.XInputDPadHandler.RightDown;
	}

	/// <summary>
	/// 決定（Enterキー・Aボタン）を押したか？
	/// </summary>
	public static bool IsDecidePressed()
	{
		return Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("XInput A");
	}

	/// <summary>
	/// キャンセル（Bキー・Bボタン）を押したか？
	/// </summary>
	public static bool IsCancelPressed()
	{
		return Input.GetKeyDown(KeyCode.B) || Input.GetButtonDown("XInput B");
	}
}
