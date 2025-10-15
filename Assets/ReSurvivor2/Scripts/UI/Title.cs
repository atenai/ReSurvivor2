using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// タイトル
/// </summary>
public class Title : OutGameBase
{
	[Tooltip("メニューオプションのイメージリスト")]
	[SerializeField] Image[] menuOptions;
	int selectedIndex = 0;
	[SerializeField] string showCreditsSceneName;
	[Tooltip("メニューボタンを連続でキー入力による選択をできないようにする為の変数")]
	bool isDisableConsecutiveKeystrokes = true;


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
			selectedIndex--;
			if (selectedIndex < 0)
			{
				selectedIndex = menuOptions.Length - 1;
			}
			Menu();
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) || xInputDPadHandler.RightDown)
		{
			selectedIndex++;
			if (menuOptions.Length <= selectedIndex)
			{
				selectedIndex = 0;
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
			if (i == selectedIndex)
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
		switch (selectedIndex)
		{
			case 0:
				StartGame();
				break;
			case 1:
				ShowCredits();
				break;
			case 2:
				SaveDataDelete();
				break;
			case 3:
				ExitGame();
				break;
		}
	}

	/// <summary>
	/// ゲームスタート
	/// </summary>
	void StartGame()
	{
		isDisableConsecutiveKeystrokes = false;
		Load(sceneName);
	}

	/// <summary>
	/// クレジットシーンへ
	/// </summary>
	void ShowCredits()
	{
		isDisableConsecutiveKeystrokes = false;
		Load(showCreditsSceneName);
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
	/// ゲーム終了
	/// </summary>
	void ExitGame()
	{
		isDisableConsecutiveKeystrokes = false;
		Quit();
	}

	void Quit()
	{
#if UNITY_EDITOR
		EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
        UnityEngine.Application.Quit();
#endif
	}

	void UpdateXInputDPad()
	{
		// DPad軸を取得（InputManagerで設定済み or 軸番号で直接）
		float dpadX = Input.GetAxis("XInput DPad Left&Right");
		float dpadY = Input.GetAxis("XInput DPad Up&Down");

		// 状態更新
		xInputDPadHandler.Update(dpadX, dpadY);
	}
}
