using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// エフェクトマネージャー
/// </summary>
public class EffectManager : MonoBehaviour
{
	/// <summary>
	/// シングルトンで作成（ゲーム中に１つのみにする）
	/// </summary>
	static EffectManager singletonInstance = null;
	public static EffectManager SingletonInstance => singletonInstance;

	[Header("着弾エフェクト")]
	//パス(Assets/Knife/PRO Effects FPS Muzzle flashes & Impacts/Particles/Prefabs/Impacts)
	[Tooltip("血の着弾エフェクト")]
	[SerializeField] GameObject bloodImpactEffect;
	[Tooltip("煙の着弾エフェクト")]
	[SerializeField] GameObject rockImpactEffect;

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
		if (hit.collider.gameObject.CompareTag("Enemy") || hit.collider.gameObject.CompareTag("FlyingEnemy") || hit.collider.gameObject.CompareTag("GroundEnemy"))//※間違ってオブジェクトの設定にレイヤーとタグを間違えるなよおれｗ
		{
			GameObject impactGameObject = Instantiate(bloodImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
			Destroy(impactGameObject, 2.0f);
		}
		else
		{
			GameObject impactGameObject = Instantiate(rockImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
			Destroy(impactGameObject, 2.0f);
		}
	}
}
