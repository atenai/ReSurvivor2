using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Experimental.Rendering;

/// <summary>
/// アウトゲームのマネージャー
/// </summary>
public class OutGameManagerBase : MonoBehaviour
{
	ShaderVariantCollection shaderVariantCollection;
	[SerializeField] protected Slider sliderShaderLoading;
	[SerializeField] protected string sceneName;
	[SerializeField] protected Slider sliderSceneLoading;
	bool isLoadOnce = false;

	protected void Start()
	{
		if (InGameManager.SingletonInstance != null)
		{
			Destroy(InGameManager.SingletonInstance.gameObject);
		}

		if (Player.SingletonInstance != null)
		{
			Destroy(Player.SingletonInstance.gameObject);
		}

		if (PlayerCamera.SingletonInstance != null)
		{
			Destroy(PlayerCamera.SingletonInstance.gameObject);
		}

		if (UI.SingletonInstance != null)
		{
			Destroy(UI.SingletonInstance.gameObject);
		}

		//シェーダーをロード
		ShaderLoad();
	}

	protected void Update()
	{
		sliderShaderLoading.value = GetShaderWarmupProgressRate();

		if (Input.GetKeyDown(KeyCode.Return))
		{
			Load(sceneName);
		}
	}

	/// <summary>
	/// ロード
	/// </summary> 
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
	/// </summary>
	IEnumerator SceneLoad(string nextSceneName)
	{
		//スライダーの値を最低にする
		sliderSceneLoading.value = float.MinValue;

		//シーンをロード
		AsyncOperation async = SceneManager.LoadSceneAsync(nextSceneName);
		//シーンが勝手に切り替わらないようにする
		async.allowSceneActivation = false;
		//シーンをロードするまでのループ処理
		while (async.isDone == false)
		{
			//ロード数値をスライダーに反映
			sliderSceneLoading.value = async.progress;

			//ロード数値が0.9より同じかそれ以上大きくなったら かつ シェーダーロードが1と同じかそれ以上になったら 中身を実行する
			if (0.9f <= async.progress && 1.0f <= GetShaderWarmupProgressRate())
			{
				//スライダーの値を最大にする
				sliderSceneLoading.value = float.MaxValue;

				//フレームのラストまで待つ
				yield return new WaitForEndOfFrame();

				//シーンを切り替える
				async.allowSceneActivation = true;
			}

			//1フレーム待つ
			yield return null;
		}
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
