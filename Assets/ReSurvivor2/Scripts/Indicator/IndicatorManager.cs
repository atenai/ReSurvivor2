using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorManager : MonoBehaviour
{
	//シングルトンで作成（ゲーム中に１つのみにする）
	static IndicatorManager singletonInstance = null;
	public static IndicatorManager SingletonInstance => singletonInstance;
	[Tooltip("敵位置表示マーカー用プレハブ")]
	[SerializeField] private GameObject indicatorPrefab;
	private Dictionary<Target, GameObject> indicatorList = new Dictionary<Target, GameObject>();   //マーカー一覧

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
		if (!indicatorList.ContainsKey(target))
		{
			GameObject gameObject = (GameObject)Instantiate(indicatorPrefab, this.transform.position, Quaternion.identity);
			gameObject.transform.SetParent(this.transform);
			indicatorList.Add(target, gameObject);
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
		if (indicatorList.ContainsKey(target))
		{
			GameObject gameObject = indicatorList[target];
			Destroy(gameObject);
			indicatorList.Remove(target);
		}
	}

	public void ShowIndicator(Target target)
	{
		if (indicatorList.ContainsKey(target))
		{
			GameObject gameObject = indicatorList[target];
			gameObject.GetComponent<Indicator>().Show();
		}
	}
}
