using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ガンビュープレゼンター（銃の知らせを受け取って、銃のビューに見た目と音を出させる）
/// MVPパターンのPresenter担当
/// ※銃（GunModelFacade）は銃のビュー（GunViewFacade）を知らず、銃のビューも銃を知らない。ここで両方をつなぐ
/// </summary>
public class GunViewPresenter
{
	[Tooltip("知らせを受け取っている銃（つないでいない時はnull）")]
	GunModelFacade gunModelFacade;
	[Tooltip("見た目と音を出させる銃のビュー（つないでいない時はnull）")]
	GunViewFacade gunViewFacade;

	/// <summary>
	/// 銃の知らせを、銃のビューにつなぐ（つないだ時に、今持っている銃を手に持つ見せ方にする）
	/// </summary>
	/// <param name="gunModelFacade">ガンモデルファサード</param>
	/// <param name="gunViewFacade">ガンビューファサード</param>
	public void Bind(GunModelFacade gunModelFacade, GunViewFacade gunViewFacade)
	{
		this.gunModelFacade = gunModelFacade;
		this.gunViewFacade = gunViewFacade;
		gunModelFacade.OnGunTYPEChanged += gunViewFacade.SwitchWeaponModel;
		gunModelFacade.OnGunFired += gunViewFacade.Fire;
		gunModelFacade.OnGunReloadStarted += gunViewFacade.ReloadStartSE;
		gunModelFacade.OnShotGunShellInserted += gunViewFacade.ShotGunView.ShotGunShellInsertSE;
		gunModelFacade.OnShotGunChambered += gunViewFacade.ShotGunView.ShotGunReloadSE;
		gunViewFacade.SwitchWeaponModel(gunModelFacade.GetGunTYPE);
	}

	/// <summary>
	/// 銃の知らせと、銃のビューのつなぎを外す（ビューが破棄される時に呼ぶ）
	/// </summary>
	public void Unbind()
	{
		if (gunModelFacade == null)
		{
			return;
		}

		gunModelFacade.OnGunTYPEChanged -= gunViewFacade.SwitchWeaponModel;
		gunModelFacade.OnGunFired -= gunViewFacade.Fire;
		gunModelFacade.OnGunReloadStarted -= gunViewFacade.ReloadStartSE;
		gunModelFacade.OnShotGunShellInserted -= gunViewFacade.ShotGunView.ShotGunShellInsertSE;
		gunModelFacade.OnShotGunChambered -= gunViewFacade.ShotGunView.ShotGunReloadSE;
		gunModelFacade = null;
		gunViewFacade = null;
	}
}
