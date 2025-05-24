using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
	[UnityEngine.Tooltip("起爆時間")]
	float detonationTime = 3.0f;
	[UnityEngine.Tooltip("攻撃カウント")]
	float count = 0.0f;

	[Header("爆発")]
	[Tooltip("爆発エフェクトのプレファブ")]
	[SerializeField] GameObject mineExplosionEffectPrefab = null;
	[Tooltip("爆発エフェクトの終了時間")]
	[SerializeField] float mineExplosionEffectDestroyTime = 1.0f;

	[Tooltip("地雷のSEプレファブ")]
	[SerializeField] GameObject mineSePrefab = null;
	[Tooltip("地雷のSE終了時間")]
	[SerializeField] float mineSeEndtime = 1.0f;

	[Tooltip("爆発のあたり判定オブジェクトを生成")]
	[SerializeField] GameObject mineExplosionColliderPrefab = null;
	float mineExplosionColliderDestroyTime = 1.0f;

	void Start()
	{
		//グレネードマーカー作成
		GrenadeIndicatorManager.SingletonInstance.InstanceIndicator(this);
	}

	void Update()
	{
		count = count + Time.deltaTime;
		if (detonationTime < count)
		{
			count = 0.0f;

			Explosion();
		}
	}

	/// <summary>
	/// 爆発
	/// </summary>
	void Explosion()
	{
		if (mineSePrefab != null)
		{
			//SEオブジェクトを生成する
			var se = Instantiate(mineSePrefab, this.gameObject.transform.position, Quaternion.identity);
			Destroy(se, mineSeEndtime);
		}

		if (mineExplosionEffectPrefab != null)
		{
			//爆発エフェクトオブジェクトを生成する	
			var effect = Instantiate(mineExplosionEffectPrefab, this.gameObject.transform.position, Quaternion.identity);
			Destroy(effect, mineExplosionEffectDestroyTime);
		}

		if (mineExplosionColliderPrefab != null)
		{
			//爆発のあたり判定オブジェクトを生成する	
			var mineExplosionCollider = Instantiate(mineExplosionColliderPrefab, this.gameObject.transform.position, Quaternion.identity);
			Destroy(mineExplosionCollider, mineExplosionColliderDestroyTime);
		}

		Destroy(this.gameObject);
	}

	/// <summary>
	/// ゲームオブジェクトが非表示またはデストロイされた際に呼ばれる
	/// </summary>
	void OnDisable()
	{
		//敵マーカー削除
		GrenadeIndicatorManager.SingletonInstance.DeleteIndicator(this);
	}
}
