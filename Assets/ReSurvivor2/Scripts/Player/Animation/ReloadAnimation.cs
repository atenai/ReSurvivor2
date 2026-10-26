using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// リロードのアニメーション（リロード中の銃のリロードアニメーションをオンにして、再生位置をリロードの進み具合に合わせる）
/// ※PlayerCharacterViewが持ち、銃の状態を見て動かす（銃のクラスからは使わない）
/// </summary>
[System.Serializable]
public class ReloadAnimation
{
	//※ショットガンは弾を1発ずつ込めるため、リロードアニメーションの「ポーチから弾を取る → 装填口に込める」の区間を、
	//  込める弾の数だけ行ったり来たりさせる（2発目以降は、装填口から少しだけ巻き戻した所で次の弾を取る）
	[Tooltip("ショットガンの2発目以降の弾を取るタイミング（リロードアニメーションの再生位置）。装填口に込めるタイミングから、ここまで巻き戻して次の弾を取る")]
	[SerializeField] float shotGunShellPickTime;
	[Tooltip("リロードアニメーションの始まりの再生位置")]
	const float ReloadAnimationStartTime = 0.0f;
	[Tooltip("リロードアニメーションの終わりの再生位置")]
	const float ReloadAnimationEndTime = 1.0f;
	[Tooltip("リロードアニメーションの再生位置（0がアニメーションの始まり、1が終わり）")]
	float reloadAnimationTime = ReloadAnimationStartTime;
	public float ReloadAnimationTime => reloadAnimationTime;
	[Tooltip("リロードアニメーションを前へ進めているか？（ショットガンは弾を1発込めるたびに少し巻き戻して次の弾を取りに行くので、巻き戻している間はfalseになる）")]
	bool isReloadAnimationForward = true;
	public bool IsReloadAnimationForward => isReloadAnimationForward;

	/// <summary>
	/// 銃の状態に合わせて、リロードのアニメーションを動かす（銃の更新処理の後に呼ぶ）
	/// ・リロード中の銃のリロードアニメーションをオンにする
	/// ・リロードアニメーションの再生位置を、リロードの進み具合に合わせる（こうすると、リロード時間が変わっても、アニメーションがちょうどリロードの終わりで終わる）
	/// ※リロードしていない時は再生位置を変えない（リロードが終わってアニメーションを抜ける間に、手が急に動かないようにする）
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	/// <param name="gunModelFacade">銃</param>
	/// <param name="shotGunAnimation">ショットガンを持っている時のアニメーションの設定（弾を取るタイミング・込めるタイミングを使う）</param>
	public void UpdateAnimation(Animator animator, GunModelFacade gunModelFacade, GunAnimation shotGunAnimation)
	{
		animator.SetBool(PlayerAnimatorParameter.IsHandGunReload, gunModelFacade.FindGunBase(EnumManager.GunTYPE.HandGun).IsReloadTimeActive);
		animator.SetBool(PlayerAnimatorParameter.IsAssaultRifleReload, gunModelFacade.FindGunBase(EnumManager.GunTYPE.AssaultRifle).IsReloadTimeActive);
		animator.SetBool(PlayerAnimatorParameter.IsShotGunReload, gunModelFacade.FindGunBase(EnumManager.GunTYPE.ShotGun).IsReloadTimeActive);

		GunModelBase gunModelBase = gunModelFacade.GunModelBase;
		if (gunModelBase.IsReloadTimeActive == false)
		{
			return;
		}

		if (gunModelFacade.GetGunTYPE == EnumManager.GunTYPE.ShotGun)
		{
			UpdateShotGunReloadAnimationTime((ShotGunModel)gunModelBase, shotGunAnimation);
		}
		else
		{
			reloadAnimationTime = gunModelBase.ReloadProgress;
			isReloadAnimationForward = true;
		}
		animator.SetFloat(PlayerAnimatorParameter.ReloadAnimationTime, reloadAnimationTime);
	}

	/// <summary>
	/// ショットガンのリロードアニメーションの再生位置を、今込めている弾に合わせて決める
	/// （最初 → 込める弾の数だけ「ポーチ → 装填口」を繰り返す → 最後、の順に進める。装填口から次の弾を取りに戻る間は巻き戻す）
	/// </summary>
	/// <param name="shotGunModel">ショットガン</param>
	/// <param name="shotGunAnimation">ショットガンを持っている時のアニメーションの設定</param>
	void UpdateShotGunReloadAnimationTime(ShotGunModel shotGunModel, GunAnimation shotGunAnimation)
	{
		float phaseProgress;
		EnumManager.ShotGunReloadPhaseTYPE phase = shotGunModel.GetReloadPhase(out phaseProgress);
		isReloadAnimationForward = phase != EnumManager.ShotGunReloadPhaseTYPE.ShellReturn;
		switch (phase)
		{
			case EnumManager.ShotGunReloadPhaseTYPE.Start:
				//銃を弾を込める姿勢にして、左手がポーチまで弾を取りに行く
				reloadAnimationTime = Mathf.SmoothStep(ReloadAnimationStartTime, shotGunAnimation.MagazineOutTime, phaseProgress);
				break;
			case EnumManager.ShotGunReloadPhaseTYPE.FirstShellInsert:
				//最初にポーチに着いた所から、装填口まで進めて込める
				reloadAnimationTime = Mathf.SmoothStep(shotGunAnimation.MagazineOutTime, shotGunAnimation.MagazineInTime, phaseProgress);
				break;
			case EnumManager.ShotGunReloadPhaseTYPE.ShellReturn:
				//次の弾を取る所まで巻き戻す
				reloadAnimationTime = Mathf.SmoothStep(shotGunAnimation.MagazineInTime, shotGunShellPickTime, phaseProgress);
				break;
			case EnumManager.ShotGunReloadPhaseTYPE.ShellInsert:
				//次の弾を取った所から、装填口まで進めて込める
				reloadAnimationTime = Mathf.SmoothStep(shotGunShellPickTime, shotGunAnimation.MagazineInTime, phaseProgress);
				break;
			case EnumManager.ShotGunReloadPhaseTYPE.End:
				//最後の弾を込めた所から、元の構えに戻す
				reloadAnimationTime = Mathf.SmoothStep(shotGunAnimation.MagazineInTime, ReloadAnimationEndTime, phaseProgress);
				break;
		}
	}
}
