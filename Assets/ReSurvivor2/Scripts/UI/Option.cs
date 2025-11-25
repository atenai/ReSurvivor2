using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// オプション
/// </summary>
public class Option : OutGameBase
{
	[Tooltip("メニューオプションのイメージリスト")]
	[SerializeField] Image[] menuOptions;
	/// <summary>
	/// 現在選択されているメニューのインデックス
	/// </summary>
	int currentSelectedIndex = 0;
	/// <summary>
	/// メニューボタンを連続でキー入力による選択をできないようにする為の変数
	/// </summary>
	bool isDisableConsecutiveKeystrokes = true;
	/// <summary>
	/// XInputのDPadハンドラー
	/// </summary>
	XInputDPadHandler xInputDPadHandler = new XInputDPadHandler();

	new void Start()
	{
		base.Start();

		Menu();

		isDisableConsecutiveKeystrokes = true;
	}

	new void Update()
	{
		sliderShaderLoading.value = GetShaderWarmupProgressRate();

		if (isDisableConsecutiveKeystrokes == false)
		{
			return;
		}

		//上下の矢印キーで選択を変更
		if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || xInputDPadHandler.LeftDown)
		{
			currentSelectedIndex--;
			if (currentSelectedIndex < 0)
			{
				currentSelectedIndex = menuOptions.Length - 1;
			}
			Menu();
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) || xInputDPadHandler.RightDown)
		{
			currentSelectedIndex++;
			if (menuOptions.Length <= currentSelectedIndex)
			{
				currentSelectedIndex = 0;
			}
			Menu();
		}

		UpdateXInputDPad();

		//Enterキーで選択を確定
		if (Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("XInput A"))
		{
			ExecuteMenuAction();
		}
	}

	/// <summary>
	/// メニュー
	/// </summary> 
	void Menu()
	{
		//メニューの見た目を更新
		for (int i = 0; i < menuOptions.Length; i++)
		{
			if (i == currentSelectedIndex)
			{
				//選択中の項目の色を変更
				menuOptions[i].color = new Color(menuOptions[i].color.r, menuOptions[i].color.g, menuOptions[i].color.b, 0.5f);
			}
			else
			{
				menuOptions[i].color = new Color(menuOptions[i].color.r, menuOptions[i].color.g, menuOptions[i].color.b, 0.0f);
			}
		}
	}

	void ExecuteMenuAction()
	{
		switch (currentSelectedIndex)
		{
			case 0:

				break;
			case 1:

				break;
			case 2:
				SaveDataDelete();
				break;
			case 3:
				BackToTitle();
				break;
		}
	}

	/// <summary>
	/// セーブデータ削除
	/// </summary>
	void SaveDataDelete()
	{
		Debug.Log("<color=orange>セーブデータを削除しました</color>");
		PlayerPrefs.DeleteAll();
	}

	/// <summary>
	/// タイトルに戻る
	/// </summary>
	void BackToTitle()
	{
		isDisableConsecutiveKeystrokes = false;
		Load(nextSceneName);
	}

	/// <summary>
	/// XInputのDPad状態を更新
	/// </summary>
	void UpdateXInputDPad()
	{
		// DPad軸を取得（InputManagerで設定済み or 軸番号で直接）
		float dpadX = Input.GetAxis("XInput DPad Left&Right");
		float dpadY = Input.GetAxis("XInput DPad Up&Down");

		// 状態更新
		xInputDPadHandler.Update(dpadX, dpadY);
	}
}
