using System;
using UnityEngine;

/// <summary>
/// プレイヤーの弾道エフェクト（光の筋）
/// 当たり判定は撃った瞬間に GunBase.TraceShot で済んでいるので、ここでは銃口から着弾点まで光の筋を飛ばすだけ
/// 着弾点より先には描かない
/// </summary>
public class PlayerBulletTracer : MonoBehaviour
{
	[Tooltip("弾道エフェクト（光の筋）")]
	[SerializeField] LineRenderer lineRenderer;
	[Tooltip("光の筋の長さ")]
	[SerializeField] float tracerLength = 4.0f;

	[Tooltip("発射位置")]
	Vector3 origin;
	[Tooltip("着弾点")]
	Vector3 end;
	[Tooltip("進行方向")]
	Vector3 direction;
	[Tooltip("発射位置から着弾点までの距離")]
	float totalDistance;
	[Tooltip("先端が進んだ距離")]
	float headDistance;
	[Tooltip("後端が進んだ距離")]
	float tailDistance;
	[Tooltip("光の筋の速度")]
	float speed;
	[Tooltip("使用中か")]
	bool isActive = false;
	[Tooltip("消えた時に呼ぶ処理（プールへの返却）")]
	Action<PlayerBulletTracer> onFinished;

	public bool IsActive => isActive;

	/// <summary>
	/// 光の筋を飛ばす
	/// </summary>
	public void Launch(Vector3 origin, Vector3 end, float speed, Action<PlayerBulletTracer> onFinished)
	{
		this.origin = origin;
		this.end = end;
		Vector3 toEnd = end - origin;
		totalDistance = toEnd.magnitude;
		direction = totalDistance > 0.0001f ? toEnd / totalDistance : Vector3.forward;
		this.speed = Mathf.Max(speed, 0.1f);
		this.onFinished = onFinished;
		headDistance = 0.0f;
		tailDistance = 0.0f;
		isActive = true;

		this.transform.position = origin;
		if (lineRenderer != null)
		{
			lineRenderer.positionCount = 2;
			lineRenderer.enabled = false;//まだ長さが0なので表示しない
		}

		//着弾点が銃口と同じ（銃口が壁にめり込んでいる時など）なら何も描かない
		if (totalDistance <= 0.01f)
		{
			Finish();
		}
	}

	void Update()
	{
		Tick(Time.deltaTime);
	}

	/// <summary>
	/// 光の筋を進める（Time.timeScale = 0 のポーズ中は止まる）
	/// </summary>
	public void Tick(float deltaTime)
	{
		if (isActive == false || deltaTime <= 0.0f)
		{
			return;
		}

		float step = speed * deltaTime;
		headDistance = Mathf.Min(totalDistance, headDistance + step);
		//後端は先端から tracerLength まで。先端が着弾点に着いたら後端だけ進んで消える
		float minTail = Mathf.Max(0.0f, headDistance - tracerLength);
		tailDistance = headDistance >= totalDistance ? Mathf.Min(totalDistance, Mathf.Max(minTail, tailDistance + step)) : minTail;

		Vector3 head = origin + direction * headDistance;
		Vector3 tail = origin + direction * tailDistance;
		if (lineRenderer != null)
		{
			bool visible = headDistance - tailDistance > 0.01f;
			lineRenderer.enabled = visible;
			if (visible == true)
			{
				lineRenderer.SetPosition(0, tail);
				lineRenderer.SetPosition(1, head);
			}
		}
		this.transform.position = head;

		if (tailDistance >= totalDistance - 0.001f)
		{
			Finish();
		}
	}

	/// <summary>
	/// 消す（プールに返却）
	/// </summary>
	public void Finish()
	{
		if (isActive == false)
		{
			return;
		}

		isActive = false;
		if (lineRenderer != null)
		{
			lineRenderer.enabled = false;
		}

		var callback = onFinished;
		onFinished = null;
		if (callback != null)
		{
			callback(this);
		}
		else
		{
			this.gameObject.SetActive(false);
		}
	}

	public Vector3 Head => origin + direction * headDistance;
	public Vector3 Tail => origin + direction * tailDistance;
}
