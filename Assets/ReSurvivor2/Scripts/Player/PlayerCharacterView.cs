using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

/// <summary>
/// プレイヤーキャラクタービュー
/// MVPパターンのView担当
/// </summary>
public class PlayerCharacterView : MonoBehaviour
{
	[Tooltip("アニメーター")]
	[SerializeField] Animator animator;
	public Animator Animator => animator;

	[Header("キャラクターモデル")]
	[Tooltip("キャラクターの首ボーン")]
	[SerializeField] Transform neck_01;
	[Tooltip("キャラクターの脊椎ボーン")]
	[SerializeField] Transform spine_03;
	[Tooltip("アニメーターの、構えている割合のパラメーターの名前（0で構えていない、1で構えている。構えの切り替えに合わせて、なめらかに変わる）")]
	const string AimParameterName = "f_aim";
	//※型のボーンを曲げると銃の持つ位置がずれておかしくなる為、首と背骨のボーンを曲げる事によって型のボーンを曲げずに済むようにする必要がある
	[Tooltip("キャラクターの右肩ボーン")]
	[SerializeField] Transform upperarm_r;

	[Tooltip("キャラクターの左肩ボーン")]
	[SerializeField] Transform upperarm_l;

	[Tooltip("肩のXボーンを曲げる数値(エイムアニメーションの銃の位置をカメラの中心に合わせる為の数値)")]
	public readonly float Arm_Aim_Animation_Rot_X = 0.0f;//0にすれば武器を構えた際の腕のずれがなくなる
	[Tooltip("肩のYボーンを曲げる数値(エイムアニメーションの銃の位置をカメラの中心に合わせる為の数値)")]
	public readonly float Arm_Aim_Animation_Rot_Y = 0.0f;//0にすれば武器を構えた際の腕のずれがなくなる

	[Header("ガンモデル")]
	[Tooltip("ガンモデルファサード")]
	[SerializeField] GunModelFacade gunModelFacade = new GunModelFacade();
	public GunModelFacade GunModelFacade => gunModelFacade;

	[Header("銃ごとのアニメーション")]
	//※銃の状態（リロード中か・リロードの進み具合など）を見て、プレイヤーのアニメーションを動かす（銃のクラスからはこのクラスを使わない）
	[Tooltip("ハンドガンを持っている時のアニメーションの設定")]
	[SerializeField] GunAnimation handGunAnimation = new GunAnimation();
	[Tooltip("アサルトライフルを持っている時のアニメーションの設定")]
	[SerializeField] GunAnimation assaultRifleAnimation = new GunAnimation();
	[Tooltip("ショットガンを持っている時のアニメーションの設定")]
	[SerializeField] GunAnimation shotGunAnimation = new GunAnimation();
	//※ショットガンは弾を1発ずつ込めるため、リロードアニメーションの「ポーチから弾を取る → 装填口に込める」の区間を、
	//  込める弾の数だけ行ったり来たりさせる（2発目以降は、装填口から少しだけ巻き戻した所で次の弾を取る）
	[Tooltip("ショットガンの2発目以降の弾を取るタイミング（リロードアニメーションの再生位置）。装填口に込めるタイミングから、ここまで巻き戻して次の弾を取る")]
	[SerializeField] float shotGunShellPickTime;

	[Header("リロードのアニメーション")]
	[Tooltip("アニメーターのリロードアニメーションの再生位置のパラメーターの名前")]
	const string ReloadAnimationTimeParameterName = "f_reloadAnimationTime";
	[Tooltip("アニメーターのハンドガンのリロードアニメーションをオンにするパラメーターの名前")]
	const string HandGunReloadParameterName = "b_isHandGunReload";
	[Tooltip("アニメーターのアサルトライフルのリロードアニメーションをオンにするパラメーターの名前")]
	const string AssaultRifleReloadParameterName = "b_isAssaultRifleReload";
	[Tooltip("アニメーターのショットガンのリロードアニメーションをオンにするパラメーターの名前")]
	const string ShotGunReloadParameterName = "b_isShotGunReload";
	[Tooltip("リロードアニメーションの始まりの再生位置")]
	const float ReloadAnimationStartTime = 0.0f;
	[Tooltip("リロードアニメーションの終わりの再生位置")]
	const float ReloadAnimationEndTime = 1.0f;
	[Tooltip("リロードアニメーションの再生位置（0がアニメーションの始まり、1が終わり）")]
	float reloadAnimationTime = ReloadAnimationStartTime;
	public float ReloadAnimationTime => reloadAnimationTime;
	[Tooltip("リロードアニメーションを前へ進めているか？（ショットガンは弾を1発込めるたびに少し巻き戻して次の弾を取りに行くので、巻き戻している間はfalseになる）")]
	bool isReloadAnimationForward = true;

	[Header("ハンドガンの片手で狙う構え")]
	[Tooltip("アニメーターの、片手で狙っている割合のパラメーターの名前")]
	const string OneHandAimParameterName = "f_oneHandAim";
	[Tooltip("ハンドガンを構え始めてから、片手で狙う時間（秒）（この後、左手を添えて両手で狙う）")]
	const float OneHandAimTime = 1.0f;
	[Tooltip("片手で狙う構えから、両手で狙う構えに移るのにかける時間（秒）")]
	const float TwoHandAimBlendTime = 0.3f;
	[Tooltip("アニメーターに渡す、片手で狙っている割合（片手で狙っている時）")]
	const float OneHandAimValue = 1.0f;
	[Tooltip("アニメーターに渡す、片手で狙っている割合（両手で狙っている時）")]
	const float TwoHandAimValue = 0.0f;
	[Tooltip("構え続けている時間（秒）（構えていない時）")]
	const float NotAimContinueTime = 0.0f;
	[Tooltip("構え続けている時間（秒）")]
	float aimContinueTime = NotAimContinueTime;

	[Tooltip("地雷のプレファブ")]
	[SerializeField] GameObject minePrefab;
	public GameObject MinePrefab => minePrefab;

	void Start()
	{
		ResetMoveAnimation();
		handGunAnimation.Initialize();
		assaultRifleAnimation.Initialize();
		shotGunAnimation.Initialize();
	}

	void LateUpdate()
	{
		UpdateReloadMagazines();
	}

	/// <summary>
	/// 銃の種類から、その銃を持っている時のアニメーションの設定を探す
	/// </summary>
	/// <param name="gunTYPE">銃の種類</param>
	/// <returns>アニメーションの設定（見つからなかった場合はnull）</returns>
	public GunAnimation FindGunAnimation(EnumManager.GunTYPE gunTYPE)
	{
		switch (gunTYPE)
		{
			case EnumManager.GunTYPE.HandGun:
				return handGunAnimation;
			case EnumManager.GunTYPE.AssaultRifle:
				return assaultRifleAnimation;
			case EnumManager.GunTYPE.ShotGun:
				return shotGunAnimation;
		}
		return null;
	}

	/// <summary>
	/// 銃の状態に合わせて、リロードのアニメーションを動かす（銃の更新処理の後に呼ぶ）
	/// ・リロード中の銃のリロードアニメーションをオンにする
	/// ・リロードアニメーションの再生位置を、リロードの進み具合に合わせる（こうすると、リロード時間が変わっても、アニメーションがちょうどリロードの終わりで終わる）
	/// ※リロードしていない時は再生位置を変えない（リロードが終わってアニメーションを抜ける間に、手が急に動かないようにする）
	/// </summary>
	/// <param name="gunFacade">銃</param>
	public void UpdateReloadAnimation(GunFacade gunFacade)
	{
		animator.SetBool(HandGunReloadParameterName, gunFacade.FindGunBase(EnumManager.GunTYPE.HandGun).IsReloadTimeActive);
		animator.SetBool(AssaultRifleReloadParameterName, gunFacade.FindGunBase(EnumManager.GunTYPE.AssaultRifle).IsReloadTimeActive);
		animator.SetBool(ShotGunReloadParameterName, gunFacade.FindGunBase(EnumManager.GunTYPE.ShotGun).IsReloadTimeActive);

		GunBase gunBase = gunFacade.GetGunBase;
		if (gunBase.IsReloadTimeActive == false)
		{
			return;
		}

		if (gunFacade.GetGunTYPE == EnumManager.GunTYPE.ShotGun)
		{
			UpdateShotGunReloadAnimationTime((ShotGun)gunBase);
		}
		else
		{
			reloadAnimationTime = gunBase.ReloadProgress;
			isReloadAnimationForward = true;
		}
		animator.SetFloat(ReloadAnimationTimeParameterName, reloadAnimationTime);
	}

	/// <summary>
	/// ショットガンのリロードアニメーションの再生位置を、今込めている弾に合わせて決める
	/// （最初 → 込める弾の数だけ「ポーチ → 装填口」を繰り返す → 最後、の順に進める。装填口から次の弾を取りに戻る間は巻き戻す）
	/// </summary>
	/// <param name="shotGun">ショットガン</param>
	void UpdateShotGunReloadAnimationTime(ShotGun shotGun)
	{
		float phaseProgress;
		EnumManager.ShotGunReloadPhaseTYPE phase = shotGun.GetReloadPhase(out phaseProgress);
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

	/// <summary>
	/// 銃ごとに、リロード中のマガジンを動かす（抜き取る → 左手で持つ → 差し込む）
	/// ※アニメーションで手が動いた後に動かす
	/// </summary>
	void UpdateReloadMagazines()
	{
		GunFacade gunFacade = PlayerCameraManager.SingletonInstance.GetGunFacade;
		Transform hand_l = animator.GetBoneTransform(HumanBodyBones.LeftHand);
		handGunAnimation.UpdateReloadMagazine(gunFacade.FindGunBase(EnumManager.GunTYPE.HandGun).IsReloadTimeActive, reloadAnimationTime, isReloadAnimationForward, hand_l);
		assaultRifleAnimation.UpdateReloadMagazine(gunFacade.FindGunBase(EnumManager.GunTYPE.AssaultRifle).IsReloadTimeActive, reloadAnimationTime, isReloadAnimationForward, hand_l);
		shotGunAnimation.UpdateReloadMagazine(gunFacade.FindGunBase(EnumManager.GunTYPE.ShotGun).IsReloadTimeActive, reloadAnimationTime, isReloadAnimationForward, hand_l);
	}

	/// <summary>
	/// ハンドガンを構えた時の、片手で狙う構えと両手で狙う構えを切り替える
	/// ・構え始めた時は、まず片手で狙い、少ししてから左手を添えて両手で狙う
	/// ・敵が警戒体制の時は、片手で狙わずに、最初から両手で狙う
	/// </summary>
	/// <param name="isAim">構えているか？</param>
	/// <param name="isEnemyAlert">警戒体制の敵がいるか？</param>
	public void UpdateOneHandAimAnimation(bool isAim, bool isEnemyAlert)
	{
		if (isAim == false)
		{
			//構えをやめた時は、片手で狙っている割合はそのままにしておく（構えをやめるアニメーションの間に、左手が急に銃から離れないようにする）
			aimContinueTime = NotAimContinueTime;
			return;
		}

		if (isEnemyAlert == true)
		{
			if (aimContinueTime <= NotAimContinueTime)
			{
				//構え始めた時は、片手で狙う時間を飛ばして、すぐに両手で狙う
				aimContinueTime = OneHandAimTime + TwoHandAimBlendTime;
			}
			else if (aimContinueTime < OneHandAimTime)
			{
				//片手で狙っている途中で警戒体制になった時は、そこから両手で狙う構えに移る（左手が急に銃に飛びつかないようにする）
				aimContinueTime = OneHandAimTime;
			}
		}

		aimContinueTime = aimContinueTime + Time.deltaTime;
		animator.SetFloat(OneHandAimParameterName, Mathf.SmoothStep(OneHandAimValue, TwoHandAimValue, (aimContinueTime - OneHandAimTime) / TwoHandAimBlendTime));
	}

	/// <summary>
	/// 移動アニメーションをリセットする
	/// </summary>
	public void ResetMoveAnimation()
	{
		animator.SetFloat("f_moveSpeedX", 0.0f);
		animator.SetFloat("f_moveSpeedY", 0.0f);
		animator.SetBool("b_isAim", false);
	}

	/// <summary>
	/// キャラクターの首ボーンを曲げる
	/// ※構えている割合に合わせて、アニメーションどおりの向きから曲げた向きへ混ぜる（構えた瞬間・構えをやめた瞬間に、首が一気に回らないようにする）
	/// </summary>
	public void RotateBoneNeck01()
	{
		const float aimAnimationRotY = -20.0f;
		//首のボーンの角度を、構えている時の角度にする
		Quaternion aimRotation = Quaternion.Euler(neck_01.eulerAngles.x, neck_01.eulerAngles.y + aimAnimationRotY, neck_01.eulerAngles.z);
		neck_01.rotation = Quaternion.Slerp(neck_01.rotation, aimRotation, animator.GetFloat(AimParameterName));
	}

	/// <summary>
	/// キャラクターの脊椎ボーンを曲げる
	/// ※構えている割合に合わせて、アニメーションどおりの向きからカメラの向きへ混ぜる
	///   （ダッシュなどで前に倒れている上半身が、構えた瞬間に一気に起き上がって、両手と銃が飛ばないようにする。構えをやめた時も同じように戻す）
	/// </summary>
	public void RotateBoneSpine03(Transform cameraTransform)
	{
		const float aimAnimationRotX = 12.5f;
		const float aimAnimationRotY = 12.5f;
		//腰のボーンの角度をカメラの向きにする
		Quaternion aimRotation = Quaternion.Euler(cameraTransform.localEulerAngles.x + aimAnimationRotX, spine_03.eulerAngles.y + aimAnimationRotY, spine_03.eulerAngles.z);
		spine_03.rotation = Quaternion.Slerp(spine_03.rotation, aimRotation, animator.GetFloat(AimParameterName));
	}

	/// <summary>
	/// キャラクターの右肩ボーンを曲げる
	/// </summary> 
	public void RotateBoneUpperArmR(bool isAim)
	{
		if (isAim == true)
		{
			//右肩のボーンの角度をカメラの向きにする
			//upperarm_r.rotation = Quaternion.Euler(PlayerCamera.singletonInstance.transform.localEulerAngles.x + aimAnimationRotX, upperarm_r.eulerAngles.y + aimAnimationRotY, upperarm_r.eulerAngles.z);
			upperarm_r.rotation = Quaternion.Euler(upperarm_r.eulerAngles.x + Arm_Aim_Animation_Rot_X, upperarm_r.eulerAngles.y + Arm_Aim_Animation_Rot_Y, upperarm_r.eulerAngles.z);
		}
		else if (isAim == false)
		{
			//右肩のボーンの角度を真正面（初期値）にする
			upperarm_r.rotation = Quaternion.Euler(upperarm_r.eulerAngles.x, upperarm_r.eulerAngles.y, upperarm_r.eulerAngles.z);
		}
	}

	/// <summary>
	/// キャラクターの左肩ボーンを曲げる
	/// </summary> 
	public void RotateBoneUpperArmL(bool isAim)
	{
		if (isAim == true)
		{
			//左肩のボーンの角度をカメラの向きにする
			upperarm_l.rotation = Quaternion.Euler(upperarm_l.eulerAngles.x + Arm_Aim_Animation_Rot_X, upperarm_l.eulerAngles.y + Arm_Aim_Animation_Rot_Y, upperarm_l.eulerAngles.z);
		}
		else if (isAim == false)
		{
			//左肩のボーンの角度を真正面（初期値）にする
			upperarm_l.rotation = Quaternion.Euler(upperarm_l.eulerAngles.x, upperarm_l.eulerAngles.y, upperarm_l.eulerAngles.z);
		}
	}

	void OnGUI()
	{
#if UNITY_EDITOR//Unityエディター上での処理

		GUIStyle styleGreen = new GUIStyle();
		styleGreen.fontSize = 30;
		GUIStyleState styleStateGreen = new GUIStyleState();
		styleStateGreen.textColor = Color.green;
		styleGreen.normal = styleStateGreen;

		GUIStyle styleRed = new GUIStyle();
		styleRed.fontSize = 30;
		GUIStyleState styleStateRed = new GUIStyleState();
		styleStateRed.textColor = Color.red;
		styleRed.normal = styleStateRed;

		GUIStyle styleBlack = new GUIStyle();
		styleBlack.fontSize = 30;
		GUIStyleState styleStateBlack = new GUIStyleState();
		styleStateBlack.textColor = Color.black;
		styleBlack.normal = styleStateBlack;

		GUIStyle styleYellow = new GUIStyle();
		styleYellow.fontSize = 30;
		GUIStyleState styleStateYellow = new GUIStyleState();
		styleStateYellow.textColor = Color.yellow;
		styleYellow.normal = styleStateYellow;

		int lineHeight = 50;

		GUI.Box(new Rect(10, 9 * lineHeight, 100, 50), "spine_03.eulerAngles", styleGreen);
		GUI.Box(new Rect(350, 9 * lineHeight, 100, 50), spine_03.eulerAngles.ToString(), styleGreen);
		GUI.Box(new Rect(10, 10 * lineHeight, 100, 50), "upperarm_r.eulerAngles.x + armAimAnimationRotX", styleGreen);
		GUI.Box(new Rect(750, 10 * lineHeight, 100, 50), upperarm_r.eulerAngles.x + Arm_Aim_Animation_Rot_X.ToString(), styleGreen);
		GUI.Box(new Rect(10, 11 * lineHeight, 100, 50), "upperarm_r.eulerAngles.y + armAimAnimationRotY", styleGreen);
		GUI.Box(new Rect(750, 11 * lineHeight, 100, 50), upperarm_r.eulerAngles.y + Arm_Aim_Animation_Rot_Y.ToString(), styleGreen);
		GUI.Box(new Rect(10, 12 * lineHeight, 100, 50), "upperarm_l.eulerAngles.x + armAimAnimationRotX", styleGreen);
		GUI.Box(new Rect(750, 12 * lineHeight, 100, 50), upperarm_l.eulerAngles.x + Arm_Aim_Animation_Rot_X.ToString(), styleGreen);
		GUI.Box(new Rect(10, 13 * lineHeight, 100, 50), "upperarm_l.eulerAngles.y + armAimAnimationRotY", styleGreen);
		GUI.Box(new Rect(750, 13 * lineHeight, 100, 50), upperarm_l.eulerAngles.y + Arm_Aim_Animation_Rot_Y.ToString(), styleGreen);

#endif //終了  
	}
}
