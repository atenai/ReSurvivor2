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
	public void SwitchWeaponModel(EnumManager.GunTYPE gunTYPE)
	{
		handGunModel.GetHandGunModel.SetActive(false);
		handGunModel.GetHandGunModelBodyDecoration.SetActive(false);
		assaultRifleModel.GetAssaultRifleModel.SetActive(false);
		assaultRifleModel.GetAssaultRifleModelBodyDecoration.SetActive(false);
		shotGunModel.GetShotGunModel.SetActive(false);
		shotGunModel.GetShotGunModelBodyDecoration.SetActive(false);

		switch (gunTYPE)
		{
			case EnumManager.GunTYPE.HandGun:
				handGunModel.GetHandGunModel.SetActive(true);
				assaultRifleModel.GetAssaultRifleModelBodyDecoration.SetActive(true);
				shotGunModel.GetShotGunModelBodyDecoration.SetActive(true);
				break;
			case EnumManager.GunTYPE.AssaultRifle:
				handGunModel.GetHandGunModelBodyDecoration.SetActive(true);
				assaultRifleModel.GetAssaultRifleModel.SetActive(true);
				shotGunModel.GetShotGunModelBodyDecoration.SetActive(true);
				break;
			case EnumManager.GunTYPE.ShotGun:
				handGunModel.GetHandGunModelBodyDecoration.SetActive(true);
				assaultRifleModel.GetAssaultRifleModelBodyDecoration.SetActive(true);
				shotGunModel.GetShotGunModel.SetActive(true);
				break;
		}
	}

	/// <summary>
	/// 銃のタイプから銃のモデルを探す
	/// </summary>
	/// <param name="gunTYPE">銃のタイプ</param>
	/// <returns>見つかった銃のモデル（見つからなかった場合はnull）</returns>
	public GunModelBase FindGunModel(EnumManager.GunTYPE gunTYPE)
	{
		switch (gunTYPE)
		{
			case EnumManager.GunTYPE.HandGun:
				return handGunModel;
			case EnumManager.GunTYPE.AssaultRifle:
				return assaultRifleModel;
			case EnumManager.GunTYPE.ShotGun:
				return shotGunModel;
		}
		return null;
	}
}
