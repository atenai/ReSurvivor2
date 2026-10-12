using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// ショットガンのリロードのSE
/// </summary>
public class ShotGunReloadSEPool : MonoBehaviour
{
	ObjectPool<GameObject> objectPool;
	[SerializeField] GameObject prefab;
	[Tooltip("プレイヤーが鳴らす時の3Dの設定（カメラはプレイヤーの3〜4m後ろにあるので、その距離では音量が下がらないようにしている）")]
	[SerializeField] SpatialSoundSetting playerSpatialSetting = new SpatialSoundSetting(5.0f, 25.0f);
	[Tooltip("エネミーが鳴らす時の3Dの設定（遠くのエネミーの音は小さく、鳴った方向から聞こえる）")]
	[SerializeField] SpatialSoundSetting enemySpatialSetting = new SpatialSoundSetting(1.5f, 25.0f);
	int defalutCapacity = 8;
	int maxCount = 40;

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
	/// 外部から呼ぶObj取得関数（プレイヤーが鳴らす時）
	/// </summary>
	public void GetGameObject(Transform transform)
	{
		Play(transform, false);
	}

	/// <summary>
	/// エネミーから呼ぶObj取得関数
	/// </summary>
	public void GetGameObjectForEnemy(Transform transform)
	{
		Play(transform, true);
	}

	/// <summary>
	/// プールから取り出して、プレイヤー用かエネミー用の3Dの設定にしてから鳴らす
	/// （プールはプレイヤーとエネミーで同じものを使うので、鳴らすたびに設定し直す）
	/// </summary>
	void Play(Transform transform, bool isEnemy)
	{
		GameObject gameObject = objectPool.Get();
		gameObject.transform.position = transform.position;
		AudioSource audioSource = gameObject.GetComponent<AudioSource>();
		SpatialSoundSetting spatialSetting = isEnemy == true ? enemySpatialSetting : playerSpatialSetting;
		spatialSetting.Apply(audioSource);
		gameObject.GetComponent<AudioPlayShotGunReloadSEPool>().PlaySound();
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
