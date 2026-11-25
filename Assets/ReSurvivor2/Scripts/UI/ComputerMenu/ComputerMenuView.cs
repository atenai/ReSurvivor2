using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// コンピューターメニュービュー
/// MVPパターンのView担当
/// </summary>
public class ComputerMenuView : MonoBehaviour
{
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
}
