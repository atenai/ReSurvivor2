using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

/// <summary>
/// プレイヤーモデル
/// </summary>
public class PlayerModel : MonoBehaviour
{
	[Header("キャラクターモデル")]
	[Tooltip("キャラクターの首ボーン")]
	[SerializeField] Transform neck_01;
	[Tooltip("キャラクターの首ボーンの初期値")]
	float neck_01_InitEulerAnglesY;
	bool isNeck01AnimationRotInit = false;
	[Tooltip("キャラクターの脊椎ボーン")]
	[SerializeField] Transform spine_03;
	public Transform Spine_03 => spine_03;

	[Tooltip("キャラクターの脊椎ボーンの初期値")]
	float spine_03_InitEulerAnglesX;
	bool isSpine03AnimationRotInit = false;
	//※型のボーンを曲げると銃の持つ位置がずれておかしくなる為、首と背骨のボーンを曲げる事によって型のボーンを曲げずに済むようにする必要がある
	[Tooltip("キャラクターの右肩ボーン")]
	[SerializeField] Transform upperarm_r;
	public Transform Upperarm_r => upperarm_r;

	[Tooltip("キャラクターの左肩ボーン")]
	[SerializeField] Transform upperarm_l;
	public Transform Upperarm_l => upperarm_l;

	[Tooltip("肩のXボーンを曲げる数値(エイムアニメーションの銃の位置をカメラの中心に合わせる為の数値)")]
	public readonly float Arm_Aim_Animation_Rot_X = 0.0f;//0にすれば武器を構えた際の腕のずれがなくなる
	[Tooltip("肩のYボーンを曲げる数値(エイムアニメーションの銃の位置をカメラの中心に合わせる為の数値)")]
	public readonly float Arm_Aim_Animation_Rot_Y = 0.0f;//0にすれば武器を構えた際の腕のずれがなくなる

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

	void Update()
	{
		//コンピュータを使用中は切り上げる
		if (ScreenUI.SingletonInstance.IsComputerMenuActive == true)
		{
			ResetMoveAnimation();
			return;
		}

		//↑ロード中に動かせる処理
		if (InGameManager.SingletonInstance.IsGamePlayReady == false)
		{
			ResetMoveAnimation();
			return;
		}
		//↓ロード中に動かせない処理

		NormalMoveAnimation();
	}

	/// <summary>
	/// 移動アニメーション
	/// </summary>
	void NormalMoveAnimation()
	{
		Player.SingletonInstance.Animator.SetFloat("f_moveSpeedX", Player.SingletonInstance.InputHorizontal);
		Player.SingletonInstance.Animator.SetFloat("f_moveSpeedY", Player.SingletonInstance.InputVertical);
		Player.SingletonInstance.Animator.SetBool("b_isAim", Player.SingletonInstance.IsAim);
	}

	/// <summary>
	/// 移動アニメーションをリセットする
	/// </summary>
	void ResetMoveAnimation()
	{
		Player.SingletonInstance.Animator.SetFloat("f_moveSpeedX", 0.0f);
		Player.SingletonInstance.Animator.SetFloat("f_moveSpeedY", 0.0f);
		Player.SingletonInstance.Animator.SetBool("b_isAim", false);
	}

	void LateUpdate()
	{
		//ボーンを曲げる際は必ずLateUpdateに書く必要がある！（これいつかメモする！）
		RotateBoneNeck01();
		RotateBoneSpine03();
		RotateBoneUpperArmR();
		RotateBoneUpperArmL();
	}

	/// <summary>
	/// キャラクターの首ボーンを曲げる
	/// </summary> 
	void RotateBoneNeck01()
	{
		if (Player.SingletonInstance.IsAim == true)
		{
			const float aimAnimationRotY = -20.0f;
			//腰のボーンの角度をカメラの向きにする
			neck_01.rotation = Quaternion.Euler(neck_01.eulerAngles.x, neck_01.eulerAngles.y + aimAnimationRotY, neck_01.eulerAngles.z);
			isNeck01AnimationRotInit = true;
		}
		else if (Player.SingletonInstance.IsAim == false)
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
	void RotateBoneSpine03()
	{
		if (Player.SingletonInstance.IsAim == true)
		{
			const float aimAnimationRotX = 12.5f;
			const float aimAnimationRotY = 12.5f;
			//腰のボーンの角度をカメラの向きにする
			spine_03.rotation = Quaternion.Euler(PlayerCamera.SingletonInstance.transform.localEulerAngles.x + aimAnimationRotX, spine_03.eulerAngles.y + aimAnimationRotY, spine_03.eulerAngles.z);
			isSpine03AnimationRotInit = true;
		}
		else if (Player.SingletonInstance.IsAim == false)
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
	void RotateBoneUpperArmR()
	{
		if (Player.SingletonInstance.IsAim == true)
		{
			//右肩のボーンの角度をカメラの向きにする
			//upperarm_r.rotation = Quaternion.Euler(PlayerCamera.singletonInstance.transform.localEulerAngles.x + aimAnimationRotX, upperarm_r.eulerAngles.y + aimAnimationRotY, upperarm_r.eulerAngles.z);
			upperarm_r.rotation = Quaternion.Euler(upperarm_r.eulerAngles.x + Arm_Aim_Animation_Rot_X, upperarm_r.eulerAngles.y + Arm_Aim_Animation_Rot_Y, upperarm_r.eulerAngles.z);
		}
		else if (Player.SingletonInstance.IsAim == false)
		{
			//右肩のボーンの角度を真正面（初期値）にする
			upperarm_r.rotation = Quaternion.Euler(upperarm_r.eulerAngles.x, upperarm_r.eulerAngles.y, upperarm_r.eulerAngles.z);
		}
	}

	/// <summary>
	/// キャラクターの左肩ボーンを曲げる
	/// </summary> 
	void RotateBoneUpperArmL()
	{
		if (Player.SingletonInstance.IsAim == true)
		{
			//左肩のボーンの角度をカメラの向きにする
			upperarm_l.rotation = Quaternion.Euler(upperarm_l.eulerAngles.x + Arm_Aim_Animation_Rot_X, upperarm_l.eulerAngles.y + Arm_Aim_Animation_Rot_Y, upperarm_l.eulerAngles.z);
		}
		else if (Player.SingletonInstance.IsAim == false)
		{
			//左肩のボーンの角度を真正面（初期値）にする
			upperarm_l.rotation = Quaternion.Euler(upperarm_l.eulerAngles.x, upperarm_l.eulerAngles.y, upperarm_l.eulerAngles.z);
		}
	}
}
