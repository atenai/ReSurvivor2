using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// リロードのアニメーション（リロード中の銃のリロードアニメーションをオンにして、再生位置をリロードの進み具合に合わせる）
/// ※GunAnimationFacadeが持ち、銃の状態（IReadOnlyGunModel）を見て動かす（銃のクラスからは使わない）
/// </summary>
[System.Serializable]
public class ReloadAnimation
{
	//※ショットガンは弾を1発ずつ込めるため、リロードアニメーションの「ポーチから弾を取る → 装填口に込める」の区間を、
	//  込める弾の数だけ行ったり来たりさせる（2発目以降は、装填口からポーチまで巻き戻して次の弾を取る）
	[Tooltip("リロードアニメーションの始まりの再生位置")]
	const float ReloadAnimationStartTime = 0.0f;
	[Tooltip("リロードアニメーションの終わりの再生位置")]
	const float ReloadAnimationEndTime = 1.0f;
	[Tooltip("リロードアニメーションの再生位置（0がアニメーションの始まり、1が終わり）")]
	float reloadAnimationTime = ReloadAnimationStartTime;
	public float ReloadAnimationTime => reloadAnimationTime;
	[Tooltip("リロードアニメーションを前へ進めているか？（ショットガンは弾を1発込めるたびにポーチまで巻き戻して次の弾を取りに行くので、巻き戻している間はfalseになる）")]
	bool isReloadAnimationForward = true;
	public bool IsReloadAnimationForward => isReloadAnimationForward;

	/// <summary>
	/// 銃の状態に合わせて、リロードのアニメーションを動かす（銃の更新処理の後に呼ぶ）
	/// ・リロード中の銃のリロードアニメーションをオンにする
	/// ・リロードアニメーションの再生位置を、リロードの進み具合に合わせる（こうすると、リロード時間が変わっても、アニメーションがちょうどリロードの終わりで終わる）
	/// ※リロードしていない時は再生位置を変えない（リロードが終わってアニメーションを抜ける間に、手が急に動かないようにする）
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	/// <param name="gunState">銃の状態を読むための窓口</param>
	/// <param name="shotGunAnimation">ショットガンを持っている時のアニメーションの設定（弾を取るタイミング・込めるタイミングを使う）</param>
	public void UpdateAnimation(Animator animator, IReadOnlyGunModel gunState, GunAnimation shotGunAnimation)
	{
		animator.SetBool(PlayerAnimatorParameter.IsHandGunReload, gunState.IsGunReloadTimeActive(EnumManager.GunTYPE.HandGun));
		animator.SetBool(PlayerAnimatorParameter.IsAssaultRifleReload, gunState.IsGunReloadTimeActive(EnumManager.GunTYPE.AssaultRifle));
		animator.SetBool(PlayerAnimatorParameter.IsShotGunReload, gunState.IsGunReloadTimeActive(EnumManager.GunTYPE.ShotGun));

		if (gunState.IsReloadTimeActive == false)
		{
			return;
		}

		if (gunState.GetGunTYPE == EnumManager.GunTYPE.ShotGun)
		{
			UpdateShotGunReloadAnimationTime(gunState, shotGunAnimation);
		}
		else
		{
			reloadAnimationTime = gunState.ReloadProgress;
			isReloadAnimationForward = true;
		}
		animator.SetFloat(PlayerAnimatorParameter.ReloadAnimationTime, reloadAnimationTime);
	}

	/// <summary>
	/// ショットガンのリロードアニメーションの再生位置を、今込めている弾に合わせて決める
	/// （最初 → 込める弾の数だけ「ポーチ → 装填口」を繰り返す → 最後、の順に進める。装填口から次の弾を取りに戻る間は巻き戻す）
	/// </summary>
	/// <param name="gunState">銃の状態を読むための窓口</param>
	/// <param name="shotGunAnimation">ショットガンを持っている時のアニメーションの設定</param>
	void UpdateShotGunReloadAnimationTime(IReadOnlyGunModel gunState, GunAnimation shotGunAnimation)
	{
		float phaseProgress;
		EnumManager.ShotGunReloadPhaseTYPE phase = gunState.GetShotGunReloadPhase(out phaseProgress);
		isReloadAnimationForward = phase != EnumManager.ShotGunReloadPhaseTYPE.ShellReturn;
		switch (phase)
		{
			case EnumManager.ShotGunReloadPhaseTYPE.Start:
				//銃を弾を込める持ち方にして、右手がポーチまで弾を取りに行く
				reloadAnimationTime = Mathf.SmoothStep(ReloadAnimationStartTime, shotGunAnimation.MagazineOutTime, phaseProgress);
				break;
			case EnumManager.ShotGunReloadPhaseTYPE.FirstShellInsert:
				//最初にポーチに着いた所から、装填口まで進めて込める
				reloadAnimationTime = Mathf.SmoothStep(shotGunAnimation.MagazineOutTime, shotGunAnimation.MagazineInTime, phaseProgress);
				break;
			case EnumManager.ShotGunReloadPhaseTYPE.ShellReturn:
				//次の弾を取るポーチまで巻き戻す
				reloadAnimationTime = Mathf.SmoothStep(shotGunAnimation.MagazineInTime, shotGunAnimation.MagazineOutTime, phaseProgress);
				break;
			case EnumManager.ShotGunReloadPhaseTYPE.ShellInsert:
				//ポーチで次の弾を取った所から、装填口まで進めて込める
				reloadAnimationTime = Mathf.SmoothStep(shotGunAnimation.MagazineOutTime, shotGunAnimation.MagazineInTime, phaseProgress);
				break;
			case EnumManager.ShotGunReloadPhaseTYPE.End:
				//最後の弾を込めた所から、元の構えに戻す
				reloadAnimationTime = Mathf.SmoothStep(shotGunAnimation.MagazineInTime, ReloadAnimationEndTime, phaseProgress);
				break;
		}
	}
}
