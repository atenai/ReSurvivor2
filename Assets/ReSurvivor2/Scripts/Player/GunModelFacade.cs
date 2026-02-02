using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ガンモデルファサード
/// </summary>
[System.Serializable]
public class GunModelFacade
{
	[Header("ハンドガンモデル")]
	[Tooltip("ハンドガンのモデル")]
	[SerializeField] HandGunModel handGunModel;
	public HandGunModel HandGunModel => handGunModel;

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
	public void SwitchWeaponModel(GunFacade.GunTYPE gunTYPE)
	{
		handGunModel.GetHandGunModel.SetActive(false);
		handGunModel.GetHandGunModelBodyDecoration.SetActive(false);
		assaultRifleModel.GetAssaultRifleModel.SetActive(false);
		assaultRifleModel.GetAssaultRifleModelBodyDecoration.SetActive(false);
		shotGunModel.GetShotGunModel.SetActive(false);
		shotGunModel.GetShotGunModelBodyDecoration.SetActive(false);

		switch (gunTYPE)
		{
			case GunFacade.GunTYPE.HandGun:
				handGunModel.GetHandGunModel.SetActive(true);
				assaultRifleModel.GetAssaultRifleModelBodyDecoration.SetActive(true);
				shotGunModel.GetShotGunModelBodyDecoration.SetActive(true);
				break;
			case GunFacade.GunTYPE.AssaultRifle:
				handGunModel.GetHandGunModelBodyDecoration.SetActive(true);
				assaultRifleModel.GetAssaultRifleModel.SetActive(true);
				shotGunModel.GetShotGunModelBodyDecoration.SetActive(true);
				break;
			case GunFacade.GunTYPE.ShotGun:
				handGunModel.GetHandGunModelBodyDecoration.SetActive(true);
				assaultRifleModel.GetAssaultRifleModelBodyDecoration.SetActive(true);
				shotGunModel.GetShotGunModel.SetActive(true);
				break;
		}
	}
}
