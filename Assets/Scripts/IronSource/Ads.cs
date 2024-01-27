using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//アイアンソースのスクリプト
public class Ads : MonoBehaviour
{
    public TextMeshProUGUI RewardStatusText;
    public TextMeshProUGUI InterstitialStatusText;

    private void Awake()
    {
        //IronSourceを使う為の初期化処理（第一引数に、APP KEY:の後に書いてある数字を文字列にして入れる）
        IronSource.Agent.init("16a036d7d", IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.OFFERWALL, IronSourceAdUnits.BANNER);
    }

    void OnEnable()
    {
        IronSourceEvents.onInterstitialAdOpenedEvent += InterstitialAdOpenedEvent;
    }

    void Start()
    {
        IsRewardReady();
        IsInterstitialReady();
        LoadInterstitial();

        //バナー広告の表示
        IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);
    }

    void Update()
    {
        IsRewardReady();
        IsInterstitialReady();
    }

    void OnDisable()
    {
        IronSourceEvents.onInterstitialAdClickedEvent -= InterstitialAdOpenedEvent;
    }

    //インターステーショナル広告をオープンにした際に行われる関数
    void InterstitialAdOpenedEvent()
    {
        LoadInterstitial();
    }

    //インターステーショナル広告のロード
    public void LoadInterstitial()
    {
        IronSource.Agent.loadInterstitial();
    }

    //インターステーショナル広告の準備ができたか？どうか
    void IsInterstitialReady()
    {
        InterstitialStatusText.text = IronSource.Agent.isInterstitialReady().ToString();
    }

    //インターステーショナル広告の表示
    public void ShowInterstitial()
    {
        IronSource.Agent.showInterstitial();
    }

    //リワード広告の準備ができたか？どうか
    void IsRewardReady()
    {
        RewardStatusText.text = IronSource.Agent.isRewardedVideoAvailable().ToString();
    }

    //リワード広告を表示
    public void ShowReward()
    {
        IronSource.Agent.showRewardedVideo();
    }
}
