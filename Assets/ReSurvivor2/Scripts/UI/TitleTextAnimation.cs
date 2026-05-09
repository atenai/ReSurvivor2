using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// タイトル画面の文字演出を管理するクラス
/// </summary>
public class TitleTextAnimation : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;

    [SerializeField] float typeSpeed = 0.12f;

    [SerializeField] float waitTime = 0.8f;

    Sequence sequence;

    void Start()
    {
        PlayTitleAnimation();
    }

    /// <summary>
    /// タイトル演出を再生する
    /// </summary>
    void PlayTitleAnimation()
    {
        titleText.text = "";
        titleText.transform.localScale = Vector3.one;

        sequence = DOTween.Sequence();

        // Runner を1文字ずつ表示
        sequence.Append(CreateTypeTextTween("Runner"));

        // 少し待つ
        sequence.AppendInterval(waitTime);

        // Runner から Re だけ残す
        sequence.Append(CreateBackspaceTween("Runner", "Re"));

        // 少し待つ
        sequence.AppendInterval(waitTime);

        // Re の後ろに : Survivor2 を追加
        sequence.Append(CreateAddTextTween("Re", ": Survivor2"));

        // 最後に軽く強調
        sequence.Append(titleText.transform.DOScale(1.08f, 0.25f));
        sequence.Append(titleText.transform.DOScale(1.0f, 0.25f));
    }

    /// <summary>
    /// 文字を1文字ずつ表示するTweenを作成する
    /// </summary>
    Tween CreateTypeTextTween(string targetText)
    {
        int currentLength = 0;

        Tween tween = DOTween.To(
            () => currentLength,
            value =>
            {
                currentLength = value;
                titleText.text = targetText.Substring(0, currentLength);
            },
            targetText.Length,
            targetText.Length * typeSpeed
        );

        tween.SetEase(Ease.Linear);

        return tween;
    }

    /// <summary>
    /// 文字を後ろから消して、指定した文字数だけ残すTweenを作成する
    /// </summary>
    Tween CreateBackspaceTween(string beforeText, string afterText)
    {
        int currentLength = beforeText.Length;

        Tween tween = DOTween.To(
            () => currentLength,
            value =>
            {
                currentLength = value;
                titleText.text = beforeText.Substring(0, currentLength);
            },
            afterText.Length,
            (beforeText.Length - afterText.Length) * typeSpeed
        );

        tween.SetEase(Ease.Linear);

        return tween;
    }

    /// <summary>
    /// 既にある文字の後ろに、追加文字を1文字ずつ表示するTweenを作成する
    /// </summary>
    Tween CreateAddTextTween(string baseText, string addText)
    {
        int currentLength = 0;

        Tween tween = DOTween.To(
            () => currentLength,
            value =>
            {
                currentLength = value;
                titleText.text = baseText + addText.Substring(0, currentLength);
            },
            addText.Length,
            addText.Length * typeSpeed
        );

        tween.SetEase(Ease.Linear);

        return tween;
    }

    void OnDestroy()
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
    }
}