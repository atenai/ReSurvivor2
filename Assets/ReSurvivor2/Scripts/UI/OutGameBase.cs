using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

/// <summary>
/// アウトゲームのベース
/// </summary>
public class OutGameBase : MonoBehaviour
{
	ShaderVariantCollection shaderVariantCollection;
	/// <summary>
	/// シェーダーロード用スライダー
	/// </summary>
	[SerializeField] protected Slider sliderShaderLoading;
	/// <summary>
	/// シーンロード用スライダー
	/// </summary>
	[SerializeField] protected Slider sliderSceneLoading;
	/// <summary>
	/// 次にロードするシーン名
	/// </summary>
	[SerializeField] protected string nextSceneName;
	bool isLoadOnce = false;

	protected void Start()
	{
		//インゲームのマネージャークラスを必ずデストロイする

		if (InGameManager.SingletonInstance != null)
		{
			Destroy(InGameManager.SingletonInstance.gameObject);
		}

		if (ChangeSceneManager.SingletonInstance != null)
		{
			Destroy(ChangeSceneManager.SingletonInstance.gameObject);
		}

		if (PlayerManagerPresenter.SingletonInstance != null)
		{
			Destroy(PlayerManagerPresenter.SingletonInstance.gameObject);
		}

		if (PlayerCameraManager.SingletonInstance != null)
		{
			Destroy(PlayerCameraManager.SingletonInstance.gameObject);
		}

		if (ScreenUIManagerPresenter.SingletonInstance != null)
		{
			Destroy(ScreenUIManagerPresenter.SingletonInstance.gameObject);
		}

		if (MissionManager.SingletonInstance != null)
		{
			Destroy(MissionManager.SingletonInstance.gameObject);
		}

		if (TimerManager.SingletonInstance != null)
		{
			Destroy(TimerManager.SingletonInstance.gameObject);
		}

		if (EffectManager.SingletonInstance != null)
		{
			Destroy(EffectManager.SingletonInstance.gameObject);
		}

		if (SoundManager.SingletonInstance != null)
		{
			Destroy(SoundManager.SingletonInstance.gameObject);
		}

		if (EnemyManager.SingletonInstance != null)
		{
			Destroy(EnemyManager.SingletonInstance.gameObject);
		}

		//シェーダーをロード
		ShaderLoad();
	}

	protected void Update()
	{
		sliderShaderLoading.value = GetShaderWarmupProgressRate();

		if (Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("XInput A"))
		{
			Load(nextSceneName);
		}
	}

	/// <summary>
	/// ロード
	/// </summary>
	/// <param name="nextSceneName">次にロードするシーン名</param>
	protected void Load(string nextSceneName)
	{
		if (isLoadOnce == false)
		{
			//シーンの読み込みはSceneLoadManagerに任せる（このシーンが破棄されても切り替えは必ず行われる）
			//シーン切り替え中などで受け付けられなかった場合は、もう一度押せるようにする
			//シェーダーロードが1と同じかそれ以上になったらシーンを切り替える
			isLoadOnce = SceneLoadManager.SingletonInstance.LoadScene(nextSceneName, SetSceneLoadingSliderValue, IsShaderWarmupCompleted);
		}
	}

	/// <summary>
	/// ロード数値をスライダーに反映する
	/// </summary>
	void SetSceneLoadingSliderValue(float progress)
	{
		if (sliderSceneLoading != null)
		{
			sliderSceneLoading.value = progress;
		}
	}

	/// <summary>
	/// シェーダーのウォームアップが終わったか？
	/// </summary>
	bool IsShaderWarmupCompleted()
	{
		return 1.0f <= GetShaderWarmupProgressRate();
	}

	/// <summary>
	/// シェーダーをロード
	/// </summary>
	void ShaderLoad()
	{
		//スライダーの値を最低にする
		sliderShaderLoading.value = float.MinValue;

		shaderVariantCollection = Resources.Load<ShaderVariantCollection>("ReSurvivor2ShaderVariants");

		if (shaderVariantCollection != null)
		{
			Debug.Log("シェーダーウォームアップ開始");
			shaderVariantCollection.WarmUp();
			Debug.Log("シェーダーウォームアップ完了");
		}
		else
		{
			Debug.LogWarning("Shader Variant Collection が見つかりません");
		}
	}

	/// <summary>
	/// シェーダーウォームアップの進捗を返す
	/// </summary>
	/// <returns>進捗(0～1)</returns>
	protected float GetShaderWarmupProgressRate()
	{
		int variantCount = shaderVariantCollection.variantCount;            // variantの総数
		int warmedUpCount = shaderVariantCollection.warmedUpVariantCount;   // Warmup済みのvariant数
		return (float)warmedUpCount / (float)variantCount;
	}
}
