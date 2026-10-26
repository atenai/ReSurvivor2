using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ガンビューファサード
/// MVPパターンのView担当
/// </summary>
[System.Serializable]
public class GunViewFacade
{
	[Header("ハンドガン")]
	[Tooltip("ハンドガンビュー")]
	[SerializeField] HandGunView handGunView;
	public HandGunView HandGunView => handGunView;

	[Header("アサルトライフル")]
	[Tooltip("アサルトライフルのビュー")]
	[SerializeField] AssaultRifleView assaultRifleView;
	public AssaultRifleView AssaultRifleView => assaultRifleView;

	[Header("ショットガンビュー")]
	[Tooltip("ショットガンのビュー")]
	[SerializeField] ShotGunView shotGunView;
	public ShotGunView ShotGunView => shotGunView;

	/// <summary>
	/// 銃の種類から、右手に持っている銃のモデルを探す
	/// </summary>
	/// <param name="gunTYPE">銃の種類</param>
	/// <returns>銃のモデル（前向きが銃身、上向きが銃の上側。見つからなかった場合はnull）</returns>
	public Transform FindGunModelTransform(EnumManager.GunTYPE gunTYPE)
	{
		switch (gunTYPE)
		{
			case EnumManager.GunTYPE.HandGun:
				return handGunView.GetHandGunModel.transform;
			case EnumManager.GunTYPE.AssaultRifle:
				return assaultRifleView.GetAssaultRifleModel.transform;
			case EnumManager.GunTYPE.ShotGun:
				return shotGunView.GetShotGunModel.transform;
		}
		return null;
	}

	/// <summary>
	/// 銃のモデルを切り替え
	/// </summary>
	public void SwitchWeaponModel(EnumManager.GunTYPE gunTYPE)
	{
		handGunView.GetHandGunModel.SetActive(false);
		handGunView.GetHandGunModelBodyDecoration.SetActive(false);
		assaultRifleView.GetAssaultRifleModel.SetActive(false);
		assaultRifleView.GetAssaultRifleModelBodyDecoration.SetActive(false);
		shotGunView.GetShotGunModel.SetActive(false);
		shotGunView.GetShotGunModelBodyDecoration.SetActive(false);

		switch (gunTYPE)
		{
			case EnumManager.GunTYPE.HandGun:
				handGunView.GetHandGunModel.SetActive(true);
				assaultRifleView.GetAssaultRifleModelBodyDecoration.SetActive(true);
				shotGunView.GetShotGunModelBodyDecoration.SetActive(true);
				break;
			case EnumManager.GunTYPE.AssaultRifle:
				handGunView.GetHandGunModelBodyDecoration.SetActive(true);
				assaultRifleView.GetAssaultRifleModel.SetActive(true);
				shotGunView.GetShotGunModelBodyDecoration.SetActive(true);
				break;
			case EnumManager.GunTYPE.ShotGun:
				handGunView.GetHandGunModelBodyDecoration.SetActive(true);
				assaultRifleView.GetAssaultRifleModelBodyDecoration.SetActive(true);
				shotGunView.GetShotGunModel.SetActive(true);
				break;
		}
	}
}
