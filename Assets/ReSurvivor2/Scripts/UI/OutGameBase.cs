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

	[Tooltip("allowSceneActivation が false の時、読み込みが終わると progress はこの値で止まる（ここから先はシーンを切り替えると進む）")]
	const float ActivationReadyProgress = 0.9f;
	[Tooltip("スライダーを満タンにする時の進捗")]
	const float FullProgress = 1.0f;

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

		if (SceneLoadManager.SingletonInstance != null)
		{
			Destroy(SceneLoadManager.SingletonInstance.gameObject);
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
			isLoadOnce = true;
			StartCoroutine(SceneLoad(nextSceneName));
		}
	}

	/// <summary>
	/// シーンをロードする
	/// （SceneLoadManager はインゲームのマネージャーなので Start で破棄している。アウトゲームではここでロードする）
	/// </summary>
	/// <param name="nextSceneName">次にロードするシーン名</param>
	IEnumerator SceneLoad(string nextSceneName)
	{
		//スライダーの値を最低にする
		SetSceneLoadingSliderValue(float.MinValue);

		//シーンをロード
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(nextSceneName);
		//シーンが勝手に切り替わらないようにする
		asyncOperation.allowSceneActivation = false;

		//ロード数値が0.9になる かつ シェーダーロードが終わる まで待つ（必ず両方の条件が揃ってから切り替える）
		while (asyncOperation.progress < ActivationReadyProgress || IsShaderWarmupCompleted() == false)
		{
			//ロード数値をスライダーに反映
			SetSceneLoadingSliderValue(asyncOperation.progress);
			yield return null;
		}

		//スライダーを満タンにして1フレーム表示する
		//※WaitForEndOfFrameはエディターでGameビューが描画されていないと再開しないため使わない
		SetSceneLoadingSliderValue(FullProgress);
		yield return null;

		//シーンを切り替える
		asyncOperation.allowSceneActivation = true;
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
