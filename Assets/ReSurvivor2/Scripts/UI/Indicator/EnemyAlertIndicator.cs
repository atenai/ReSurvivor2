using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵がプレイヤーを最初に発見した時の方向インジケーター
/// プレイヤーを最初に発見した敵の方向に、白い矢印と「！」マークを表示する
/// ダメージ方向のインジケーターと同じく、一定時間たったらフェードアウトして消える
/// </summary>
public class EnemyAlertIndicator : MonoBehaviour
{
	[Header("UI")]
	[Tooltip("インジケーター全体（CanvasGroupのアルファ値でまとめてフェードする）")]
	[SerializeField] CanvasGroup canvasGroup;
	[Tooltip("敵の方向を指す矢印")]
	[SerializeField] RectTransform arrowTransform;
	[Tooltip("「！」マーク（敵の方向に関係なく、常に立てて表示する）")]
	[SerializeField] RectTransform alertIconTransform;

	[Header("演出")]
	[Tooltip("発見した時に表示し続ける時間（秒）。この時間がたったらフェードアウトする")]
	[SerializeField] float showTime;
	[Tooltip("フェードインの速さ（1秒間に変化するアルファ値）")]
	[SerializeField] float fadeInSpeed;
	[Tooltip("フェードアウトの速さ（1秒間に変化するアルファ値）")]
	[SerializeField] float fadeOutSpeed;
	[Tooltip("発見した瞬間の拡大率（ここから等倍まで縮みながら表示する）")]
	[SerializeField] float popScale;
	[Tooltip("発見した瞬間の拡大から等倍に戻る速さ（1秒間に変化する拡大率）")]
	[SerializeField] float popReturnSpeed;

	[Tooltip("表示中のアルファ値")]
	const float ShowAlpha = 1.0f;
	[Tooltip("非表示のアルファ値")]
	const float HideAlpha = 0.0f;
	[Tooltip("等倍の拡大率")]
	const float NormalScale = 1.0f;
	[Tooltip("表示時間の終わり（表示し続ける残り時間がこれ以下になったらフェードアウトする）")]
	const float ShowEndTime = 0.0f;

	/// <summary>インジケーターの対象とする敵のTransform</summary>
	Transform target;
	/// <summary>表示し続ける残り時間（0になったらフェードアウトする）</summary>
	float showCount = ShowEndTime;
	/// <summary>今の拡大率</summary>
	float currentScale = NormalScale;

	/// <summary>
	/// 初期化処理
	/// </summary>
	/// <param name="target">インジケーターの対象とする敵のゲームオブジェクト</param>
	public void Init(GameObject target)
	{
		this.target = target.transform;
		canvasGroup.alpha = HideAlpha;
	}

	/// <summary>
	/// 表示する（敵がプレイヤーを最初に発見した時に呼ぶ）
	/// </summary>
	public void Show()
	{
		showCount = showTime;
		//発見した瞬間は大きく表示する
		currentScale = popScale;
	}

	void Update()
	{
		if (target == null)
		{
			canvasGroup.alpha = HideAlpha;
			return;
		}

		//表示時間の間はフェードインして表示し続け、表示時間がたったらフェードアウトする
		if (ShowEndTime < showCount)
		{
			showCount = showCount - Time.deltaTime;
			canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, ShowAlpha, fadeInSpeed * Time.deltaTime);
		}
		else
		{
			canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, HideAlpha, fadeOutSpeed * Time.deltaTime);
		}

		//見えていない時は向きなどを更新しない
		if (canvasGroup.alpha <= HideAlpha)
		{
			return;
		}

		//発見した瞬間の拡大から等倍に戻す
		currentScale = Mathf.MoveTowards(currentScale, NormalScale, popReturnSpeed * Time.deltaTime);
		arrowTransform.localScale = new Vector3(currentScale, currentScale, NormalScale);
		alertIconTransform.localScale = new Vector3(currentScale, currentScale, NormalScale);

		//高さの違い（飛んでいるドローンなど）は無視して、水平方向の向きだけを使う
		//Vector3.ProjectOnPlaneは、ベクトルから指定した向き（ここでは真上）の成分を取り除く
		Vector3 toEnemy = Vector3.ProjectOnPlane(target.position - PlayerManagerPresenter.SingletonInstance.transform.position, Vector3.up);
		//真上・真下にいる時は向きが決まらないので回さない
		if (toEnemy == Vector3.zero)
		{
			return;
		}

		//カメラの向きと敵の方向の角度の差だけ回す（ダメージ方向・グレネード方向のインジケーターと同じ計算）
		float angle = PlayerCameraManager.SingletonInstance.transform.eulerAngles.y - Quaternion.LookRotation(toEnemy).eulerAngles.y;
		this.transform.localEulerAngles = Vector3.forward * angle;
		//「！」マークは矢印と一緒に回ってしまうので、逆に回して常に立てて表示する
		alertIconTransform.localEulerAngles = Vector3.forward * -angle;
	}
}
