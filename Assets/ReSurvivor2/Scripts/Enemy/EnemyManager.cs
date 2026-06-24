using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
	/// カバーポイントをセット
	/// </summary>
	void SetCoverPoint()
	{
		foreach (GameObject enemy in enemies)
		{
			enemy.GetComponentInChildren<IEnemy>().SetCoverPoints(coverPoints);
		}
	}
}
