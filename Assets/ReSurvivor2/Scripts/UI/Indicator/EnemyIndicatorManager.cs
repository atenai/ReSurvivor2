using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// エネミー位置表示マーカーの管理クラス
/// </summary>
public class EnemyIndicatorManager : MonoBehaviour
{
	/// <summary> シングルトンで作成（ゲーム中に１つのみにする）</summary>
	static EnemyIndicatorManager singletonInstance = null;
	/// <summary>シングルトンのプロパティ</summary>
	public static EnemyIndicatorManager SingletonInstance => singletonInstance;

	[Tooltip("敵位置表示マーカー用プレハブ")]
	[SerializeField] GameObject indicatorPrefab;
	Dictionary<IEnemy, GameObject> indicatorDic = new Dictionary<IEnemy, GameObject>();

	/// <summary>
	/// 初期化処理
	/// </summary>
	void Awake()
	{
		//staticな変数instanceはメモリ領域は確保されていますが、初回では中身が入っていないので、中身を入れます。
		if (singletonInstance == null)
		{
			singletonInstance = this;//thisというのは自分自身のインスタンスという意味になります。この場合、Playerのインスタンスという意味になります。
			DontDestroyOnLoad(this.gameObject);//シーンを切り替えた時に破棄しない
		}
		else
		{
			Destroy(this.gameObject);//中身がすでに入っていた場合、自身のインスタンスがくっついているゲームオブジェクトを破棄します。
		}
	}

	/// <summary>
	/// 敵表示マーカー生成
	/// </summary>
	/// <param name="enemy">ターゲット</param>
	/// <returns>生成された敵表示マーカー</returns>
	public void InstanceIndicator(IEnemy enemy)
	{
		//既に含まれていたら追加しない
		if (!indicatorDic.ContainsKey(enemy))
		{
			GameObject gameObject = Instantiate(indicatorPrefab, this.transform.position, Quaternion.identity);
			gameObject.transform.SetParent(this.transform);
			indicatorDic.Add(enemy, gameObject);
			gameObject.GetComponent<Indicator>().Init(enemy.GetEnemyGameObject());
		}
		else
		{
			Debug.LogError("EnemySensoreManager InstanceSensorMarker Failure.");
		}
	}

	/// <summary>
	/// 敵表示マーカーの削除
	/// </summary>
	/// <param name="enemy">ターゲット</param>
	public void DeleteIndicator(IEnemy enemy)
	{
		if (indicatorDic.ContainsKey(enemy))
		{
			GameObject gameObject = indicatorDic[enemy];
			Destroy(gameObject);
			indicatorDic.Remove(enemy);
		}
	}

	/// <summary>
	/// 敵表示マーカーを表示する
	/// </summary>
	/// <param name="enemy"></param>
	public void ShowIndicator(IEnemy enemy)
	{
		if (indicatorDic.ContainsKey(enemy))
		{
			GameObject gameObject = indicatorDic[enemy];
			gameObject.GetComponent<Indicator>().Show();
		}
	}
}
