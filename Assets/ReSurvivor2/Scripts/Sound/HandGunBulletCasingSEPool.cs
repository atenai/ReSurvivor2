using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class HandGunBulletCasingSEPool : MonoBehaviour
{
	ObjectPool<GameObject> objectPool;
	[SerializeField] GameObject prefab;
	int defalutCapacity = 14;
	int maxCount = 70;

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
			false,
			defalutCapacity,
			maxCount
		);

		//オブジェクトプールのゲームオブジェクトを初期生成する
		//必ずコンポーネントのインスペクターにあるPlay On Awakeのチェックを外すしてOFFにしておくこと！
		for (int i = 0; i < defalutCapacity; i++)
		{
			GameObject gameObject = objectPool.Get();
			gameObject.transform.position = transform.position;
		}
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
	/// 外部から呼ぶObj取得関数
	/// </summary>
	public void GetGameObject(Transform transform)
	{
		GameObject gameObject = objectPool.Get();
		gameObject.transform.position = transform.position;
		gameObject.GetComponent<AudioPlayHandGunBulletCasingSEPool>().PlaySound();
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
