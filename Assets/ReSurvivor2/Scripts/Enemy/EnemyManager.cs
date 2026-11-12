using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// エネミーを管理するマネージャークラス
/// </summary>
public class EnemyManager : MonoBehaviour
{
	/// <summary> シングルトンで作成（ゲーム中に１つのみにする）</summary>
	static EnemyManager singletonInstance = null;
	/// <summary>シングルトンのプロパティ</summary>
	public static EnemyManager SingletonInstance => singletonInstance;

	[Tooltip("エネミー")]
	[SerializeField] List<GameObject> enemies;

	[Tooltip("カバーポイント")]
	[SerializeField] CoverPoint[] coverPoints;

	UnityEvent chaseEvent = new UnityEvent();
	public UnityEvent ChaseEvent => chaseEvent;

	void Awake()
	{
		//staticな変数instanceはメモリ領域は確保されていますが、初回では中身が入っていないので、中身を入れます。
		if (singletonInstance == null)
		{
			singletonInstance = this;//thisというのは自分自身のインスタンスという意味になります。この場合、Playerのインスタンスという意味になります。
		}
		else
		{
			Destroy(this.gameObject);//中身がすでに入っていた場合、自身のインスタンスがくっついているゲームオブジェクトを破棄します。
		}
	}

	void Start()
	{
		SetCoverPoint();
	}

	/// <summary>
	/// 警戒体制の敵がいるか？（プレイヤーを追跡中で、頭の上にアラートのマークが出ている敵がいるか？）
	/// </summary>
	public bool IsAnyEnemyAlert()
	{
		foreach (GameObject enemy in enemies)
		{
			if (GetAlertEnemy(enemy) != null)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// プレイヤーの銃声で、敵全員にプレイヤーを追跡させる
	/// まだ警戒体制の敵が1体もいなかった時は、プレイヤーに一番近い敵が最初に気づいたことにして、その敵の方向に発見マーカーを表示する
	/// </summary>
	public void ChaseByGunshot()
	{
		//追跡を始めると全員が警戒体制になるので、追跡を始める前に調べておく
		bool isAnyEnemyAlertBeforeGunshot = IsAnyEnemyAlert();

		chaseEvent.Invoke();

		//既に警戒体制の敵がいた時は、最初に気づいた敵ではないので表示しない
		if (isAnyEnemyAlertBeforeGunshot == true)
		{
			return;
		}

		IEnemy nearestEnemy = FindNearestAlertEnemy();
		if (nearestEnemy != null)
		{
			EnemyIndicatorManager.SingletonInstance.ShowAlertIndicator(nearestEnemy);
		}
	}

	/// <summary>
	/// プレイヤーに一番近い、警戒体制の敵を探す
	/// </summary>
	/// <returns>一番近い警戒体制の敵（いない時はnull）</returns>
	IEnemy FindNearestAlertEnemy()
	{
		Vector3 playerPosition = PlayerManagerPresenter.SingletonInstance.transform.position;
		IEnemy nearestEnemy = null;
		float nearestSqrDistance = float.MaxValue;
		foreach (GameObject enemy in enemies)
		{
			IEnemy alertEnemy = GetAlertEnemy(enemy);
			if (alertEnemy == null)
			{
				continue;
			}

			//近さを比べるだけなので、平方根の計算がいらない「距離の2乗」（sqrMagnitude）で比べる
			float sqrDistance = (alertEnemy.GetEnemyGameObject().transform.position - playerPosition).sqrMagnitude;
			if (sqrDistance < nearestSqrDistance)
			{
				nearestSqrDistance = sqrDistance;
				nearestEnemy = alertEnemy;
			}
		}
		return nearestEnemy;
	}

	/// <summary>
	/// 警戒体制の敵なら、その敵を返す（プレイヤーを追跡中で、頭の上にアラートのマークが出ている敵）
	/// </summary>
	/// <param name="enemy">エネミーのリストに入っているゲームオブジェクト</param>
	/// <returns>警戒体制の敵（警戒体制ではない時・倒されて消えた時はnull）</returns>
	IEnemy GetAlertEnemy(GameObject enemy)
	{
		//飛行する敵・的は、倒されると消えるので数えない
		if (enemy == null)
		{
			return null;
		}

		GroundEnemyPresenter groundEnemy = enemy.GetComponentInChildren<GroundEnemyPresenter>();
		if (groundEnemy != null && groundEnemy.GroundEnemyModel.IsAlert == true)
		{
			return groundEnemy;
		}

		FlyingEnemyPresenter flyingEnemy = enemy.GetComponentInChildren<FlyingEnemyPresenter>();
		if (flyingEnemy != null && flyingEnemy.FlyingEnemyModel.IsAlert == true)
		{
			return flyingEnemy;
		}

		return null;
	}

	/// <summary>
	/// カバーポイントをセット
	/// </summary>
	void SetCoverPoint()
	{
		foreach (GameObject enemy in enemies)
		{
			if (enemy.GetComponentInChildren<GroundEnemyPresenter>() == true)
			{
				enemy.GetComponentInChildren<GroundEnemyPresenter>().SetCoverPoints(coverPoints);
			}
		}
	}
}
