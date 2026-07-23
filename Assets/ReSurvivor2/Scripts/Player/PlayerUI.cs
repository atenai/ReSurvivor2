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
	[Tooltip("地雷設置イメージ")]
	[SerializeField] Image imagePlace;
	[Tooltip("タイマーテキスト")]
	[SerializeField] TextMeshProUGUI textTimer;
	public TextMeshProUGUI TextTimer
	{
		get { return textTimer; }
		set { textTimer = value; }
	}

	void Start()
	{
		StartImageReload();
		SetMinePlacingFillAmount(0.0f);
		StartTextTimer();
		UpdateUITransform();
	}

	/// <summary>
	/// HPの初期化処理
	/// </summary>
	public void InitHP(float currentHp)
	{
		Debug.Log("InitHP");
		sliderHp.value = currentHp / HitPoint.Max_Hp;
	}

	/// <summary>
	/// スタミナの初期化処理
	/// </summary> 
	public void InitStamina(float currentStamina)
	{
		sliderStamina.value = currentStamina / Stamina.Max_Stamina;
	}

	/// <summary>
	/// アーマープレートテキストの初期化処理
	/// </summary> 
	public void StartTextArmorPlate(int currentArmorPlate)
	{
		textArmorPlate.text = currentArmorPlate.ToString();
	}

	/// <summary>
	/// 食料テキストの初期化処理
	/// </summary> 
	public void StartTextFood(int currentFood)
	{
		textFood.text = currentFood.ToString();
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
	public void SetTextMagazine(int currentMagazine)
	{
		textMagazine.text = currentMagazine.ToString();
	}

	/// <summary>
	/// 弾薬数テキストの初期化処理
	/// </summary>
	/// <param name="currentAmmo"></param>
	public void SetTextAmmo(int currentAmmo)
	{
		textAmmo.text = currentAmmo.ToString();
	}

	/// <summary>
	/// タイマーテキストの初期化処理
	/// </summary>
	void StartTextTimer()
	{
		textTimer.text = "--" + ":" + "--"; ;
	}

	public void AfterUpdate()
	{
		UpdateUITransform();
		UpdateImageReload();
	}

	/// <summary>
	/// プレイヤーキャラクターの右横にある3DのUI
	/// </summary> 
	void UpdateUITransform()
	{
		if (PlayerManager.SingletonInstance.PlayerModel.IsAim == false)
		{
			//常にキャンバスをメインカメラの方を向かせる
			canvasPlayer.transform.rotation = Camera.main.transform.rotation;
			//キャンバスの高さとカメラの高さを合わせる（これをしないとプレイヤーUIの奥行がおかしくなる）
			canvasPlayer.gameObject.GetComponent<RectTransform>().position = new Vector3(PlayerManager.SingletonInstance.transform.position.x, PlayerManager.SingletonInstance.transform.position.y + PlayerCameraManager.SingletonInstance.NormalUpPos, PlayerManager.SingletonInstance.transform.position.z);
			//SRT(スケール→トランスフォーム→ローテーション)
			const float Normal_Scale = 0.6f;
			const float Normal_RotY = 0.4f;
			const float Normal_RightPos = 125.0f;
			const float Normal_UpPos = -60.0f;
			imageBG.transform.localScale = new Vector3(Normal_Scale, Normal_Scale, 1.0f);
			imageBG.transform.localRotation = Quaternion.Euler(0.0f, Normal_RotY, 0.0f);
			imageBG.transform.localPosition = new Vector3(Normal_RightPos, Normal_UpPos, 0.0f);
		}
		else if (PlayerManager.SingletonInstance.PlayerModel.IsAim == true)
		{
			//常にキャンバスをメインカメラの方を向かせる
			canvasPlayer.transform.rotation = Camera.main.transform.rotation;
			//キャンバスの高さとカメラの高さを合わせる（これをしないとプレイヤーUIの奥行がおかしくなる）
			canvasPlayer.gameObject.GetComponent<RectTransform>().position = new Vector3(PlayerManager.SingletonInstance.transform.position.x, PlayerManager.SingletonInstance.transform.position.y + PlayerCameraManager.SingletonInstance.AimUpPos, PlayerManager.SingletonInstance.transform.position.z);
			//SRT(スケール→トランスフォーム→ローテーション)
			const float Aim_Scale = 0.2f;
			const float Aim_RotY = 0.2f;
			const float Aim_RightPos = 85.0f;
			const float Aim_UpPos = -20.0f;
			imageBG.transform.localScale = new Vector3(Aim_Scale, Aim_Scale, 1.0f);
			imageBG.transform.localRotation = Quaternion.Euler(0.0f, Aim_RotY, 0.0f);
			imageBG.transform.localPosition = new Vector3(Aim_RightPos, Aim_UpPos, 0.0f);
		}
	}

	/// <summary>
	/// リロードイメージの処理
	/// </summary>
	void UpdateImageReload()
	{
		const float Rotate_Speed = -500.0f;
		imageReload.gameObject.GetComponent<RectTransform>().transform.Rotate(0.0f, 0.0f, Rotate_Speed * Time.deltaTime);

		if (PlayerCameraManager.SingletonInstance.GetGunFacade.GetGunBase.IsReloadTimeActive == true)
		{
			if (reloadColor.a <= 1)
			{
				reloadColor.a += Time.deltaTime * 2.0f;
				imageReload.color = reloadColor;
			}
		}
		else if (PlayerCameraManager.SingletonInstance.GetGunFacade.GetGunBase.IsReloadTimeActive == false)
		{
			if (reloadColor.a >= 0)
			{
				reloadColor.a -= Time.deltaTime * 2.0f;
				imageReload.color = reloadColor;
			}
		}
	}

	/// <summary>
	/// 地雷設置イメージのフィルアマウントを設定する
	/// </summary>
	/// <param name="amount"></param>
	public void SetMinePlacingFillAmount(float amount)
	{
		imagePlace.fillAmount = amount;
	}

	/// <summary>
	/// アーマープレートテキストの処理
	/// </summary>
	/// <param name="currentArmorPlate"></param>
	public void SetTextArmorPlate(int currentArmorPlate)
	{
		textArmorPlate.text = currentArmorPlate.ToString();
	}

	/// <summary>
	/// 食料テキストの処理
	/// </summary>
	/// <param name="currentFood"></param>
	public void SetTextFood(int currentFood)
	{
		textFood.text = currentFood.ToString();
	}
}
