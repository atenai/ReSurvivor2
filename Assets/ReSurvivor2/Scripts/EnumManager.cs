/// <summary>
/// Enumを管理するマネージャークラス
/// </summary>
public static class EnumManager
{
	/// <summary>
	/// 銃のタイプ
	/// </summary>
	public enum GunTYPE
	{
		HandGun = 1,
		AssaultRifle = 2,
		ShotGun = 3,
	}

	/// <summary>
	/// ショットガンのリロードの段階（弾を1発ずつ込める）
	/// </summary>
	public enum ShotGunReloadPhaseTYPE
	{
		//銃を弾を込める姿勢にして、左手が腰の後ろのポーチまで弾を取りに行く
		Start = 0,
		//1発目の弾を、ポーチから装填口まで持っていって込める
		FirstShellInsert = 1,
		//弾を込めた後、次の弾を取りに腰のベルトへ戻る
		ShellReturn = 2,
		//2発目以降の弾を、腰のベルトから装填口まで持っていって込める
		ShellInsert = 3,
		//最後の弾を込めてから、元の構えに戻る
		End = 4,
	}

	/// <summary>
	/// 地雷を置く動きの段階
	/// </summary>
	public enum MinePlacePhaseTYPE
	{
		//地雷を置いていない
		None = 0,
		//地雷を置くボタンを押している間、しゃがんで左手を地面へ下ろしていく
		Crouch = 1,
		//地雷を置いた後、立ち上がる
		StandUp = 2,
		//長押しの途中でボタンを離して地雷を置くのをやめたので、しゃがんだ分だけ戻して立ち上がる
		Cancel = 3,
	}

	/// <summary>
	/// アサルトライフル・ショットガンの弾が切れた時に、ハンドガンに持ち替える動きの段階
	/// </summary>
	public enum SideArmPhaseTYPE
	{
		//ハンドガンに持ち替えていない
		None = 0,
		//メインの銃（アサルトライフル・ショットガン）を左手に持ち替えて、右手で太もものホルスターからハンドガンを抜く
		Draw = 1,
		//右手のハンドガンだけで狙って撃つ（左手はメインの銃を持ったまま）
		Aim = 2,
		//ハンドガンをホルスターにしまって、メインの銃を右手に持ち直す
		Holster = 3,
	}

	/// <summary>
	/// ステージのタイプ
	/// </summary>
	public enum StageTYPE
	{
		Stage0 = 0,
		Stage1 = 1,
		Stage2 = 2,
		Stage3 = 3,
		Stage4 = 4,
		Stage5 = 5,
		Stage6 = 6,
		Stage7 = 7,
		Stage8 = 8,
		Stage9 = 9,
		Stage10 = 10,
		Stage11 = 11,
		Stage12 = 12,
		Stage13 = 13,
		Stage14 = 14,
		Stage15 = 15,
		Stage16 = 16,
		Stage17 = 17,
		Stage18 = 18,
		Stage19 = 19,
		Stage20 = 20,
	}
}
