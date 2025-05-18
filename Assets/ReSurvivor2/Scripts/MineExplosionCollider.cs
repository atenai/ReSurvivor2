using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MineExplosionCollider : MonoBehaviour
{
	[Tooltip("ダメージ")]
	[SerializeField] int damage = 50;
	[Tooltip("爆発範囲")]
	float explosionRangeScale = 3.0f;
	float localScale = 1.0f;
	[Tooltip("爆発範囲拡大スピード")]
	[SerializeField] float rangeSpeed = 2.0f;
	[SerializeField] private CinemachineImpulseSource cinemachineImpulseSource;
	public CinemachineImpulseSource CinemachineImpulseSource => cinemachineImpulseSource;

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
	private void ExplosionRangeExpansion()
	{
		localScale = localScale + (Time.deltaTime * rangeSpeed);
		this.transform.localScale = new Vector3(localScale, localScale, localScale);
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (collider.gameObject.CompareTag("Enemy") || collider.gameObject.CompareTag("FlyingEnemy") || collider.gameObject.CompareTag("GroundEnemy"))
		{
			//ダメージ
			Target target = collider.transform.GetComponent<Target>();
			if (target != null)
			{
				target.TakeDamage(damage);
			}
		}

		if (collider.CompareTag("Player"))
		{
			Player.SingletonInstance.TakeDamage(damage);
			Shaker();
		}
	}

	private void Shaker()
	{
		cinemachineImpulseSource.GenerateImpulse();
	}
}
