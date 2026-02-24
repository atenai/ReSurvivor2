using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// エネミーインジケーターマネージャー
/// </summary>
public class IndicatorManager : MonoBehaviour
{
	/// <summary> シングルトンで作成（ゲーム中に１つのみにする）</summary>
	static IndicatorManager singletonInstance = null;
	/// <summary>シングルトンのプロパティ</summary>
	public static IndicatorManager SingletonInstance => singletonInstance;
	[Tooltip("敵位置表示マーカー用プレハブ")]
	[SerializeField] private GameObject indicatorPrefab;
	private Dictionary<Target, GameObject> indicatorDic = new Dictionary<Target, GameObject>();   //マーカー一覧

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
	/// <param name="target">ターゲット</param>
	/// <returns>生成された敵表示マーカー</returns>
	public void InstanceIndicator(Target target)
	{
		//既に含まれていたら追加しない
		if (!indicatorDic.ContainsKey(target))
		{
			GameObject gameObject = (GameObject)Instantiate(indicatorPrefab, this.transform.position, Quaternion.identity);
			gameObject.transform.SetParent(this.transform);
			indicatorDic.Add(target, gameObject);
			gameObject.GetComponent<Indicator>().Init(target.gameObject);
		}
		else
		{
			Debug.LogError("EnemySensoreManager InstanceSensorMarker Failure.");
		}
	}

	/// <summary>
	/// 敵表示マーカーの削除
	/// </summary>
	/// <param name="target">ターゲット</param>
	public void DeleteIndicator(Target target)
	{
		if (indicatorDic.ContainsKey(target))
		{
			GameObject gameObject = indicatorDic[target];
			Destroy(gameObject);
			indicatorDic.Remove(target);
		}
	}

	/// <summary>
	/// 敵表示マーカーを表示する
	/// </summary>
	/// <param name="target"></param>
	public void ShowIndicator(Target target)
	{
		if (indicatorDic.ContainsKey(target))
		{
			GameObject gameObject = indicatorDic[target];
			gameObject.GetComponent<Indicator>().Show();
		}
	}
}
