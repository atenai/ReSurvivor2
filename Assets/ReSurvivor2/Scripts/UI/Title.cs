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
	/// <summary>
	/// 現在選択されているメニューのインデックス
	/// </summary>
	int currentSelectedIndex = 0;
	/// <summary>
	/// メニューボタンを連続でキー入力による選択をできないようにする為の変数
	/// </summary>
	bool isDisableConsecutiveKeystrokes = true;

	[Tooltip("クレジットシーン名")]
	[SerializeField] string creditsSceneName;
	[Tooltip("オプションシーン名")]
	[SerializeField] string optionSceneName;

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
		if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || XInputManager.SingletonInstance.XInputDPadHandler.LeftDown)
		{
			currentSelectedIndex--;
			if (currentSelectedIndex < 0)
			{
				currentSelectedIndex = menuOptions.Length - 1;
			}
			Menu();
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) || XInputManager.SingletonInstance.XInputDPadHandler.RightDown)
		{
			currentSelectedIndex++;
			if (menuOptions.Length <= currentSelectedIndex)
			{
				currentSelectedIndex = 0;
			}
			Menu();
		}

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
				StartGame();
				break;
			case 1:
				ShowCredits();
				break;
			case 2:
				ShowOption();
				//SaveDataDelete();
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
		const string Stage = "Stage";
		string sceneName = Stage + ES3.Load<int>("Stage", 0).ToString();
		Load(sceneName);
	}

	/// <summary>
	/// クレジットシーンへ
	/// </summary>
	void ShowCredits()
	{
		isDisableConsecutiveKeystrokes = false;
		Load(creditsSceneName);
	}

	/// <summary>
	/// オプションシーンへ
	/// </summary>
	void ShowOption()
	{
		isDisableConsecutiveKeystrokes = false;
		Load(optionSceneName);
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
}
