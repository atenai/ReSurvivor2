using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeIndicatorManager : MonoBehaviour
{
	//シングルトンで作成（ゲーム中に１つのみにする）
	static GrenadeIndicatorManager singletonInstance = null;
	public static GrenadeIndicatorManager SingletonInstance => singletonInstance;
	[Tooltip("グレネード位置表示マーカー用プレハブ")]
	[SerializeField] private GameObject grenadeIndicatorPrefab;
	private Dictionary<Grenade, GameObject> indicatorDic = new Dictionary<Grenade, GameObject>();   //マーカー一覧

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
	/// グレネード表示マーカー生成
	/// </summary>
	public void InstanceIndicator(Grenade grenade)
	{
		//既に含まれていたら追加しない
		if (!indicatorDic.ContainsKey(grenade))
		{
			GameObject gameObject = (GameObject)Instantiate(grenadeIndicatorPrefab, this.transform.position, Quaternion.identity);
			gameObject.transform.SetParent(this.transform);
			indicatorDic.Add(grenade, gameObject);
			gameObject.GetComponent<Indicator>().Init(grenade.gameObject);
		}
		else
		{
			Debug.LogError("グレネード表示マーカーManager InstanceSensorMarker Failure.");
		}
	}

	/// <summary>
	/// グレネード表示マーカーの削除
	/// </summary>
	public void DeleteIndicator(Grenade grenade)
	{
		if (indicatorDic.ContainsKey(grenade))
		{
			GameObject gameObject = indicatorDic[grenade];
			Destroy(gameObject);
			indicatorDic.Remove(grenade);
		}
	}
}
