using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Knife.Effects
{
	/// <summary>
	/// Particle group emitter component.
	/// </summary>
	public class ParticleGroupEmitter : MonoBehaviour
	{
		/// <summary>
		/// Particle systems array.
		/// </summary>
		[SerializeField][Tooltip("Particle systems array")] private ParticleSystem[] particleSystems;
		/// <summary>
		/// Particle count multiplier.
		/// </summary>
		[SerializeField][Tooltip("Particle count multiplier")] private int countMultiplier = 1;

		/// <summary>
		/// particleSystems 配列のパーティクルをカウント倍率付きで放出します。
		/// </summary>
		/// <param name="count">count multiplier</param>
		public void Emit(int count)
		{
			foreach (var ps in particleSystems)
			{
				if (ps.main.startDelay.constant == 0)
				{
					//Debug.Log("<color=red>Emit0</color>");
					Emit(ps, count * countMultiplier);
				}
				// else
				// {
				// 	StartCoroutine(PlayDelayed(ps, count));
				// }
			}
		}

		// private IEnumerator PlayDelayed(ParticleSystem particleSystem, int count)
		// {
		// 	Debug.Log("<color=green>PlayDelayed</color>");
		// 	yield return new WaitForSeconds(particleSystem.main.startDelay.constant);
		// 	Emit(particleSystem, count);
		// }

		private void Emit(ParticleSystem particleSystem, int count)
		{
			//Debug.Log("<color=blue>Emit1</color>");
			if (particleSystem.emission.burstCount == 0)
			{
				//Debug.Log("<color=purple>Emit2</color>");
				particleSystem.Emit(count * countMultiplier);
			}
			else
			{
				//Debug.Log("<color=orange>Emit3</color>");
				var burst = particleSystem.emission.GetBurst(0);
				switch (burst.count.mode)
				{
					case ParticleSystemCurveMode.Constant:
						//Debug.Log("<color=cyan>Emit4</color>");
						particleSystem.Emit((int)burst.count.constant * countMultiplier * count);
						break;
					case ParticleSystemCurveMode.TwoConstants:
						//Debug.Log("<color=yellow>Emit5</color>");
						particleSystem.Emit((int)Random.Range(burst.count.constantMin, burst.count.constantMax) * countMultiplier * count);
						break;
				}
			}
		}
	}
}