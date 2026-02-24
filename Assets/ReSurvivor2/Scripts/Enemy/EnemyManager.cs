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

	[SerializeField] GroundEnemy[] groundEnemies;
	[SerializeField] FlyingEnemy[] flyingEnemies;

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

	public void AllChaseOn()
	{
		Debug.Log("<color=yellow>EnemyManagerのAllChaseOn()</color>");

		foreach (GroundEnemy groundEnemy in groundEnemies)
		{
			groundEnemy.ChaseOn();
		}

		foreach (FlyingEnemy flyingEnemy in flyingEnemies)
		{
			flyingEnemy.ChaseOn();
		}
	}
}
