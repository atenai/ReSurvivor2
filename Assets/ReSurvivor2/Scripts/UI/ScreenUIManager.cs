using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// スクリーンUIを管理するマネージャークラス
/// </summary>
public class ScreenUIManager : MonoBehaviour
{
	/// <summary> シングルトンで作成（ゲーム中に１つのみにする）</summary>
	static ScreenUIManager singletonInstance = null;
	/// <summary>シングルトンのプロパティ</summary>
	public static ScreenUIManager SingletonInstance => singletonInstance;

	[SerializeField] ScreenUIView screenUIView;

	ScreenUIPresenter screenUIPresenter;
	public ScreenUIPresenter ScreenUIPresenter => screenUIPresenter;

	void Awake()
	{
		//staticな変数instanceはメモリ領域は確保されていますが、初回では中身が入っていないので、中身を入れます。
		if (singletonInstance == null)
		{
			singletonInstance = this;//thisというのは自分自身のインスタンスという意味になります。この場合、Playerのインスタンスという意味になります。
			DontDestroyOnLoad(this.gameObject);//シーンを切り替えた時に破棄しない
		}
		else
		{
			Destroy(this.gameObject);//中身がすでに入っていた場合、自身のインスタンスがくっついているゲームオブジェクトを破棄します。
		}
	}

	void Start()
	{
		screenUIPresenter = new ScreenUIPresenter(screenUIView);
		screenUIPresenter.Init();
	}

	public void BeforeUpdate()
	{
		screenUIPresenter.UpdateComputerMenuSystem();
		if (screenUIPresenter.SkipYesNoDialogInput == true)
		{
			// 表示直後のフレームは入力を消費して無視する
			screenUIPresenter.SkipYesNoDialogInput = false;
		}
		else
		{
			screenUIPresenter.UpdateYesNoDialog();
		}

		screenUIPresenter.UpdatePauseMenuSystem();
	}

	public void AfterUpdate()
	{
		screenUIView.Crosshair(PlayerManager.SingletonInstance.IsAim, PlayerCameraManager.SingletonInstance.IsTargetHit);
		screenUIPresenter.UpdateHitReticule();
		screenUIPresenter.UpdateDamageEffect();
		screenUIPresenter.UpdateHpHealEffect();
		screenUIPresenter.UpdateStaminaHealEffect();

		if (Input.GetKeyDown(KeyCode.M) || Input.GetButtonDown("XInput Pause"))
		{
			screenUIView.MapUI.EnableMap();
		}
	}
}
