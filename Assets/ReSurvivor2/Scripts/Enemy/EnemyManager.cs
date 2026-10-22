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
			//飛行する敵・的は、倒されると消えるので数えない
			if (enemy == null)
			{
				continue;
			}

			GroundEnemyPresenter groundEnemy = enemy.GetComponentInChildren<GroundEnemyPresenter>();
			if (groundEnemy != null && groundEnemy.GroundEnemyModel.IsAlert == true)
			{
				return true;
			}

			FlyingEnemy flyingEnemy = enemy.GetComponentInChildren<FlyingEnemy>();
			if (flyingEnemy != null && flyingEnemy.IsAlert == true)
			{
				return true;
			}
		}
		return false;
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
