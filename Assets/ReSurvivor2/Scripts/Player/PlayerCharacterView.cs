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
	[Tooltip("キャラクターの首ボーンの初期値")]
	float neck_01_InitEulerAnglesY;
	bool isNeck01AnimationRotInit = false;
	[Tooltip("キャラクターの脊椎ボーン")]
	[SerializeField] Transform spine_03;

	[Tooltip("キャラクターの脊椎ボーンの初期値")]
	float spine_03_InitEulerAnglesX;
	bool isSpine03AnimationRotInit = false;
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

	[Tooltip("地雷のプレファブ")]
	[SerializeField] GameObject minePrefab;
	public GameObject MinePrefab => minePrefab;

	void Start()
	{
		InitBoneNeck01();
		InitBoneSpine03();
		ResetMoveAnimation();
	}

	/// <summary>
	/// キャラクターの首ボーンの初期化処理
	/// </summary> 
	void InitBoneNeck01()
	{
		//キャラクターの脊椎ボーンの初期値を取得する（真正面に戻す際に必要なため）
		neck_01_InitEulerAnglesY = neck_01.eulerAngles.y;
	}

	/// <summary>
	/// キャラクターの脊椎ボーンの初期化処理
	/// </summary> 
	void InitBoneSpine03()
	{
		//キャラクターの脊椎ボーンの初期値を取得する（真正面に戻す際に必要なため）
		spine_03_InitEulerAnglesX = spine_03.eulerAngles.x;
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
	/// </summary> 
	public void RotateBoneNeck01(bool isAim)
	{
		if (isAim == true)
		{
			const float aimAnimationRotY = -20.0f;
			//腰のボーンの角度をカメラの向きにする
			neck_01.rotation = Quaternion.Euler(neck_01.eulerAngles.x, neck_01.eulerAngles.y + aimAnimationRotY, neck_01.eulerAngles.z);
			isNeck01AnimationRotInit = true;
		}
		else if (isAim == false)
		{
			if (isNeck01AnimationRotInit == true)
			{
				//腰のボーンの角度を真正面（初期値）にする
				neck_01.rotation = Quaternion.Euler(neck_01.eulerAngles.x, neck_01_InitEulerAnglesY, neck_01.eulerAngles.z);
				isNeck01AnimationRotInit = false;
			}
		}
	}

	/// <summary>
	/// キャラクターの脊椎ボーンを曲げる
	/// </summary> 
	public void RotateBoneSpine03(bool isAim, Transform cameraTransform)
	{
		if (isAim == true)
		{
			const float aimAnimationRotX = 12.5f;
			const float aimAnimationRotY = 12.5f;
			//腰のボーンの角度をカメラの向きにする
			spine_03.rotation = Quaternion.Euler(cameraTransform.localEulerAngles.x + aimAnimationRotX, spine_03.eulerAngles.y + aimAnimationRotY, spine_03.eulerAngles.z);
			isSpine03AnimationRotInit = true;
		}
		else if (isAim == false)
		{
			if (isSpine03AnimationRotInit == true)
			{
				//腰のボーンの角度を真正面（初期値）にする
				spine_03.rotation = Quaternion.Euler(spine_03_InitEulerAnglesX, spine_03.eulerAngles.y, spine_03.eulerAngles.z);
				isSpine03AnimationRotInit = false;
			}
		}
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
