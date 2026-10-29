using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// プレイヤー3DUIビュー
/// MVPパターンのView担当
/// </summary>
public class PlayerUIView : MonoBehaviour
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
	[Tooltip("ハンドガンに持ち替えられることを表すアイコン（ハンドガンのアイコンと矢印のアイコン）を、まとめて表示・非表示にするキャンバスグループ")]
	[SerializeField] CanvasGroup canvasGroupSideArm;
	[Tooltip("ハンドガンに持ち替えられることを表す、ハンドガンのアイコンを指す矢印のアイコン")]
	[SerializeField] Image imageSideArmArrow;
	[Tooltip("矢印のアイコンが、ハンドガンのアイコンの方へ動き終わる位置（プレハブで置いた位置）")]
	Vector2 sideArmArrowEndPosition;
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
	[Tooltip("マガジン弾数テキスト・弾薬数テキスト・タイマーテキストの、弾・残り時間が十分ある時の色")]
	[SerializeField] Color textNormalColor = Color.white;
	[Tooltip("マガジン弾数テキスト・弾薬数テキスト・タイマーテキストの、弾・残り時間が少ない時の色")]
	[SerializeField] Color textLowColor = Color.red;

	void Start()
	{
		StartImageReload();
		StartImageSideArm();
		SetMinePlacingFillAmount(0.0f);
		StartTextTimer();
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
	/// ハンドガンに持ち替えられることを表すアイコンの初期化処理（最初は非表示にする）
	/// </summary>
	void StartImageSideArm()
	{
		const float Hidden_Alpha = 0.0f;
		canvasGroupSideArm.alpha = Hidden_Alpha;
		sideArmArrowEndPosition = imageSideArmArrow.rectTransform.anchoredPosition;
	}

	/// <summary>
	/// 残弾テキストの初期化処理
	/// </summary>
	/// <param name="currentMagazine">マガジンの弾数</param>
	/// <param name="isMagazineLow">マガジンの弾が少ないか？（少ない時は赤くする）</param>
	public void SetTextMagazine(int currentMagazine, bool isMagazineLow)
	{
		textMagazine.text = currentMagazine.ToString();
		textMagazine.color = isMagazineLow == true ? textLowColor : textNormalColor;
	}

	/// <summary>
	/// 弾薬数テキストの初期化処理
	/// </summary>
	/// <param name="currentAmmo">予備の弾数</param>
	/// <param name="isAmmoLow">予備の弾が少ないか？（少ない時は赤くする）</param>
	public void SetTextAmmo(int currentAmmo, bool isAmmoLow)
	{
		textAmmo.text = currentAmmo.ToString();
		textAmmo.color = isAmmoLow == true ? textLowColor : textNormalColor;
	}

	/// <summary>
	/// タイマーテキストの初期化処理
	/// </summary>
	void StartTextTimer()
	{
		textTimer.text = "--" + ":" + "--"; ;
	}

	/// <summary>
	/// タイマーテキストの色の処理
	/// </summary>
	/// <param name="isTimeLow">ミッションの残り時間が少ないか？（少ない時は赤くする）</param>
	public void SetTextTimerColor(bool isTimeLow)
	{
		textTimer.color = isTimeLow == true ? textLowColor : textNormalColor;
	}

	/// <summary>
	/// プレイヤーキャラクターの右横にある3DのUI
	/// </summary> 
	public void UpdateUITransform(bool isAim)
	{
		if (isAim == false)
		{
			//常にキャンバスをメインカメラの方を向かせる
			canvasPlayer.transform.rotation = Camera.main.transform.rotation;
			//キャンバスの高さとカメラの高さを合わせる（これをしないとプレイヤーUIの奥行がおかしくなる）
			canvasPlayer.gameObject.GetComponent<RectTransform>().position = new Vector3(PlayerManagerPresenter.SingletonInstance.transform.position.x, PlayerManagerPresenter.SingletonInstance.transform.position.y + PlayerCameraManager.SingletonInstance.NormalUpPos, PlayerManagerPresenter.SingletonInstance.transform.position.z);
			//SRT(スケール→トランスフォーム→ローテーション)
			const float Normal_Scale = 0.6f;
			const float Normal_RotY = 0.4f;
			const float Normal_RightPos = 125.0f;
			const float Normal_UpPos = -60.0f;
			imageBG.transform.localScale = new Vector3(Normal_Scale, Normal_Scale, 1.0f);
			imageBG.transform.localRotation = Quaternion.Euler(0.0f, Normal_RotY, 0.0f);
			imageBG.transform.localPosition = new Vector3(Normal_RightPos, Normal_UpPos, 0.0f);
		}
		else if (isAim == true)
		{
			//常にキャンバスをメインカメラの方を向かせる
			canvasPlayer.transform.rotation = Camera.main.transform.rotation;
			//キャンバスの高さとカメラの高さを合わせる（これをしないとプレイヤーUIの奥行がおかしくなる）
			canvasPlayer.gameObject.GetComponent<RectTransform>().position = new Vector3(PlayerManagerPresenter.SingletonInstance.transform.position.x, PlayerManagerPresenter.SingletonInstance.transform.position.y + PlayerCameraManager.SingletonInstance.AimUpPos, PlayerManagerPresenter.SingletonInstance.transform.position.z);
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
	public void UpdateImageReload(bool isReloadTimeActive)
	{
		const float Rotate_Speed = -500.0f;
		imageReload.gameObject.GetComponent<RectTransform>().transform.Rotate(0.0f, 0.0f, Rotate_Speed * Time.deltaTime);

		if (isReloadTimeActive == true)
		{
			if (reloadColor.a <= 1)
			{
				reloadColor.a += Time.deltaTime * 2.0f;
				imageReload.color = reloadColor;
			}
		}
		else if (isReloadTimeActive == false)
		{
			if (reloadColor.a >= 0)
			{
				reloadColor.a -= Time.deltaTime * 2.0f;
				imageReload.color = reloadColor;
			}
		}
	}

	/// <summary>
	/// ハンドガンに持ち替えられることを表すアイコンの処理
	/// （持ち替えられる間は、ハンドガンのアイコンと矢印のアイコンを表示して、矢印をハンドガンのアイコンの方へ繰り返し動かす）
	/// </summary>
	/// <param name="isSideArmDrawWindowOpen">ハンドガンに持ち替えられる間か？</param>
	public void UpdateImageSideArm(bool isSideArmDrawWindowOpen)
	{
		//持ち替えられる間だけ、すばやくフェードインして表示する
		const float Shown_Alpha = 1.0f;
		const float Hidden_Alpha = 0.0f;
		const float Fade_Speed = 8.0f;
		float targetAlpha = isSideArmDrawWindowOpen == true ? Shown_Alpha : Hidden_Alpha;
		canvasGroupSideArm.alpha = Mathf.MoveTowards(canvasGroupSideArm.alpha, targetAlpha, Fade_Speed * Time.deltaTime);

		//矢印は、左からハンドガンのアイコンの方へ動かしながら、動きの始めと終わりで薄くする
		const float Arrow_Move_Time = 0.5f;
		const float Arrow_Move_Progress_Max = 1.0f;
		const float Arrow_Move_Distance = 10.0f;
		float arrowMoveProgress = Mathf.Repeat(Time.time / Arrow_Move_Time, Arrow_Move_Progress_Max);
		imageSideArmArrow.rectTransform.anchoredPosition = sideArmArrowEndPosition + Vector2.left * Arrow_Move_Distance * (Arrow_Move_Progress_Max - arrowMoveProgress);
		Color arrowColor = imageSideArmArrow.color;
		arrowColor.a = Mathf.Sin(arrowMoveProgress * Mathf.PI);
		imageSideArmArrow.color = arrowColor;
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
