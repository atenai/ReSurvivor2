using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.VisualScripting;

/// <summary>
/// 地雷の爆発コライダー
/// </summary>
public class MineExplosionCollider : MonoBehaviour
{
	[Tooltip("ダメージ")]
	[SerializeField] int damage = 50;
	[Tooltip("爆発範囲")]
	float explosionRangeScale = 3.0f;
	float localScale = 1.0f;
	[Tooltip("爆発範囲拡大スピード")]
	[SerializeField] float rangeSpeed = 2.0f;
	[Tooltip("シネマシーンインパルス")]
	[SerializeField] private CinemachineImpulseSource cinemachineImpulseSource;

	void Start()
	{
		localScale = explosionRangeScale;
		this.transform.localScale = new Vector3(localScale, localScale, localScale);
	}

	void FixedUpdate()
	{
		//ExplosionRangeExpansion();
	}

	/// <summary>
	/// 爆発範囲拡大
	/// </summary>
	void ExplosionRangeExpansion()
	{
		localScale = localScale + (Time.deltaTime * rangeSpeed);
		this.transform.localScale = new Vector3(localScale, localScale, localScale);
	}

	void OnTriggerEnter(Collider collider)
	{
		if (collider.gameObject.CompareTag("Enemy") || collider.gameObject.CompareTag("FlyingEnemy") || collider.gameObject.CompareTag("GroundEnemy"))
		{
			//ダメージ
			IEnemy enemy = collider.transform.GetComponent<IEnemy>();
			if (enemy != null)
			{
				enemy.GetHitPoint().Damage(damage);
			}
		}

		if (collider.CompareTag("Player"))
		{
			PlayerManager.SingletonInstance.PlayerModel.HP.Damage(damage);
			CameraShaker();
		}
	}

	/// <summary>
	/// プレイヤーにダメージを与えた際にカメラを揺らす
	/// </summary>
	void CameraShaker()
	{
		cinemachineImpulseSource.GenerateImpulse();
	}
}
