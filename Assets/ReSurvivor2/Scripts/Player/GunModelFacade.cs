using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ガンモデルファサード
/// </summary>
[System.Serializable]
public class GunModelFacade
{
	[Header("ハンドガン")]
	[Tooltip("ハンドガンビュー")]
	[SerializeField] HandGunView handGunView;
	public HandGunView HandGunView => handGunView;

	[Header("アサルトライフルモデル")]
	[Tooltip("アサルトライフルのモデル")]
	[SerializeField] AssaultRifleModel assaultRifleModel;
	public AssaultRifleModel AssaultRifleModel => assaultRifleModel;

	[Header("ショットガンモデル")]
	[Tooltip("ショットガンのモデル")]
	[SerializeField] ShotGunModel shotGunModel;
	public ShotGunModel ShotGunModel => shotGunModel;

	/// <summary>
	/// 銃のモデルを切り替え
	/// </summary>
	public void SwitchWeaponModel(EnumManager.GunTYPE gunTYPE)
	{
		handGunView.GetHandGunModel.SetActive(false);
		handGunView.GetHandGunModelBodyDecoration.SetActive(false);
		assaultRifleModel.GetAssaultRifleModel.SetActive(false);
		assaultRifleModel.GetAssaultRifleModelBodyDecoration.SetActive(false);
		shotGunModel.GetShotGunModel.SetActive(false);
		shotGunModel.GetShotGunModelBodyDecoration.SetActive(false);

		switch (gunTYPE)
		{
			case EnumManager.GunTYPE.HandGun:
				handGunView.GetHandGunModel.SetActive(true);
				assaultRifleModel.GetAssaultRifleModelBodyDecoration.SetActive(true);
				shotGunModel.GetShotGunModelBodyDecoration.SetActive(true);
				break;
			case EnumManager.GunTYPE.AssaultRifle:
				handGunView.GetHandGunModelBodyDecoration.SetActive(true);
				assaultRifleModel.GetAssaultRifleModel.SetActive(true);
				shotGunModel.GetShotGunModelBodyDecoration.SetActive(true);
				break;
			case EnumManager.GunTYPE.ShotGun:
				handGunView.GetHandGunModelBodyDecoration.SetActive(true);
				assaultRifleModel.GetAssaultRifleModelBodyDecoration.SetActive(true);
				shotGunModel.GetShotGunModel.SetActive(true);
				break;
		}
	}
}
