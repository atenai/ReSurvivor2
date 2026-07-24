using UnityEngine;
using System;
using System.ComponentModel;


/// <summary>
/// SRデバッガー
/// </summary>
public partial class SROptions
{
	#region 定数

	/// <summary>
	/// 全般カテゴリ
	/// </summary>
	private const string GeneralCategory = "General";

	#endregion


	#region デバッグ機能

	[Category(GeneralCategory)]
	[DisplayName("TimeScale")]
	[Sort(0)]
	[Increment(0.1)]
	[NumberRange(0.0, 10.0)]
	public float TimeScale
	{
		get
		{
			return Time.timeScale;
		}
		set
		{
			Time.timeScale = value;
		}
	}

	[Category(GeneralCategory)]
	[DisplayName("DisplayDateTime")]
	[Sort(1)]
	public void DisplayDateTime()
	{
		Debug.Log(DateTime.Now.ToString("yyyy/MM/dd"));
	}

	[Category(GeneralCategory)]
	[DisplayName("LightEnabled")]
	[Sort(2)]
	public bool LightEnabled
	{
		get
		{
			return GameObject.FindObjectOfType<Light>().enabled;
		}
		set
		{
			GameObject.FindObjectOfType<Light>().enabled = value;
		}
	}

	[Category("Player")]
	[DisplayName("アーマープレートを1つ取得する")]
	[Sort(0)]
	public void AcquireArmorPlate()
	{
		PlayerManagerPresenter.SingletonInstance.PlayerModel.AcquireArmorPlate((armorPlate) => PlayerManagerPresenter.SingletonInstance.PlayerUIView.StartTextArmorPlate(armorPlate));
	}

	[Category("Player")]
	[DisplayName("アーマープレートの最大数を増やす")]
	[Sort(1)]
	public void IncreaseMaxArmorPlate()
	{
		PlayerManagerPresenter.SingletonInstance.PlayerModel.IncreaseMaxArmorPlate();
	}

	[Category("Player")]
	[DisplayName("食料を1つ取得する")]
	[Sort(2)]
	public void AcquireFood()
	{
		PlayerManagerPresenter.SingletonInstance.PlayerModel.AcquireFood((food) => PlayerManagerPresenter.SingletonInstance.PlayerUIView.SetTextFood(food));
	}

	[Category("Player")]
	[DisplayName("食料の最大数を増やす")]
	[Sort(3)]
	public void IncreaseMaxFood()
	{
		PlayerManagerPresenter.SingletonInstance.PlayerModel.IncreaseMaxFood();
	}

	[Category("Player")]
	[DisplayName("ハンドガンの弾を取得する")]
	[Sort(4)]
	public void AcquireHandGunAmmo()
	{
		PlayerCameraManager.SingletonInstance.GetGunFacade.AcquireAmmo(EnumManager.GunTYPE.HandGun);
	}

	[Category("Player")]
	[DisplayName("アサルトライフルの弾を取得する")]
	[Sort(5)]
	public void AcquireAssaultRifleAmmo()
	{
		PlayerCameraManager.SingletonInstance.GetGunFacade.AcquireAmmo(EnumManager.GunTYPE.AssaultRifle);
	}

	[Category("Player")]
	[DisplayName("ショットガンの弾を取得する")]
	[Sort(6)]
	public void AcquireShotGunAmmo()
	{
		PlayerCameraManager.SingletonInstance.GetGunFacade.AcquireAmmo(EnumManager.GunTYPE.ShotGun);
	}

	[Category("Player")]
	[DisplayName("地雷を1つ取得する")]
	[Sort(7)]
	public void AcquireMine()
	{
		PlayerManagerPresenter.SingletonInstance.AcquireMine();
	}

	#endregion
}
