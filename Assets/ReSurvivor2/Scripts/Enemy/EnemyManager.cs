using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// エネミーマネージャー
/// </summary>
public class EnemyManager : MonoBehaviour
{
	/// <summary> シングルトンで作成（ゲーム中に１つのみにする）</summary>
	static EnemyManager singletonInstance = null;
	/// <summary>シングルトンのプロパティ</summary>
	public static EnemyManager SingletonInstance => singletonInstance;

	[SerializeField] List<GameObject> enemies;

	[Tooltip("カバーポイント")]
	[SerializeField] CoverPoint[] coverPoints;

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
	/// カバーポイントをセット
	/// </summary>
	void SetCoverPoint()
	{
		foreach (GameObject enemy in enemies)
		{
			enemy.GetComponentInChildren<IEnemy>().SetCoverPoints(coverPoints);
		}
	}

	/// <summary>
	/// 全員が警戒態勢
	/// </summary>
	public void AllChaseOn()
	{
		Debug.Log("<color=yellow>EnemyManagerのAllChaseOn()</color>");

		foreach (GameObject enemy in enemies)
		{
			enemy.GetComponentInChildren<IEnemy>().ChaseOn();
		}
	}

	/// <summary>
	/// リストからエネミーの削除
	/// </summary>
	/// <param name="enemy"></param>
	public void RemoveEnemyList(GameObject enemy)
	{
		if (enemies.Contains(enemy))
		{
			enemies.Remove(enemy);
		}
	}
}
