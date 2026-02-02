using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// プレイヤー3DUI
/// </summary>
public class PlayerUI : MonoBehaviour
{
	[Header("UI")]
	[Tooltip("プレイヤーのキャンバス")]
	[SerializeField] Canvas canvasPlayer;
	[Tooltip("バックグラウンドイメージ")]
	[SerializeField] Image imageBG;
	[Tooltip("プレイヤーUIのY軸の傾き")]
	float playerUIRotY = 0.2f;
	[Tooltip("HPバー")]
	[SerializeField] Slider sliderHp;
	public Slider SliderHp
	{
		get { return sliderHp; }
		set { sliderHp = value; }
	}
	[Tooltip("スタミナバー")]
	[SerializeField] Slider sliderStamina;
	public Slider SliderStamina
	{
		get { return sliderStamina; }
		set { sliderStamina = value; }
	}
	[Tooltip("アーマープレートテキスト")]
	[SerializeField] TextMeshProUGUI textArmorPlate;
	public TextMeshProUGUI TextArmorPlate
	{
		get { return textArmorPlate; }
		set { textArmorPlate = value; }
	}
	[Tooltip("食料テキスト")]
	[SerializeField] TextMeshProUGUI textFood;
	public TextMeshProUGUI TextFood
	{
		get { return textFood; }
		set { textFood = value; }
	}
	[Tooltip("リロード画像")]
	[SerializeField] Image imageReload;
	Color reloadColor = new Color(255.0f, 255.0f, 255.0f, 0.0f);
	[Tooltip("マガジン弾数テキスト")]
	[SerializeField] TextMeshProUGUI textMagazine;
	[Tooltip("弾薬数テキスト")]
	[SerializeField] TextMeshProUGUI textAmmo;
	[Tooltip("地雷テキスト")]
	[SerializeField] TextMeshProUGUI textMine;
	public TextMeshProUGUI TextMine
	{
		get { return textMine; }
		set { textMine = value; }
	}
	[Tooltip("タイマーテキスト")]
	[SerializeField] TextMeshProUGUI textTimer;
	public TextMeshProUGUI TextTimer
	{
		get { return textTimer; }
		set { textTimer = value; }
	}

	void Start()
	{
		InitHP();
		InitStamina();
		StartTextArmorPlate();
		StartTextFood();
		StartImageReload();
		StartTextMagazine();
	}

	/// <summary>
	/// HPの初期化処理
	/// </summary>
	void InitHP()
	{
		sliderHp.value = (float)Player.SingletonInstance.CurrentHp / (float)Player.SingletonInstance.MaxHp;
	}

	/// <summary>
	/// スタミナの初期化処理
	/// </summary> 
	void InitStamina()
	{
		sliderStamina.value = (float)Player.SingletonInstance.CurrentStamina / (float)Player.SingletonInstance.MaxStamina;
	}

	/// <summary>
	/// アーマープレートテキストの初期化処理
	/// </summary> 
	void StartTextArmorPlate()
	{
		textArmorPlate.text = Player.SingletonInstance.CurrentArmorPlate.ToString();
	}

	/// <summary>
	/// 食料テキストの初期化処理
	/// </summary> 
	void StartTextFood()
	{
		textFood.text = Player.SingletonInstance.CurrentFood.ToString();
	}

	/// <summary>
	/// リロードイメージの初期化処理
	/// </summary>
	void StartImageReload()
	{
		imageReload.color = reloadColor;
	}

	/// <summary>
	/// 残弾テキストの初期化処理
	/// </summary>
	void StartTextMagazine()
	{
		textMagazine.text = PlayerCamera.SingletonInstance.GetGunFacade.GetGunBase.CurrentMagazine.ToString();
		textAmmo.text = PlayerCamera.SingletonInstance.GetGunFacade.GetGunBase.CurrentAmmo.ToString();
	}

	void Update()
	{
		if (ScreenUI.SingletonInstance.IsPause == true)
		{
			return;
		}

		if (ScreenUI.SingletonInstance.IsComputerMenuActive == true)
		{
			return;
		}

		//↑ロード中に動かせる処理
		if (InGameManager.SingletonInstance.IsGamePlayReady == false)
		{
			return;
		}
		//↓ロード中に動かせない処理

		UI();
		UpdateImageReload();
		UpdateTextMagazine();
	}

	/// <summary>
	/// プレイヤーキャラクターの右横にある3DのUI
	/// </summary> 
	void UI()
	{
		if (Player.SingletonInstance.IsAim == false)
		{
			//常にキャンバスをメインカメラの方を向かせる
			canvasPlayer.transform.rotation = Camera.main.transform.rotation;
			//キャンバスの高さとカメラの高さを合わせる（これをしないとプレイヤーUIの奥行がおかしくなる）
			canvasPlayer.gameObject.GetComponent<RectTransform>().position = new Vector3(Player.SingletonInstance.transform.position.x, Player.SingletonInstance.transform.position.y + PlayerCamera.SingletonInstance.NormalUpPos, Player.SingletonInstance.transform.position.z);
			//SRT(スケール→トランスフォーム→ローテーション)
			//HP、スタミナ、弾薬、タイマーUI
			imageBG.transform.localScale = new Vector3(1.0f, 1.0f, 1f);
			imageBG.transform.localRotation = Quaternion.Euler(0.0f, playerUIRotY, 0.0f);
			imageBG.transform.localPosition = new Vector3(150.0f, -100.0f, 0.0f);
		}
		else if (Player.SingletonInstance.IsAim == true)
		{
			//常にキャンバスをメインカメラの方を向かせる
			canvasPlayer.transform.rotation = Camera.main.transform.rotation;
			//キャンバスの高さとカメラの高さを合わせる（これをしないとプレイヤーUIの奥行がおかしくなる）
			canvasPlayer.gameObject.GetComponent<RectTransform>().position = new Vector3(Player.SingletonInstance.transform.position.x, Player.SingletonInstance.transform.position.y + PlayerCamera.SingletonInstance.AimUpPos, Player.SingletonInstance.transform.position.z);
			//SRT(スケール→トランスフォーム→ローテーション)
			//HP、スタミナ、弾薬、タイマーUI
			imageBG.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
			imageBG.transform.localRotation = Quaternion.Euler(0.0f, playerUIRotY, 0.0f);
			imageBG.transform.localPosition = new Vector3(85.0f, -20.0f, 0.0f);
		}
	}

	/// <summary>
	/// リロードイメージの処理
	/// </summary>
	void UpdateImageReload()
	{
		const float Rotate_Speed = -500.0f;
		imageReload.gameObject.GetComponent<RectTransform>().transform.Rotate(0.0f, 0.0f, Rotate_Speed * Time.deltaTime);

		if (PlayerCamera.SingletonInstance.GetGunFacade.GetGunBase.IsReloadTimeActive == true)
		{
			if (reloadColor.a <= 1)
			{
				reloadColor.a += Time.deltaTime * 2.0f;
				imageReload.color = reloadColor;
			}
		}
		else if (PlayerCamera.SingletonInstance.GetGunFacade.GetGunBase.IsReloadTimeActive == false)
		{
			if (reloadColor.a >= 0)
			{
				reloadColor.a -= Time.deltaTime * 2.0f;
				imageReload.color = reloadColor;
			}
		}
	}

	/// <summary>
	/// 残弾テキスト
	/// </summary>
	void UpdateTextMagazine()
	{
		textMagazine.text = PlayerCamera.SingletonInstance.GetGunFacade.GetGunBase.CurrentMagazine.ToString();
		textAmmo.text = PlayerCamera.SingletonInstance.GetGunFacade.GetGunBase.CurrentAmmo.ToString();
	}
}
