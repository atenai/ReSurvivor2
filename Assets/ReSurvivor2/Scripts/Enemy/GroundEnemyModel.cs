using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地上敵モデル
/// MVPパターンのModel担当
/// </summary>
public class GroundEnemyModel
{
	[Tooltip("HP")]
	[SerializeField] HitPoint hp;
	public HitPoint HP => hp;

	[Header("追跡")]
	/// <summary>追跡中か？</summary>
	bool isChase = false;
	public bool IsChase
	{
		get { return isChase; }
		set { isChase = value; }
	}
	/// <summary>追跡時間</summary>
	float chaseTime = 30.0f;
	public float ChaseTime => chaseTime;
	/// <summary>追跡カウントタイマー</summary>
	float chaseCountTime;
	public float ChaseCountTime
	{
		get { return chaseCountTime; }
		set { chaseCountTime = value; }
	}

	/// <summary>
	/// リスポーンした際の初期化処理
	/// </summary>
	public void Initialize()
	{
		hp = new HitPoint(HitPoint.Max_Hp);
		//各種パラメーターを初期化
		isChase = false;
		chaseCountTime = 0.0f;
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
}
