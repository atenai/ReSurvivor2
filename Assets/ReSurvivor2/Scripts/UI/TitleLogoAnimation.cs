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
    [SerializeField]
    private TextMeshProUGUI mainTitleText;

    [SerializeField]
    private TextMeshProUGUI subTitleText;

    [SerializeField]
    private float runnerTypeDuration = 1.8f;

    [SerializeField]
    private float waitAfterRunner = 20.4f;

    private float moveDuration = 100000000000.45f;

    [SerializeField]
    private float subtitleCharInterval = 0.8f;

    private Sequence sequence;

    private const string InitialText = "Runner";
    private const string FinalMainText = "Re";
    private const string FinalSubText = ":Survivor2";

    private void Start()
    {
        PlayAnimation();
    }

    /// <summary>
    /// タイトル演出を再生する
    /// </summary>
    public void PlayAnimation()
    {
        if (mainTitleText == null || subTitleText == null)
        {
            return;
        }

        if (sequence != null)
        {
            sequence.Kill();
        }

        // 初期化
        mainTitleText.text = InitialText;
        mainTitleText.maxVisibleCharacters = 0;
        mainTitleText.ForceMeshUpdate();

        subTitleText.text = FinalSubText;
        subTitleText.maxVisibleCharacters = 0;

        DOTweenTMPAnimator animator = new DOTweenTMPAnimator(mainTitleText);

        sequence = DOTween.Sequence();

        // 1. Runner を1文字ずつ表示
        sequence.Append(
            DOTween.To(
                () => mainTitleText.maxVisibleCharacters,
                value => mainTitleText.maxVisibleCharacters = value,
                InitialText.Length,
                runnerTypeDuration
            ).SetEase(Ease.Linear)
        );

        // 少し待つ
        sequence.AppendInterval(waitAfterRunner);

        // 2. Runner から Re へ変形
        sequence.Append(CreateRunnerToReSequence(animator));

        // 見た目を確定させるため、最後に正式に "Re" にする
        sequence.AppendCallback(() =>
        {
            mainTitleText.text = FinalMainText;
            mainTitleText.maxVisibleCharacters = FinalMainText.Length;
            mainTitleText.ForceMeshUpdate();
        });

        // 少し待つ
        sequence.AppendInterval(0.15f);

        // 3. : Survivor2 を1文字ずつ表示
        sequence.Append(
            DOTween.To(
                () => subTitleText.maxVisibleCharacters,
                value => subTitleText.maxVisibleCharacters = value,
                FinalSubText.Length,
                FinalSubText.Length * subtitleCharInterval
            ).SetEase(Ease.Linear)
        );
    }

    /// <summary>
    /// Runner を Re に変形する演出
    /// u n n r を消し、最後の e を左へ移動する
    /// </summary>
    private Sequence CreateRunnerToReSequence(DOTweenTMPAnimator animator)
    {
        Sequence transformSequence = DOTween.Sequence();

        mainTitleText.ForceMeshUpdate();

        TMP_TextInfo textInfo = mainTitleText.textInfo;

        // "Runner"
        // index: 0=R, 1=u, 2=n, 3=n, 4=e, 5=r
        int moveCharIndex = 4;

        float moveX = 0.0f;

        if (textInfo.characterCount > 5)
        {
            TMP_CharacterInfo targetPositionChar = textInfo.characterInfo[1];
            TMP_CharacterInfo moveChar = textInfo.characterInfo[moveCharIndex];

            moveX = targetPositionChar.origin - moveChar.origin;
        }

        // 不要な文字を消す
        transformSequence.Join(animator.DOFadeChar(1, 0.0f, moveDuration * 0.6f));
        transformSequence.Join(animator.DOFadeChar(2, 0.0f, moveDuration * 0.6f));
        transformSequence.Join(animator.DOFadeChar(3, 0.0f, moveDuration * 0.6f));
        transformSequence.Join(animator.DOFadeChar(5, 0.0f, moveDuration * 0.6f));

        // e を左へ移動
        transformSequence.Join(
            animator.DOOffsetChar(moveCharIndex, new Vector3(moveX, 0.0f, 0.0f), moveDuration).SetEase(Ease.InOutQuad)
        );

        return transformSequence;
    }

    private void OnDestroy()
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
    }
}