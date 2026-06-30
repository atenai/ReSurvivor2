using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScreenUIView : MonoBehaviour
{
    [Header("照準")]

    [Tooltip("クロスヘアー")]
    [SerializeField] Image imageCrosshair;

    [Tooltip("ヒットレティクル")]
    [SerializeField] Image imageHitReticule;
    public Image ImageHitReticule => imageHitReticule;

    [Header("画面エフェクト")]
    [Tooltip("ダメージ画像エフェクト")]
    [SerializeField] Image imageDamage;
    public Image ImageDamage => imageDamage;

    [Tooltip("HP回復画像エフェクト")]
    [SerializeField] Image imageHpHeal;
    public Image ImageHpHeal => imageHpHeal;

    [Tooltip("スタミナ回復画像エフェクト")]
    [SerializeField] Image imageStaminaHeal;
    public Image ImageStaminaHeal => imageStaminaHeal;

    [Header("ロード")]
    [Tooltip("ロード画面")]
    [SerializeField] Image panelLoading;
    public Image PanelLoading => panelLoading;

    [Tooltip("ロードスライダー")]
    [SerializeField] Slider sliderLoading;
    public Slider SliderLoading
    {
        get { return sliderLoading; }
        set { sliderLoading = value; }
    }

    [Header("フェード")]
    [Tooltip("フェード用画像")]
    [SerializeField] Image imageFade;
    public Image ImageFade => imageFade;

    [Header("ポーズUI")]
    [Tooltip("ポーズメニューパネル")]
    [SerializeField] Image panelPauseMenu;
    public Image PanelPauseMenu => panelPauseMenu;
    [Tooltip("ポーズメニューオプションのイメージリスト")]
    [SerializeField] Image[] pauseMenuOptions;
    public Image[] PauseMenuOptions => pauseMenuOptions;

    [Header("マップUI")]
    [Tooltip("マップイメージ")]
    [SerializeField] MapUI mapUI;
    public MapUI MapUI => mapUI;

    [Header("アイテム取得")]
    [Tooltip("アイテム取得ログ")]
    [SerializeField] ItemOutPutLog itemOutPutLog;
    public ItemOutPutLog ItemOutPutLog => itemOutPutLog;

    [Header("セーブ")]
    [Tooltip("セーブテキスト")]
    [SerializeField] TextMeshProUGUI saveNowText;
    public TextMeshProUGUI SaveNowText => saveNowText;

    [Header("コンピューターUI")]
    [Tooltip("コンピューターメニューパネル")]
    [SerializeField] Image panelComputerMenu;
    public Image PanelComputerMenu => panelComputerMenu;
    [Tooltip("メールリストコンテンツ")]
    [SerializeField] GameObject mailListContent;
    public GameObject MailListContent => mailListContent;
    [Tooltip("メールプレハブ")]
    [SerializeField] GameObject mailPrefab;
    public GameObject MailPrefab => mailPrefab;
    [Tooltip("メールメッセージタイトル")]
    [SerializeField] Text mailTitleMessage;
    public Text MailTitleMessage => mailTitleMessage;
    [Tooltip("メールメッセージ本文")]
    [SerializeField] Text mailMainMessage;
    public Text MailMainMessage => mailMainMessage;

    [Tooltip("Yes/Noダイアログ")]
    [SerializeField] Image yesnoDialogImage;
    public Image YesNoDialogImage => yesnoDialogImage;
    [Tooltip("Yes/Noボタンのイメージリスト")]
    [SerializeField] Image[] yesnoButtonImages;
    public Image[] YesNoButtonImages => yesnoButtonImages;

    [Tooltip("リザルト画面")]
    [SerializeField] Image resultImage;
    public Image ResultImage => resultImage;

    void Start()
    {
        //ヒットレティクル
        imageHitReticule.color = Color.clear;

        //ダメージ画像エフェクト
        imageDamage.color = Color.clear;

        //HP回復画像エフェクト
        imageHpHeal.color = Color.clear;

        //スタミナ回復画像エフェクト
        imageStaminaHeal.color = Color.clear;

        //セーブ表示用テキストを透明にする
        saveNowText.color = new Color(saveNowText.color.r, saveNowText.color.g, saveNowText.color.b, 0f);

        panelComputerMenu.gameObject.SetActive(false);

        // リザルト画像は初期で非表示・透明にしておく
        resultImage.color = new Color(resultImage.color.r, resultImage.color.g, resultImage.color.b, 0f);
        resultImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// クロスヘア（レティクル）の表示/非表示かつ色の切り替え
    /// </summary>
    public void Crosshair(bool isAim, bool isTargetHit)
    {
        if (isAim == true)
        {
            imageCrosshair.gameObject.SetActive(true);

            if (isTargetHit == true)
            {
                imageCrosshair.color = new Color32(255, 0, 0, 150);
            }
            else
            {
                imageCrosshair.color = new Color32(255, 255, 255, 150);
            }

            return;
        }

        imageCrosshair.gameObject.SetActive(false);
    }
}
