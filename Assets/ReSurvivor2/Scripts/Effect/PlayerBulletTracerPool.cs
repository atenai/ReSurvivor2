using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

/// <summary>
/// プレイヤーの弾道エフェクト（PlayerBulletTracer）のオブジェクトプール
/// EffectManager（シーンをまたいで残る）の子に置く
/// </summary>
public class PlayerBulletTracerPool : MonoBehaviour
{
	ObjectPool<PlayerBulletTracer> objectPool;
	[SerializeField] PlayerBulletTracer prefab;
	int defalutCapacity = 60;
	int maxCount = 300;

	[Tooltip("飛んでいる最中の光の筋")]
	readonly List<PlayerBulletTracer> activeBullets = new List<PlayerBulletTracer>();

	/// <summary>
	/// オブジェクトプールの初期化処理
	/// </summary>
	void Start()
	{
		CreatePool();

		List<PlayerBulletTracer> initBulletList = new List<PlayerBulletTracer>();

		//オブジェクトプールのゲームオブジェクトを初期生成する
		for (int i = 0; i < defalutCapacity; i++)
		{
			initBulletList.Add(objectPool.Get());
		}

		foreach (var initBullet in initBulletList)
		{
			objectPool.Release(initBullet);
		}

		initBulletList.Clear();
	}

	void CreatePool()
	{
		if (objectPool != null)
		{
			return;
		}

		//オブジェクトプールの設定
		objectPool = new ObjectPool<PlayerBulletTracer>
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

	void OnEnable()
	{
		SceneManager.activeSceneChanged += OnActiveSceneChanged;
	}

	void OnDisable()
	{
		SceneManager.activeSceneChanged -= OnActiveSceneChanged;
	}

	/// <summary>
	/// シーンが切り替わったら飛んでいる光の筋を全部消す
	/// </summary>
	void OnActiveSceneChanged(Scene previous, Scene next)
	{
		ReleaseAll();
	}

	/// <summary>
	/// ObjectPoolコンストラクタ1つ目の引数の関数
	/// プールに空きが無い時に新たに生成する処理
	/// </summary>
	PlayerBulletTracer OnCreatePoolObject()
	{
		return Instantiate(prefab, this.transform);
	}

	/// <summary>
	/// ObjectPoolコンストラクタ2つ目の引数の関数
	/// プールに空きがあった際の処理
	/// </summary>
	void OnTakeFromPool(PlayerBulletTracer bullet)
	{
		bullet.gameObject.SetActive(true);
	}

	/// <summary>
	/// ObjectPoolコンストラクタ3つ目の引数の関数
	/// プールに返却するときの処理
	/// </summary>
	void OnReturnedToPool(PlayerBulletTracer bullet)
	{
		bullet.gameObject.SetActive(false);
	}

	/// <summary>
	/// ObjectPoolコンストラクタ4つ目の引数の関数
	/// プールのMaxサイズより多くなった際に自動で破棄する
	/// </summary>
	void OnDestroyPoolObject(PlayerBulletTracer bullet)
	{
		Destroy(bullet.gameObject);
	}

	/// <summary>
	/// 外部から呼ぶ光の筋の発射関数（銃口から着弾点まで）
	/// </summary>
	public void Fire(Vector3 origin, Vector3 end, float speed)
	{
		CreatePool();
		PlayerBulletTracer bullet = objectPool.Get();
		activeBullets.Add(bullet);
		bullet.Launch(origin, end, speed, ReleaseBullet);
	}

	/// <summary>
	/// 光の筋が消えた時にプールへ返却する
	/// </summary>
	void ReleaseBullet(PlayerBulletTracer bullet)
	{
		activeBullets.Remove(bullet);
		objectPool.Release(bullet);
	}

	/// <summary>
	/// 飛んでいる光の筋を全部消す
	/// </summary>
	public void ReleaseAll()
	{
		for (int i = activeBullets.Count - 1; i >= 0; i--)
		{
			if (activeBullets[i] != null)
			{
				activeBullets[i].Finish();//Finish の中で ReleaseBullet が呼ばれて activeBullets から外れる
			}
		}
		activeBullets.Clear();
	}
}
