using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 弾がかすめた時の風切り音のオブジェクトプール
/// 音は弾がかすめた位置から鳴る（左右どちらをかすめたか分かる）
/// </summary>
public class BulletFlybySEPool : MonoBehaviour
{
	ObjectPool<GameObject> objectPool;
	[SerializeField] GameObject prefab;
	int defalutCapacity = 10;
	int maxCount = 40;

	[Tooltip("風切り音を続けて鳴らす時の最短の間隔（ショットガンの散弾などで同時に何発もかすめた時に重なりすぎないようにする）")]
	[SerializeField] float minInterval = 0.08f;
	[Tooltip("一度でも鳴らしたか")]
	bool hasPlayed = false;
	[Tooltip("最後に鳴らした時間")]
	float lastPlayTime = 0.0f;
	public float LastPlayTime => lastPlayTime;
	[Tooltip("最後に鳴らした位置")]
	Vector3 lastPlayPosition;
	public Vector3 LastPlayPosition => lastPlayPosition;

	/// <summary>
	/// オブジェクトプールの初期化処理
	/// </summary>
	void Start()
	{
		CreatePool();

		List<GameObject> initGameObjectList = new List<GameObject>();

		//オブジェクトプールのゲームオブジェクトを初期生成する
		//必ずコンポーネントのインスペクターにあるPlay On Awakeのチェックを外すしてOFFにしておくこと！
		for (int i = 0; i < defalutCapacity; i++)
		{
			GameObject initGameObject = objectPool.Get();
			initGameObject.transform.position = transform.position;
			initGameObjectList.Add(initGameObject);
		}

		foreach (GameObject initGameObject in initGameObjectList)
		{
			ReleaseGameObject(initGameObject);
		}

		initGameObjectList.Clear();
	}

	void CreatePool()
	{
		if (objectPool != null)
		{
			return;
		}

		//オブジェクトプールの設定
		objectPool = new ObjectPool<GameObject>
		(
			OnCreatePoolObject,
			OnTakeFromPool,
			OnReturnedToPool,
			OnDestroyPoolObject,
			true,//必ずtrueにする（二重Releaseが即例外で分かるので、原因特定が一気に楽になります。）
			defalutCapacity,
			maxCount
		);
	}

	/// <summary>
	/// ObjectPoolコンストラクタ1つ目の引数の関数
	/// プールに空きが無い時に新たに生成する処理
	/// </summary>
	GameObject OnCreatePoolObject()
	{
		return Instantiate(prefab, this.transform);
	}

	/// <summary>
	/// ObjectPoolコンストラクタ2つ目の引数の関数
	/// プールに空きがあった際の処理
	/// </summary>
	void OnTakeFromPool(GameObject gameObject)
	{
		gameObject.SetActive(true);
	}

	/// <summary>
	/// ObjectPoolコンストラクタ3つ目の引数の関数
	/// プールに返却するときの処理
	/// </summary>
	void OnReturnedToPool(GameObject gameObject)
	{
		gameObject.SetActive(false);
	}

	/// <summary>
	/// ObjectPoolコンストラクタ4つ目の引数の関数
	/// プールのMaxサイズより多くなった際に自動で破棄する
	/// </summary>
	void OnDestroyPoolObject(GameObject gameObject)
	{
		Destroy(gameObject);
	}

	/// <summary>
	/// 外部から呼ぶ再生関数（弾がかすめた位置で鳴らす）
	/// </summary>
	public void Play(Vector3 position)
	{
		//間隔が短すぎる時は鳴らさない
		if (hasPlayed == true && Time.time - lastPlayTime < minInterval)
		{
			return;
		}
		hasPlayed = true;
		lastPlayTime = Time.time;
		lastPlayPosition = position;

		CreatePool();
		GameObject gameObject = objectPool.Get();
		gameObject.transform.position = position;
		gameObject.GetComponent<AudioPlayBulletFlybySEPool>().PlaySound();
	}

	/// <summary>
	/// 外部から呼ぶObj返却用関数
	/// </summary>
	public void ReleaseGameObject(GameObject gameObject)
	{
		objectPool.Release(gameObject);
	}
}
