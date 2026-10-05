using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

/// <summary>
/// エネミーの弾（EnemyBullet）のオブジェクトプール
/// EffectManager（シーンをまたいで残る）の子に置く
/// </summary>
public class EnemyBulletPool : MonoBehaviour
{
	ObjectPool<EnemyBullet> objectPool;
	[SerializeField] EnemyBullet prefab;
	int defalutCapacity = 60;
	int maxCount = 300;

	[Tooltip("飛んでいる最中の弾")]
	readonly List<EnemyBullet> activeBullets = new List<EnemyBullet>();

	/// <summary>
	/// オブジェクトプールの初期化処理
	/// </summary>
	void Start()
	{
		CreatePool();

		List<EnemyBullet> initBulletList = new List<EnemyBullet>();

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
		objectPool = new ObjectPool<EnemyBullet>
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
	/// シーンが切り替わったら飛んでいる弾を全部消す（前のステージの弾が次のステージで当たらないようにする）
	/// </summary>
	void OnActiveSceneChanged(Scene previous, Scene next)
	{
		ReleaseAll();
	}

	/// <summary>
	/// ObjectPoolコンストラクタ1つ目の引数の関数
	/// プールに空きが無い時に新たに生成する処理
	/// </summary>
	EnemyBullet OnCreatePoolObject()
	{
		return Instantiate(prefab, this.transform);
	}

	/// <summary>
	/// ObjectPoolコンストラクタ2つ目の引数の関数
	/// プールに空きがあった際の処理
	/// </summary>
	void OnTakeFromPool(EnemyBullet bullet)
	{
		bullet.gameObject.SetActive(true);
	}

	/// <summary>
	/// ObjectPoolコンストラクタ3つ目の引数の関数
	/// プールに返却するときの処理
	/// </summary>
	void OnReturnedToPool(EnemyBullet bullet)
	{
		bullet.gameObject.SetActive(false);
	}

	/// <summary>
	/// ObjectPoolコンストラクタ4つ目の引数の関数
	/// プールのMaxサイズより多くなった際に自動で破棄する
	/// </summary>
	void OnDestroyPoolObject(EnemyBullet bullet)
	{
		Destroy(bullet.gameObject);
	}

	/// <summary>
	/// 外部から呼ぶ弾の発射関数
	/// </summary>
	public void Fire(GroundEnemy shooter, Vector3 origin, Vector3 direction, float speed, float range, float damage)
	{
		CreatePool();
		EnemyBullet bullet = objectPool.Get();
		activeBullets.Add(bullet);
		bullet.Launch(shooter, origin, direction, speed, range, damage, ReleaseBullet);
	}

	/// <summary>
	/// 弾が消えた時にプールへ返却する
	/// </summary>
	void ReleaseBullet(EnemyBullet bullet)
	{
		activeBullets.Remove(bullet);
		objectPool.Release(bullet);
	}

	/// <summary>
	/// 飛んでいる弾を全部消す
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
