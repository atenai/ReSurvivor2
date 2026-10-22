using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地上敵モデル
/// MVPパターンのModel担当
/// </summary>
[Serializable]
public class GroundEnemyModel
{
	[Tooltip("HP")]
	[SerializeField] HitPoint hp;
	public HitPoint HP => hp;

	[Header("追跡")]
	/// <summary>追跡時間</summary>
	[SerializeField] float chaseTime = 30.0f;
	public float ChaseTime => chaseTime;
	/// <summary>追跡カウントタイマー</summary>
	float chaseCountTime;
	public float ChaseCountTime
	{
		get { return chaseCountTime; }
		set { chaseCountTime = value; }
	}
	/// <summary>追跡中か？</summary>
	bool isChase = false;
	public bool IsChase
	{
		get { return isChase; }
		set { isChase = value; }
	}
	/// <summary>警戒体制か？（プレイヤーを追跡中で、倒されていない。頭の上にアラートのマークが出ている状態）</summary>
	public bool IsAlert => isChase == true && hp.IsDead == false;

	[Header("レイキャスト")]
	/// <summary>視界な長さ</summary>
	[SerializeField] float rayDistance = 14.0f;
	/// <summary>左右の最大角</summary>
	[SerializeField] float halfAngle = 45f;
	/// <summary>度/秒（例：90なら1秒で90度回る）</summary>
	[SerializeField] float sweepSpeed = 180f;
	/// <summary>現在の角度</summary>
	float currentAngle = 0f;
	/// <summary>角度の回転方向（+1で右へ、-1で左へ）</summary>
	float angleDir = 1f;

	[Header("攻撃")]
	[Tooltip("交戦距離")]
	[SerializeField] float engagementDistance = 14.0f;
	public float EngagementDistance => engagementDistance;

	[Tooltip("銃の現在のマガジンの弾数")]
	int currentMagazine;
	public int CurrentMagazine
	{
		get { return currentMagazine; }
		set { currentMagazine = value; }
	}
	[Tooltip("銃の最大マガジン数")]
	[SerializeField] int magazineCapacity = 8;
	public int MagazineCapacity => magazineCapacity;
	[Tooltip("銃のリロードのオン・オフ")]
	bool isReloadTimeActive = false;
	public bool IsReloadTimeActive
	{
		get { return isReloadTimeActive; }
		set { isReloadTimeActive = value; }
	}

	[Tooltip("現在のグレネード数")]
	[SerializeField] int currentGrenade = 3;
	public int CurrentGrenade
	{
		get { return currentGrenade; }
		set { currentGrenade = value; }
	}

	/// <summary>
	/// リスポーンした際の初期化処理
	/// </summary>
	public void Initialize()
	{
		hp = new HitPoint(HitPoint.Max_Hp);
		//各種パラメーターを初期化
		InitChase();
		InitGunMagazine();
	}

	/// <summary>
	/// 追跡の初期化
	/// </summary>
	void InitChase()
	{
		isChase = false;
		chaseCountTime = 0.0f;
	}

	/// <summary>
	/// 銃の弾数の初期化処理
	/// </summary>
	void InitGunMagazine()
	{
		CurrentMagazine = MagazineCapacity;
		isReloadTimeActive = false;
	}

	/// <summary>
	/// 追跡開始
	/// </summary>
	public void ChaseOn()
	{
		//Debug.Log("<color=red>GroundEnemyのChaseOn()</color>");
		isChase = true;
		chaseCountTime = chaseTime;
	}

	/// <summary>
	/// プレイヤーを追跡中の処理
	/// </summary>
	public void ChasePlayer()
	{
		if (isChase == true)
		{
			chaseCountTime -= Time.deltaTime;
			const float minTimer = 0.0f;
			if (chaseCountTime <= minTimer)
			{
				isChase = false;
			}
		}
	}

	/// <summary>
	/// レイキャストによる視界
	/// </summary>
	public bool Eyesight(Vector3 pos, Vector3 forward, Vector3 up)
	{
		if (hp.IsDead == true)
		{
			return false;
		}

		// 角度を進める
		currentAngle = currentAngle + angleDir * sweepSpeed * Time.deltaTime;

		// 端で反転（往復）
		if (halfAngle < currentAngle)
		{
			currentAngle = halfAngle;
			angleDir = -1f;
		}
		else if (currentAngle < -halfAngle)
		{
			currentAngle = -halfAngle;
			angleDir = 1f;
		}

		if (forward.sqrMagnitude < 0.0001f)
		{
			forward = Vector3.forward;
		}
		forward.Normalize();

		// 今の角度だけ回した方向に1本Ray
		Vector3 dir = Quaternion.AngleAxis(currentAngle, up) * forward;

		Debug.DrawRay(pos, dir * rayDistance, Color.yellow);

		RaycastHit hit;
		if (Physics.Raycast(pos, dir, out hit, rayDistance))
		{
			// ヒット時の処理（例：プレイヤー検知など）
			if (hit.collider.CompareTag("Player"))
			{
				return true;
			}
		}

		return false;
	}
}
