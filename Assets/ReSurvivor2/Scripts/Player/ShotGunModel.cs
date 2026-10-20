using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Knife.Effects;

/// <summary>
/// ショットガンモデル
/// </summary>
public class ShotGunModel : GunModelBase
{
	/// <summary>
	/// この銃の種類
	/// </summary>
	protected override EnumManager.GunTYPE GunType => EnumManager.GunTYPE.ShotGun;

	[Header("ショットガンの弾込め")]
	//※弾を1発ずつ込めるため、リロードアニメーションの「ポーチから弾を取る → 装填口に込める」の区間を、
	//  込める弾の数だけ行ったり来たりさせる（2発目以降は、装填口から少しだけ巻き戻した所で次の弾を取る）
	[Tooltip("2発目以降の弾を取るタイミング（リロードアニメーションの再生位置）。装填口に込めるタイミング（magazineInTime）から、ここまで巻き戻して次の弾を取る")]
	[SerializeField] float shellPickTime;
	public float ShellPickTime => shellPickTime;

	[Header("ショットガンモデル")]
	[Tooltip("キャラクターの手に持っているショットガンのモデル")]
	[SerializeField] GameObject shotGunModel;
	public GameObject GetShotGunModel => shotGunModel;
	[Tooltip("キャラクターの体についているショットガンのモデル")]
	[SerializeField] GameObject shotGunModelBodyDecoration;
	public GameObject GetShotGunModelBodyDecoration => shotGunModelBodyDecoration;
	[Tooltip("ショットガンのマズルフラッシュの生成座標位置")]
	[SerializeField] Transform shotGunMuzzleTransform;
	public Transform ShotGunMuzzleTransform => shotGunMuzzleTransform;
	[Tooltip("ショットガンの薬莢の生成座標位置")]
	[SerializeField] Transform shotGunBulletCasingTransform;
	public Transform ShotGunBulletCasingTransform => shotGunBulletCasingTransform;
	[Tooltip("ショットガンの硝煙の生成座標位置")]
	[SerializeField] Transform shotGunAfterFireSmokeTransform;
	public Transform ShotGunAfterFireSmokeTransform => shotGunAfterFireSmokeTransform;
	//↓アセットストアのプログラム↓//
	[Tooltip("ショットガンのマズルフラッシュと薬莢")]
	[SerializeField] ParticleGroupEmitter[] shotGunShotEmitters;
	[Tooltip("ショットガンの硝煙")]
	[SerializeField] ParticleGroupPlayer shotGunAfterFireSmoke;
	//↑アセットストアのプログラム↑//

	/// <summary>
	/// マズルフラッシュのエフェクトと薬莢を出す（アセットストアで買ったコードをそのままもってきている）
	/// </summary>
	public void ShotGunMuzzleFlashAndShell()
	{
		if (shotGunShotEmitters != null)
		{
			foreach (var effect in shotGunShotEmitters)
			{
				effect.Emit(1);
			}
		}
	}

	/// <summary>
	/// 硝煙のエフェクトを出す（アセットストアで買ったコードをそのままもってきている）
	/// </summary>
	public void ShotGunSmoke()
	{
		if (shotGunAfterFireSmoke != null)
		{
			shotGunAfterFireSmoke.Play();
		}
	}
}
