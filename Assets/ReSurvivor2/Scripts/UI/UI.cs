using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class UI : MonoBehaviour
{
    //シングルトンで作成（ゲーム中に１つのみにする）
    static UI singletonInstance = null;
    public static UI SingletonInstance => singletonInstance;

    [Tooltip("クロスヘアー")]
    [SerializeField] Image imageCrosshair;
    [Tooltip("ヒットレティクル")]
    [SerializeField] Image imageHitReticule;
    public Image ImageHitReticule
    {
        get { return imageHitReticule; }
        set { imageHitReticule = value; }
    }

    [SerializeField] GameObject panelLoading;
    public GameObject PanelLoading
    {
        get { return panelLoading; }
        set { panelLoading = value; }
    }
    [SerializeField] Slider sliderLoading;
    public Slider SliderLoading
    {
        get { return sliderLoading; }
        set { sliderLoading = value; }
    }

    [Tooltip("ダメージ画像エフェクト")]
    [SerializeField] Image imageDamage;
    public Image ImageDamage
    {
        get { return imageDamage; }
        set { imageDamage = value; }
    }

    [Tooltip("HP回復画像エフェクト")]
    [SerializeField] Image imageHpHeal;
    public Image ImageHpHeal
    {
        get { return imageHpHeal; }
        set { imageHpHeal = value; }
    }

    [Tooltip("スタミナ回復画像エフェクト")]
    [SerializeField] Image imageStaminaHeal;
    public Image ImageStaminaHeal
    {
        get { return imageStaminaHeal; }
        set { imageStaminaHeal = value; }
    }

    [Tooltip("フェード用のUI Image")]
    [SerializeField] Image imageFade;
    [Tooltip("フェードの速さ")]
    float fadeSpeed = 1.0f;
    [Tooltip("ポーズ")]
    bool isPause = false;
    public bool IsPause => isPause;
    [SerializeField] GameObject panelPause;
    [Tooltip("ポーズメニューオプションのイメージリスト")]
    [SerializeField] Image[] pauseMenuOptions;
    int selectedIndex = 0;

    void Awake()
    {
        //staticな変数instanceはメモリ領域は確保されていますが、初回では中身が入っていないので、中身を入れます。
        if (singletonInstance == null)
        {
            singletonInstance = this;//thisというのは自分自身のインスタンスという意味になります。この場合、UIのインスタンスという意味になります。
            DontDestroyOnLoad(this.gameObject);//シーンを切り替えた時に破棄しない
        }
        else
        {
            Destroy(this.gameObject);//中身がすでに入っていた場合、自身のインスタンスがくっついているゲームオブジェクトを破棄します。
        }
    }

    void Start()
    {
        //ダメージ画像エフェクト
        imageDamage.color = Color.clear;

        //HP回復画像エフェクト
        imageHpHeal.color = Color.clear;

        //スタミナ回復画像エフェクト
        imageStaminaHeal.color = Color.clear;

        InitFadeColor();

        imageHitReticule.color = Color.clear;
    }

    void Update()
    {
        //↑ロード中に動かせる処理
        if (InGameManager.SingletonInstance.IsGamePlayReady == false)
        {
            return;
        }
        //↓ロード中に動かせない処理

        Crosshair();

        UpdatePauseMenuSystem();
    }

    /// <summary>
    /// クロスヘア（レティクル）のオン/オフかつ色の切り替え
    /// </summary>
    void Crosshair()
    {
        if (Player.SingletonInstance.IsAim == false)
        {
            imageCrosshair.gameObject.SetActive(false);
        }
        else
        {
            imageCrosshair.gameObject.SetActive(true);

            if (PlayerCamera.SingletonInstance.IsTargethit == true)
            {
                imageCrosshair.color = new Color32(255, 0, 0, 150);
            }
            else
            {
                imageCrosshair.color = new Color32(255, 255, 255, 150);
            }
        }
    }

    /// <summary>
    /// フェードの色を初期化
    /// </summary>
    public void InitFadeColor()
    {
        InGameManager.SingletonInstance.IsGamePlayReady = false;
        imageFade.color = new Color(imageFade.color.r, imageFade.color.g, imageFade.color.b, 1);
    }

    /// <summary>
    /// フェードアウト処理（不透明にする）
    /// </summary>
    public IEnumerator FadeOut()
    {
        InGameManager.SingletonInstance.IsGamePlayReady = false;

        //現在のアルファ値を取得
        Color color = imageFade.color;
        float alpha = color.a;

        //アルファ値が1になるまで徐々に増やす
        while (alpha < 1)
        {
            alpha += Time.deltaTime * fadeSpeed;
            imageFade.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        //最後に完全に不透明にする
        imageFade.color = new Color(color.r, color.g, color.b, 1);
    }

    /// <summary>
    /// フェードイン処理（透明にする）
    /// </summary>
    public IEnumerator FadeIn()
    {
        //現在のアルファ値を取得
        Color color = imageFade.color;
        float alpha = color.a;

        //アルファ値が0になるまで徐々に減らす
        while (0 < alpha)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            imageFade.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        //最後に完全に透明にする
        imageFade.color = new Color(color.r, color.g, color.b, 0);

        InGameManager.SingletonInstance.IsGamePlayReady = true;
    }

    /// <summary>
    /// ポーズメニュー画面の更新
    /// </summary> 
    void UpdatePauseMenuSystem()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            Pause();
        }

        if (isPause == true)
        {
            //上下の矢印キーで選択を変更
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                selectedIndex--;
                if (selectedIndex < 0)
                {
                    selectedIndex = pauseMenuOptions.Length - 1;
                }
                PauseMenu();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                selectedIndex++;
                if (pauseMenuOptions.Length <= selectedIndex)
                {
                    selectedIndex = 0;
                }
                PauseMenu();
            }

            //Enterキーで選択を確定
            if (Input.GetKeyDown(KeyCode.Return))
            {
                ExecuteMenuAction();
            }
        }
    }

    /// <summary>
    /// ポーズ
    /// </summary>
    void Pause()
    {
        isPause = isPause ? false : true;

        if (isPause == true)
        {
            Time.timeScale = 0f;
            panelPause.SetActive(true);

            //各種パラメーターの初期化処理
            selectedIndex = 0;
            PauseMenu();
        }
        else
        {
            Time.timeScale = 1f;
            panelPause.SetActive(false);
        }
    }

    /// <summary>
    /// ポーズメニュー
    /// </summary> 
    void PauseMenu()
    {
        //メニューの見た目を更新
        for (int i = 0; i < pauseMenuOptions.Length; i++)
        {
            if (i == selectedIndex)
            {
                //選択中の項目の色を変更
                pauseMenuOptions[i].color = new Color(pauseMenuOptions[i].color.r, pauseMenuOptions[i].color.g, pauseMenuOptions[i].color.b, 0.5f);
            }
            else
            {
                pauseMenuOptions[i].color = new Color(pauseMenuOptions[i].color.r, pauseMenuOptions[i].color.g, pauseMenuOptions[i].color.b, 0.0f);
            }
        }
    }

    void ExecuteMenuAction()
    {
        switch (selectedIndex)
        {
            case 0:
                ReturnToGamePlay();
                break;
            case 1:
                ExitGamePlay();
                break;
        }
    }

    /// <summary>
    /// ゲームプレイ画面に戻る
    /// </summary>
    void ReturnToGamePlay()
    {
        Pause();
    }

    /// <summary>
    /// ゲーム終了
    /// </summary>
    void ExitGamePlay()
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
