using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemPlayMetalImpactEffectPool : MonoBehaviour
{
	[SerializeField] ParticleSystem particle;

	private bool isReturned = false;

	private void OnEnable()
	{
		isReturned = false;
		StopAllCoroutines();
	}

	public void PlayParticleSystem()
	{
		// particle が無ければ何もしない
		if (particle == null)
		{
			return;
		}

		isReturned = false;

		particle.Play();

		// 既存の予約コールをキャンセルしてコルーチンで完了を待つ
		StopAllCoroutines();
		StartCoroutine(WaitAndReturn());
	}

	private IEnumerator WaitAndReturn()
	{
		bool hasParticle = (particle != null);

		if (hasParticle == false)
		{
			ReturnToPool();
			yield break;
		}

		while (true)
		{
			bool particleAlive = hasParticle && particle.IsAlive(true);

			if (particleAlive == false)
			{
				break;
			}

			yield return null;
		}

		ReturnToPool();
	}

	private void ReturnToPool()
	{
		if (isReturned == true)
		{
			return;
		}

		isReturned = true;
		EffectManager.SingletonInstance.MetalImpactEffectPool.ReleaseGameObject(this.gameObject);
	}
}
