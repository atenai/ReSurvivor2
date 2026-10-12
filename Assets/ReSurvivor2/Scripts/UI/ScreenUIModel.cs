using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// スクリーンUIモデル
/// MVPパターンのModel担当
/// </summary>
public class ScreenUIModel
{
    bool isHitReticule = false;
    /// <summary>ヒットレティクルを表示するか？</summary>
    public bool IsHitReticule
    {
        get { return isHitReticule; }
        set { isHitReticule = value; }
    }
    /// <summary>ヒットレティクルが消失するスピード</summary>
    float hitReticuleSpeed = 10.0f;
    public float HitReticuleSpeed => hitReticuleSpeed;

    /// <summary>フェードの速さ</summary>
    float fadeSpeed = 1.0f;
    public float FadeSpeed => fadeSpeed;

    /// <summary>武器クイックセレクトUIを表示し続ける時間（秒）</summary>
    float weaponQuickSelectStayTime = 1.5f;
    public float WeaponQuickSelectStayTime => weaponQuickSelectStayTime;
    /// <summary>武器クイックセレクトUIのフェードインの速さ（1秒間に変化するアルファ値）</summary>
    float weaponQuickSelectFadeInSpeed = 10.0f;
    public float WeaponQuickSelectFadeInSpeed => weaponQuickSelectFadeInSpeed;
    /// <summary>武器クイックセレクトUIのフェードアウトの速さ（1秒間に変化するアルファ値）</summary>
    float weaponQuickSelectFadeOutSpeed = 2.5f;
    public float WeaponQuickSelectFadeOutSpeed => weaponQuickSelectFadeOutSpeed;
    /// <summary>武器クイックセレクトUIを表示し続ける残り時間（秒）</summary>
    float weaponQuickSelectStayTimer = 0.0f;
    public float WeaponQuickSelectStayTimer
    {
        get { return weaponQuickSelectStayTimer; }
        set { weaponQuickSelectStayTimer = value; }
    }

    /// <summary> 現在のポーズメニュー選択インデックス</summary>
    int currentPauseMenuSelectedIndex = 0;
    public int CurrentPauseMenuSelectedIndex
    {
        get { return currentPauseMenuSelectedIndex; }
        set { currentPauseMenuSelectedIndex = value; }
    }

    /// <summary> ミッションリスト </summary>
    List<MasterMissionEntity> missionList = new List<MasterMissionEntity>();
    public List<MasterMissionEntity> MissionList => missionList;
    /// <summary> メールリストのコンテンツリスト </summary>
    List<Mail> mailListContentList = new List<Mail>();
    public List<Mail> MailListContentList => mailListContentList;
    /// <summary> 現在のメールリストの選択インデックス </summary>
    int currentMailListSelectedIndex = 0;
    public int CurrentMailListSelectedIndex
    {
        get { return currentMailListSelectedIndex; }
        set { currentMailListSelectedIndex = value; }
    }
    /// <summary> コンピューターメニュー表示中か？ </summary>
    bool isComputerMenuActive = false;
    /// <summary> コンピューターメニュー表示中か？のプロパティ </summary>
    public bool IsComputerMenuActive
    {
        get { return isComputerMenuActive; }
        set { isComputerMenuActive = value; }
    }

    /// <summary>  Yes/Noダイアログ表示直後の入力を1フレームだけ無視するフラグ</summary>
    bool skipYesNoDialogInput = false;
    public bool SkipYesNoDialogInput
    {
        get { return skipYesNoDialogInput; }
        set { skipYesNoDialogInput = value; }
    }
    /// <summary> 現在選択されているYes/Noダイアログのインデックス</summary>
    int currentYesNoDialogSelectedIndex = 0;
    public int CurrentYesNoDialogSelectedIndex
    {
        get { return currentYesNoDialogSelectedIndex; }
        set { currentYesNoDialogSelectedIndex = value; }
    }
    public ScreenUIModel()
    {
    }
}
