using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// エフェクトマネージャー
/// </summary>
public class EffectManager : MonoBehaviour
{
	/// <summary> シングルトンで作成（ゲーム中に１つのみにする）</summary>
	static EffectManager singletonInstance = null;
	/// <summary>シングルトンのプロパティ</summary>
	public static EffectManager SingletonInstance => singletonInstance;


	[Header("着弾エフェクト")]
	//パス(Assets/Knife/PRO Effects FPS Muzzle flashes & Impacts/Particles/Prefabs/Impacts)

	[Tooltip("血の着弾エフェクト")]
	[SerializeField] BloodImpactEffectPool bloodImpactEffectPool;
	public BloodImpactEffectPool BloodImpactEffectPool => bloodImpactEffectPool;

	[Tooltip("鉄の着弾エフェクト")]
	[SerializeField] MetalImpactEffectPool metalImpactEffectPool;
	public MetalImpactEffectPool MetalImpactEffectPool => metalImpactEffectPool;
	[Tooltip("地面の着弾エフェクト")]
	[SerializeField] GroundImpactEffectPool groundImpactEffectPool;
	public GroundImpactEffectPool GroundImpactEffectPool => groundImpactEffectPool;
	[Tooltip("岩の着弾エフェクト")]
	[SerializeField] RockImpactEffectPool rockImpactEffectPool;
	public RockImpactEffectPool RockImpactEffectPool => rockImpactEffectPool;


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
	/// 着弾エフェクト
	/// </summary> 
	public void ImpactEffect(RaycastHit hit)
	{
		if (hit.collider.gameObject.CompareTag("Enemy") || hit.collider.gameObject.CompareTag("GroundEnemy"))//※間違ってオブジェクトの設定にレイヤーとタグを間違えるなよおれｗ
		{
			//bloodImpactEffect
			//GameObject impactGameObject = Instantiate(bloodImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
			//Destroy(impactGameObject, 2.0f);
			bloodImpactEffectPool.GetGameObject(hit.point, Quaternion.LookRotation(hit.normal));
		}
		else if (hit.collider.gameObject.CompareTag("FlyingEnemy"))
		{
			//metal2ImpactEffect
			//GameObject impactGameObject = Instantiate(metal2ImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
			//Destroy(impactGameObject, 2.0f);
			metalImpactEffectPool.GetGameObject(hit.point, Quaternion.LookRotation(hit.normal));
		}
		else if (hit.collider.gameObject.CompareTag("Ground"))
		{
			//groundImpactEffect
			//GameObject impactGameObject = Instantiate(groundImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
			//Destroy(impactGameObject, 2.0f);
			groundImpactEffectPool.GetGameObject(hit.point, Quaternion.LookRotation(hit.normal));
		}
		else
		{
			//rockImpactEffect
			//GameObject impactGameObject = Instantiate(rockImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
			//Destroy(impactGameObject, 2.0f);
			rockImpactEffectPool.GetGameObject(hit.point, Quaternion.LookRotation(hit.normal));
		}
	}
}
