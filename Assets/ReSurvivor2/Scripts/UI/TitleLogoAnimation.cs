using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// タイトル文字演出クラス
/// Runner → Re → Re: Survivor2
/// の流れを再生する
/// </summary>
public class TitleLogoAnimation : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI mainTitleText;

    [SerializeField] TextMeshProUGUI subTitleText;

    Sequence sequence;

    const string InitialText = "Runner";
    const string FinalSubText = ":Survivor2";

    void Start()
    {
        PlayAnimation();
    }

    /// <summary>
    /// タイトル演出を再生する
    /// </summary>
    void PlayAnimation()
    {
        if (sequence != null)
        {
            sequence.Kill();
        }

        sequence = DOTween.Sequence();

        // 初期化
        mainTitleText.text = InitialText;
        mainTitleText.ForceMeshUpdate();

        subTitleText.text = FinalSubText;
        subTitleText.maxVisibleCharacters = 0;
        subTitleText.ForceMeshUpdate();

        DOTweenTMPAnimator animator = new DOTweenTMPAnimator(mainTitleText);

        // Runner のまま少し待つ
        float waitAfterRunner = 1f;
        sequence.AppendInterval(waitAfterRunner);

        // Runner から Re へ変形
        sequence.Append(CreateRunnerToReSequence(animator));

        // 変形後少し待つ
        float waitAfterTransform = 0.05f;
        sequence.AppendInterval(waitAfterTransform);

        // :Survivor2 を1文字ずつ表示
        float subtitleCharInterval = 0.1f;
        sequence.Append(
            DOTween.To(
                () => subTitleText.maxVisibleCharacters,
                value => subTitleText.maxVisibleCharacters = value,
                FinalSubText.Length,
                FinalSubText.Length * subtitleCharInterval
            ).SetEase(Ease.Linear)
        );

        sequence.Play();
    }

    /// <summary>
    /// Runner を Re に変形する演出
    /// u n n r を消し、最後の e を左へ移動する
    /// </summary>
    Sequence CreateRunnerToReSequence(DOTweenTMPAnimator animator)
    {
        Sequence transformSequence = DOTween.Sequence();

        // "Runner"
        // index: 0=R, 1=u, 2=n, 3=n, 4=e, 5=r
        mainTitleText.ForceMeshUpdate();
        TMP_TextInfo textInfo = mainTitleText.textInfo;
        int moveCharIndex = 4;
        float moveX = 0.0f;
        if (textInfo.characterCount > 5)
        {
            TMP_CharacterInfo targetPositionChar = textInfo.characterInfo[1];
            TMP_CharacterInfo moveChar = textInfo.characterInfo[moveCharIndex];
            moveX = targetPositionChar.origin - moveChar.origin;
        }

        //0.5秒かけて赤に変更
        float colorChangeDuration = 0.5f;
        transformSequence.Join(animator.DOColorChar(0, Color.red, colorChangeDuration))
        .Join(animator.DOColorChar(4, Color.red, colorChangeDuration));

        // 色変更後少し待つ
        float waitAfterColorChange = 3.0f;
        transformSequence.AppendInterval(waitAfterColorChange);

        // 不要な文字を消す
        float fadeDuration = 0.25f;
        transformSequence.Append(animator.DOFadeChar(1, 0.0f, fadeDuration))
        .Join(animator.DOFadeChar(2, 0.0f, fadeDuration))
        .Join(animator.DOFadeChar(3, 0.0f, fadeDuration))
        .Join(animator.DOFadeChar(5, 0.0f, fadeDuration));

        // フェード後少し待つ
        float waitAfterFade = 0.5f;
        transformSequence.AppendInterval(waitAfterFade);

        // e を左へ移動
        float moveDuration = 0.25f;
        transformSequence.Join(animator.DOOffsetChar(moveCharIndex, new Vector3(moveX, 0.0f, 0.0f), moveDuration).SetEase(Ease.Linear));

        return transformSequence;
    }

    void OnDestroy()
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
    }
}