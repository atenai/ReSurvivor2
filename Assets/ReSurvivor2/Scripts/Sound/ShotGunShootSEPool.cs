using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// ショットガンの射撃のSE
/// </summary>
public class ShotGunShootSEPool : MonoBehaviour
{
	ObjectPool<GameObject> objectPool;
	[SerializeField] GameObject prefab;
	int defalutCapacity = 8 * 3;
	int maxCount = 40;

	[Tooltip("エネミーの射撃音が、この距離（m）までは音量が下がらない")]
	const float EnemyMinDistance = 5.0f;
	[Tooltip("エネミーの射撃音が、この距離（m）より遠くでは、音量がそれ以上下がらない")]
	const float EnemyMaxDistance = 100.0f;
	[Tooltip("エネミーが鳴らす時の3Dの設定（鳴った位置から聞こえて、遠いほど小さくなる）")]
	[SerializeField] SpatialSoundSetting enemySpatialSetting = new SpatialSoundSetting(EnemyMinDistance, EnemyMaxDistance);

	/// <summary>
	/// オブジェクトプールの初期化処理
	/// </summary>
	void Start()
	{
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

		List<GameObject> initGameObjectList = new List<GameObject>();

		//オブジェクトプールのゲームオブジェクトを初期生成する
		//必ずコンポーネントのインスペクターにあるPlay On Awakeのチェックを外すしてOFFにしておくこと！
		for (int i = 0; i < defalutCapacity; i++)
		{
			GameObject initGameObject = objectPool.Get();
			initGameObject.transform.position = transform.position;
			initGameObjectList.Add(initGameObject);
		}

		foreach (var initGameObject in initGameObjectList)
		{
			ReleaseGameObject(initGameObject);
		}

		initGameObjectList.Clear();
	}

	/// <summary>
	/// ObjectPoolコンストラクタ1つ目の引数の関数
	/// プールに空きが無い時に新たに生成する処理
	/// objectPool.Get()が呼ばれる
	/// </summary>
	GameObject OnCreatePoolObject()
	{
		var gameObject = Instantiate(prefab, this.transform);
		return gameObject;
	}

	/// <summary>
	/// ObjectPoolコンストラクタ2つ目の引数の関数
	/// プールに空きがあった際の処理
	/// objectPool.Get()が呼ばれる
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
	/// 外部から呼ぶObj取得関数（プレイヤーが鳴らす時：2Dで鳴らす）
	/// </summary>
	public void GetGameObject(Transform transform)
	{
		GameObject gameObject = objectPool.Get();
		gameObject.transform.position = transform.position;
		SpatialSoundSetting.Apply2D(gameObject.GetComponent<AudioSource>());
		gameObject.GetComponent<AudioPlayShotGunShootSEPool>().PlaySound();
	}

	/// <summary>
	/// エネミーから呼ぶObj取得関数（3Dで鳴らす：鳴った位置から聞こえて、遠いほど小さくなる）
	/// </summary>
	public void GetGameObjectForEnemy(Transform transform)
	{
		GameObject gameObject = objectPool.Get();
		gameObject.transform.position = transform.position;
		enemySpatialSetting.Apply3D(gameObject.GetComponent<AudioSource>());
		gameObject.GetComponent<AudioPlayShotGunShootSEPool>().PlaySound();
	}

	/// <summary>
	/// 外部から呼ぶObj返却用関数
	/// </summary>
	public void ReleaseGameObject(GameObject gameObject)
	{
		objectPool.Release(gameObject);
	}

	void Update()
	{

	}
}
