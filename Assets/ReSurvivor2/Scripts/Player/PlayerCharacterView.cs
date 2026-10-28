using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーキャラクタービュー
/// MVPパターンのView担当
/// ※プレイヤーのアニメーションの窓口。Presenterからプレイヤーの状態を受け取り、アニメーションごとのクラスに渡して動かす
///   （移動：MoveAnimation、構え：AimAnimation、リロード：ReloadAnimation、地雷：MineAnimation、ハンドガンへの持ち替え：SideArmAnimation、
///     手のIK：HandIK、銃ごとの設定：GunAnimationFacade）
/// ※銃の状態は、初期化の時に受け取った銃（GunModelFacade）から読む（銃のクラスからはこのクラスを使わない）
/// </summary>
public class PlayerCharacterView : MonoBehaviour
{
	[Tooltip("アニメーター")]
	[SerializeField] Animator animator;

	[Header("キャラクターモデル")]
	[Tooltip("キャラクターの首ボーン")]
	[SerializeField] Transform neck_01;
	[Tooltip("キャラクターの脊椎ボーン")]
	[SerializeField] Transform spine_03;
	//※型のボーンを曲げると銃の持つ位置がずれておかしくなる為、首と背骨のボーンを曲げる事によって型のボーンを曲げずに済むようにする必要がある
	[Tooltip("キャラクターの右肩ボーン")]
	[SerializeField] Transform upperarm_r;

	[Tooltip("キャラクターの左肩ボーン")]
	[SerializeField] Transform upperarm_l;

	[Tooltip("肩のXボーンを曲げる数値(エイムアニメーションの銃の位置をカメラの中心に合わせる為の数値)")]
	public readonly float Arm_Aim_Animation_Rot_X = 0.0f;//0にすれば武器を構えた際の腕のずれがなくなる
	[Tooltip("肩のYボーンを曲げる数値(エイムアニメーションの銃の位置をカメラの中心に合わせる為の数値)")]
	public readonly float Arm_Aim_Animation_Rot_Y = 0.0f;//0にすれば武器を構えた際の腕のずれがなくなる

	[Header("ガンビュー")]
	[Tooltip("ガンビュー ファサード")]
	[SerializeField] GunViewFacade gunViewFacade = new GunViewFacade();
	public GunViewFacade GunViewFacade => gunViewFacade;

	[Header("銃ごとのアニメーション")]
	[Tooltip("ガンアニメーション ファサード（銃ごとのアニメーションの設定）")]
	[SerializeField] GunAnimationFacade gunAnimationFacade = new GunAnimationFacade();

	[Tooltip("地雷のプレファブ")]
	[SerializeField] GameObject minePrefab;
	public GameObject MinePrefab => minePrefab;

	[Header("アニメーション")]
	[Tooltip("リロードのアニメーション")]
	[SerializeField] ReloadAnimation reloadAnimation = new ReloadAnimation();
	[Tooltip("地雷を置く時のアニメーション")]
	[SerializeField] MineAnimation mineAnimation = new MineAnimation();
	[Tooltip("手のIK")]
	[SerializeField] HandIK handIK = new HandIK();
	[Tooltip("移動のアニメーション")]
	readonly MoveAnimation moveAnimation = new MoveAnimation();
	[Tooltip("構えのアニメーション")]
	readonly AimAnimation aimAnimation = new AimAnimation();
	[Tooltip("ハンドガンへの持ち替えのアニメーション")]
	readonly SideArmAnimation sideArmAnimation = new SideArmAnimation();
	[Tooltip("銃（銃の状態を見て、リロードのアニメーション・リロード中のマガジン・手のIKを動かす）。初期化される前はnull")]
	GunModelFacade gunModelFacade;
	[Tooltip("前のフレームで、構えに合わせて脊椎ボーンを曲げた回転（キャラクターから見た向き）")]
	Quaternion spineAimLocalRotation = Quaternion.identity;
	[Tooltip("前のフレームで、構えに合わせて曲げた脊椎ボーンの位置（キャラクターから見た位置）")]
	Vector3 spineAimLocalPosition = Vector3.zero;

	/// <summary>
	/// 初期化（Presenterが最初に呼ぶ）
	/// </summary>
	/// <param name="gunModelFacade">銃</param>
	public void Initialize(GunModelFacade gunModelFacade)
	{
		this.gunModelFacade = gunModelFacade;
		ResetMoveAnimation();
		gunAnimationFacade.Initialize();
		handIK.Initialize(animator);
		sideArmAnimation.Initialize(gunViewFacade.HandGunView, handIK.LeftHandAnimationPos, animator);
	}

	void LateUpdate()
	{
		//※ステージを移る時に作られてすぐに消える、2つ目のプレイヤーは、初期化されないので動かさない
		if (gunModelFacade == null)
		{
			return;
		}

		//リロード中のマガジンを動かす（アニメーションで手が動いた後に動かす）
		gunAnimationFacade.UpdateReloadMagazines(gunModelFacade, reloadAnimation.ReloadAnimationTime, reloadAnimation.IsReloadAnimationForward, animator.GetBoneTransform(HumanBodyBones.LeftHand));
		//ハンドガンに持ち替える時の、銃のモデルを持ち替える（アニメーションで手が動いた後に動かす）
		sideArmAnimation.UpdateModels(gunModelFacade, gunViewFacade, animator.GetBoneTransform(HumanBodyBones.LeftHand));
	}

	/// <summary>
	/// IK用のUnity標準の関数（アニメーターと同じゲームオブジェクトに付いているので呼ばれる）
	/// </summary>
	void OnAnimatorIK()
	{
		//※ステージを移る時に作られてすぐに消える、2つ目のプレイヤーは、初期化されないので動かさない
		if (gunModelFacade == null)
		{
			return;
		}

		EnumManager.GunTYPE gunTYPE = gunModelFacade.GetGunTYPE;
		//※ハンドガンに持ち替えている間は、しまう途中でメインの銃に戻した後も、ハンドガンの持ち方を使う
		//  （ハンドガンの持ち方は、ハンドガンのアニメーションが混ざっている割合で効くので、アニメーションと一緒になめらかに0へ戻る）
		HandGunView handGunView = gunTYPE == EnumManager.GunTYPE.HandGun || gunModelFacade.IsSideArmActive == true ? gunViewFacade.HandGunView : null;
		//※背骨は、IKの後（LateUpdate）で構えに合わせて曲げるので、前のフレームで曲げた回転と位置を渡す
		Quaternion characterRotation = animator.transform.rotation;
		Quaternion spineAimRotation = characterRotation * spineAimLocalRotation * Quaternion.Inverse(characterRotation);
		Vector3 spinePosition = animator.transform.position + characterRotation * spineAimLocalPosition;
		GunAnimation sideArmMainGunAnimation = gunModelFacade.IsSideArmActive == true ? gunAnimationFacade.FindGunAnimation(gunModelFacade.SideArmMainGunTYPE) : null;
		sideArmAnimation.UpdateWeights(gunModelFacade.SideArmPhase, gunModelFacade.SideArmPhaseProgress, spineAimRotation, spinePosition, sideArmMainGunAnimation);
		handIK.UpdateIK(gunAnimationFacade.FindGunAnimation(gunTYPE), gunViewFacade.FindGunModelTransform(gunTYPE), handGunView, mineAnimation, sideArmAnimation, gunModelFacade.GunModelBase.IsReloadTimeActive, reloadAnimation.ReloadAnimationTime);
	}

	/// <summary>
	/// 移動のアニメーションを動かす
	/// </summary>
	/// <param name="inputHorizontal">移動の左右の入力</param>
	/// <param name="inputVertical">移動の前後の入力</param>
	/// <param name="isDash">ダッシュしているか？</param>
	/// <param name="isPlacingMine">地雷を置いている間か？（その場で止まっているアニメーションにする）</param>
	public void UpdateMoveAnimation(float inputHorizontal, float inputVertical, bool isDash, bool isPlacingMine)
	{
		moveAnimation.UpdateAnimation(animator, inputHorizontal, inputVertical, isDash, isPlacingMine, gunModelFacade.GetGunTYPE == EnumManager.GunTYPE.HandGun);
	}

	/// <summary>
	/// 構えのアニメーションを動かす（構えている割合・ハンドガンの片手で狙う構え・近距離の構え）
	/// </summary>
	/// <param name="isAim">構えているか？</param>
	/// <param name="isEnemyAlert">警戒体制の敵がいるか？（いる時は、ハンドガンを最初から両手で狙う）</param>
	/// <param name="isEnemyClose">狙っている方向に近い敵がいるか？（いる時は、ハンドガンを近距離の構えにする）</param>
	public void UpdateAimAnimation(bool isAim, bool isEnemyAlert, bool isEnemyClose)
	{
		aimAnimation.UpdateAnimation(animator, isAim, isEnemyAlert, isEnemyClose, gunModelFacade.IsSideArmActive);
	}

	/// <summary>
	/// 銃の状態に合わせて、リロードのアニメーションを動かす（銃の更新処理の後に呼ぶ）
	/// </summary>
	public void UpdateReloadAnimation()
	{
		reloadAnimation.UpdateAnimation(animator, gunModelFacade, gunAnimationFacade.FindGunAnimation(EnumManager.GunTYPE.ShotGun));
	}

	/// <summary>
	/// 地雷を置く動きの段階に合わせて、地雷を置くアニメーションを動かす
	/// </summary>
	/// <param name="minePlacePhase">地雷を置く動きの段階</param>
	/// <param name="crouchRate">しゃがんだ割合（長押しした時間 ÷ 長押しと判定する時間。1以上は、左手が地面に着いたまま）</param>
	/// <param name="standUpRate">立ち上がった割合（立ち上がり始めてからの時間 ÷ 立ち上がるのにかける時間）</param>
	public void UpdatePlaceMineAnimation(EnumManager.MinePlacePhaseTYPE minePlacePhase, float crouchRate, float standUpRate)
	{
		mineAnimation.UpdateAnimation(animator, minePlacePhase, crouchRate, standUpRate);
	}

	/// <summary>
	/// 移動アニメーションをリセットする（立ち姿に戻す）
	/// ※ステージを移る時（ロード中・次のステージのフェードイン中）は、毎フレーム呼ばれる
	///   ダッシュの割合・構えている割合も戻さないと、ダッシュや構えのまま移った時に、銃を持ち上げた右手や曲げた背骨が残ってしまう
	/// </summary>
	public void ResetMoveAnimation()
	{
		moveAnimation.ResetAnimation(animator);
		aimAnimation.ResetAnimation(animator);
		mineAnimation.ResetAnimation(animator);
	}

	/// <summary>
	/// 構えに合わせて、首・背骨・肩のボーンを曲げる
	/// ※アニメーションの後に曲げるので、必ずLateUpdateで呼ぶ
	/// </summary>
	/// <param name="cameraTransform">カメラ（背骨をカメラの向きに曲げる）</param>
	/// <param name="isAim">構えているか？</param>
	public void UpdateAimBones(Transform cameraTransform, bool isAim)
	{
		Quaternion spineRotationBeforeAim = spine_03.rotation;
		RotateBoneNeck01();
		RotateBoneSpine03(cameraTransform);
		RotateBoneUpperArmR(isAim);
		RotateBoneUpperArmL(isAim);

		//次のフレームのIKで、曲げた後の手の位置を合わせるのに使うため、脊椎ボーンを曲げた回転と、脊椎ボーンの位置を覚えておく（キャラクターから見た向き・位置）
		//（IKの中では、この後に曲げる量はまだ分からないので、前のフレームで曲げた量を使う。構えの割合やカメラの向きは少しずつしか変わらないので、ほとんど同じになる）
		Quaternion characterRotation = animator.transform.rotation;
		spineAimLocalRotation = Quaternion.Inverse(characterRotation) * spine_03.rotation * Quaternion.Inverse(spineRotationBeforeAim) * characterRotation;
		spineAimLocalPosition = Quaternion.Inverse(characterRotation) * (spine_03.position - animator.transform.position);
	}

	/// <summary>
	/// キャラクターの首ボーンを曲げる
	/// ※構えている割合に合わせて、アニメーションどおりの向きから曲げた向きへ混ぜる（構えた瞬間・構えをやめた瞬間に、首が一気に回らないようにする）
	/// </summary>
	void RotateBoneNeck01()
	{
		const float aimAnimationRotY = -20.0f;
		//首のボーンの角度を、構えている時の角度にする
		Quaternion aimRotation = Quaternion.Euler(neck_01.eulerAngles.x, neck_01.eulerAngles.y + aimAnimationRotY, neck_01.eulerAngles.z);
		neck_01.rotation = Quaternion.Slerp(neck_01.rotation, aimRotation, animator.GetFloat(PlayerAnimatorParameter.Aim));
	}

	/// <summary>
	/// キャラクターの脊椎ボーンを曲げる
	/// ※構えている割合に合わせて、アニメーションどおりの向きからカメラの向きへ混ぜる
	///   （ダッシュなどで前に倒れている上半身が、構えた瞬間に一気に起き上がって、両手と銃が飛ばないようにする。構えをやめた時も同じように戻す）
	/// </summary>
	void RotateBoneSpine03(Transform cameraTransform)
	{
		const float aimAnimationRotX = 12.5f;
		const float aimAnimationRotY = 12.5f;
		//腰のボーンの角度をカメラの向きにする
		Quaternion aimRotation = Quaternion.Euler(cameraTransform.localEulerAngles.x + aimAnimationRotX, spine_03.eulerAngles.y + aimAnimationRotY, spine_03.eulerAngles.z);
		spine_03.rotation = Quaternion.Slerp(spine_03.rotation, aimRotation, animator.GetFloat(PlayerAnimatorParameter.Aim));
	}

	/// <summary>
	/// キャラクターの右肩ボーンを曲げる
	/// </summary>
	void RotateBoneUpperArmR(bool isAim)
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
	void RotateBoneUpperArmL(bool isAim)
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
