using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 手のIK
/// </summary>
public class HandIK : MonoBehaviour
{
	[Tooltip("アニメーター")]
	[SerializeField] Animator animator;
	[Tooltip("ハンドガンの左手の位置")]
	[SerializeField] Transform handGunLeftHandPos;
	[Tooltip("アサルトライフルの左手の位置")]
	[SerializeField] Transform assaultRifleLeftHandPos;
	[Tooltip("ショットガンの左手の位置")]
	[SerializeField] Transform shotGunLeftHandPos;

	/// <summary>
	/// IK用のUnity標準の関数
	/// </summary> 
	void OnAnimatorIK()
	{
		if (PlayerCamera.SingletonInstance.GetGunFacade.GetGunTYPE == GunFacade.GunTYPE.HandGun)
		{
			if (PlayerCamera.SingletonInstance.GetGunFacade.GetGunBase.IsReloadTimeActive == false)
			{
				//キャラクターの左手の位置と角度を合わせる
				animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
				animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
				animator.SetIKPosition(AvatarIKGoal.LeftHand, handGunLeftHandPos.position);
				animator.SetIKRotation(AvatarIKGoal.LeftHand, handGunLeftHandPos.rotation);
			}
			else
			{

			}
		}
		else if (PlayerCamera.SingletonInstance.GetGunFacade.GetGunTYPE == GunFacade.GunTYPE.AssaultRifle)
		{
			if (PlayerCamera.SingletonInstance.GetGunFacade.GetGunBase.IsReloadTimeActive == false)
			{
				//キャラクターの左手の位置と角度を合わせる
				animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
				animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
				animator.SetIKPosition(AvatarIKGoal.LeftHand, assaultRifleLeftHandPos.position);
				animator.SetIKRotation(AvatarIKGoal.LeftHand, assaultRifleLeftHandPos.rotation);
			}
			else
			{

			}
		}
		else if (PlayerCamera.SingletonInstance.GetGunFacade.GetGunTYPE == GunFacade.GunTYPE.ShotGun)
		{
			if (PlayerCamera.SingletonInstance.GetGunFacade.GetGunBase.IsReloadTimeActive == false)
			{
				//キャラクターの左手の位置と角度を合わせる
				animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
				animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
				animator.SetIKPosition(AvatarIKGoal.LeftHand, shotGunLeftHandPos.position);
				animator.SetIKRotation(AvatarIKGoal.LeftHand, shotGunLeftHandPos.rotation);
			}
			else
			{

			}
		}
	}
}
