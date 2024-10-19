using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;

/// <summary>
/// タイトル
/// </summary>
public class Title : OutGameManagerBase
{
    [Tooltip("メニューオプションのイメージリスト")]
    [SerializeField] Image[] menuOptions;
    int selectedIndex = 0;
    [SerializeField] string showCreditsSceneName;

    new void Start()
    {
        base.Start();

        Menu();
    }

    new void Update()
    {
        // 上下の矢印キーで選択を変更
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            selectedIndex--;
            if (selectedIndex < 0)
            {
                selectedIndex = menuOptions.Length - 1;
            }
            Menu();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            selectedIndex++;
            if (menuOptions.Length <= selectedIndex)
            {
                selectedIndex = 0;
            }
            Menu();
        }

        // Enterキーで選択を確定
        if (Input.GetKeyDown(KeyCode.Return))
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
                ExitGame();
                break;
        }
    }

    /// <summary>
    /// ゲームスタート
    /// </summary>
    void StartGame()
    {
        Load(sceneName);
    }

    /// <summary>
    /// クレジットシーンへ
    /// </summary>
    void ShowCredits()
    {
        Load(showCreditsSceneName);
    }

    /// <summary>
    /// ゲーム終了
    /// </summary>
    void ExitGame()
    {
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
