using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 1.Universal Renderer Data（URP Asset_my_Renderer）に追加するレンダラー機能 
///　参考サイト：https://note.com/hikohiro/n/ncebd57610a73 
/// </summary>
/// <remarks>
/// 1.ScriptableRendererFeature(公式クラス)を継承したUniversal Renderer Data（URP Asset_my_Renderer）に追加する
/// ↓ 
/// 2.ScriptableRenderPass(公式クラス)を継承したレンダーパスクラスをこのクラスで使用するために作成する 
/// ↓
/// 3.シェーダーを作成してこのクラスのインスペクターにアタッチする 
/// </remarks> 
[DisallowMultipleRendererFeature]
public class BlurRendererFeature : ScriptableRendererFeature
{
	[Tooltip("シェーダーのセット")]
	[SerializeField] private Shader _shader;
	[Tooltip("レンダーパスのイベント設定(レンダーパスイベントとは、レンダリングパイプライン内でレンダーパスが実行されるタイミングを指定するための設定です。)")]
	[SerializeField] private RenderPassEvent _renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
	/// <summary>
	/// レンダーパス
	/// </summary>
	/// <remarks>
	/// BlurRenderPassはScriptableRenderPass(公式クラス)を継承した自作クラスです。
	/// ブラー効果を適用するためのレンダーパスを定義しています。
	/// このクラスは、指定されたシェーダーを使用して、カメラのレンダリングターゲットに対してブラー効果を適用します。
	/// </remarks> 
	private BlurRenderPass _renderPass;

	/// <summary>
	/// シェーダーとレンダーパスの初期化（Unity公式の関数）（絶対必須！）
	/// </summary>
	public override void Create()
	{
		// シェーダーが設定されていない場合はシェーダーを探す
		if (_shader == null)
		{
			// 指定パスのシェーダーを見つける
			_shader = Shader.Find("CustomEffects/Blur");
		}

		// シェーダーが見つからなかった場合はレンダーパスを作成せず終了
		if (_shader == null)
		{
			_renderPass = null;
			return;
		}

		// レンダーパスの作成
		_renderPass = new BlurRenderPass(_shader)
		{
			renderPassEvent = _renderPassEvent
		};
	}

	/// <summary>
	/// レンダーパスの追加（Unity公式の関数）（絶対必須！）
	/// </summary>
	/// <param name="renderer"></param>
	/// <param name="renderingData"></param>
	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (renderingData.cameraData.cameraType != CameraType.Game) return;

		if (_renderPass.UpdateBlurSettings() == false) return;

		renderer.EnqueuePass(_renderPass);
	}

	/// <summary>
	/// レンダーパスの破棄（Unity公式の関数）（絶対必須！）
	/// </summary>
	/// <param name="disposing"></param>
	protected override void Dispose(bool disposing)
	{
		_renderPass.Dispose();
	}
}

[Serializable]
public class BlurSettings
{
	[Range(0, 0.4f)] public float horizontalBlur;
	[Range(0, 0.4f)] public float verticalBlur;
}