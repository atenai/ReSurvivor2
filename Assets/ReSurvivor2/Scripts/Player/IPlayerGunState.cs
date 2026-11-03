/// <summary>
/// プレイヤーの銃の状態を読むための窓口（読むだけで、銃の状態は変えない）
/// ※銃（GunModelFacade）がこの窓口を用意し、アニメーション（PlayerCharacterViewとアニメーションのクラス）はこの窓口だけを見て動く。
///   銃の側はアニメーションの側を知らず、アニメーションの側は銃のクラスそのものを知らない
/// </summary>
public interface IPlayerGunState
{
	/// <summary>
	/// 今持っている銃
	/// </summary>
	EnumManager.GunTYPE GetGunTYPE { get; }

	/// <summary>
	/// 今持っている銃がリロード中か？
	/// </summary>
	bool IsReloadTimeActive { get; }

	/// <summary>
	/// 今持っている銃のリロードの進み具合（0がリロードの始まり、1が終わり）
	/// </summary>
	float ReloadProgress { get; }

	/// <summary>
	/// 指定した銃がリロード中か？
	/// </summary>
	/// <param name="gunTYPE">銃のタイプ</param>
	/// <returns>リロード中か？</returns>
	bool IsGunReloadTimeActive(EnumManager.GunTYPE gunTYPE);

	/// <summary>
	/// ショットガンのリロードの、今の段階と、その段階の進み具合を求める
	/// </summary>
	/// <param name="phaseProgress">今の段階の進み具合（0が始まり、1が終わり）</param>
	/// <returns>今の段階</returns>
	EnumManager.ShotGunReloadPhaseTYPE GetShotGunReloadPhase(out float phaseProgress);

	/// <summary>
	/// ハンドガンに持ち替えている間か？
	/// </summary>
	bool IsSideArmActive { get; }

	/// <summary>
	/// ハンドガンに持ち替える動きの段階
	/// </summary>
	EnumManager.SideArmPhaseTYPE SideArmPhase { get; }

	/// <summary>
	/// ハンドガンを抜く動き・しまう動きの進み具合（0が始まり、1が終わり）
	/// </summary>
	float SideArmPhaseProgress { get; }

	/// <summary>
	/// ハンドガンを抜く動き・しまう動きのうち、右手がホルスターに着く割合（ここでハンドガンを手に持つ・ホルスターに戻す）
	/// </summary>
	float SideArmHolsterReachRate { get; }

	/// <summary>
	/// ハンドガンに持ち替える前に持っていた、メインの銃
	/// </summary>
	EnumManager.GunTYPE SideArmMainGunTYPE { get; }

	/// <summary>
	/// 武器を切り替える動きの段階
	/// </summary>
	EnumManager.WeaponSwitchPhaseTYPE WeaponSwitchPhase { get; }

	/// <summary>
	/// 銃をしまう動き・取り出す動きの進み具合（0が始まり、1が終わり）
	/// </summary>
	float WeaponSwitchPhaseProgress { get; }
}
