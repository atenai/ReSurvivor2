using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 1.Universal Renderer Data（URP Asset_my_Renderer）に追加するレンダラー機能 
/// </summary>
/// <remarks>
/// 1.ScriptableRendererFeature(公式クラス)を継承したUniversal Renderer Data（URP Asset_my_Renderer）に追加する
/// ↓ 
/// 2.ScriptableRenderPass(公式クラス)を継承したレンダーパスクラスをこのクラスで使用するために作成する 
/// ↓
/// 3.シェーダーを作成してこのクラスのインスペクターにアタッチする 
/// </remarks> 
[DisallowMultipleRendererFeature]
public sealed class GrayScaleBlitterRendererFeature : ScriptableRendererFeature
{
	[Tooltip("シェーダーのセット")]
	[SerializeField] private Shader _shader;
	[Tooltip("レンダーパスのイベント設定(レンダーパスイベントとは、レンダリングパイプライン内でレンダーパスが実行されるタイミングを指定するための設定です。)")]
	[SerializeField] private RenderPassEvent _renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
	/// <summary>
	/// レンダーパス
	/// </summary>
	/// <remarks>
	/// GrayScaleBlitterRenderPassはScriptableRenderPass(公式クラス)を継承した自作クラスです。
	/// グレースケール効果を適用するためのレンダーパスを定義しています。
	/// このクラスは、指定されたシェーダーを使用して、カメラのレンダリングターゲットに対してグレースケール効果を適用します。
	/// </remarks> 
	private GrayScaleBlitterRenderPass _renderPass;

	/// <summary>
	/// シェーダーとレンダーパスの初期化（Unity公式の関数）（絶対必須！）
	/// </summary>
	public override void Create()
	{
		// シェーダーが設定されていない場合はシェーダーを探す
		if (_shader == null)
		{
			// 指定パスのシェーダーを見つける
			_shader = Shader.Find("Hidden/GrayScaleBlitterVertexFragment");
		}

		// シェーダーが見つからなかった場合はレンダーパスを作成せず終了
		if (_shader == null)
		{
			_renderPass = null;
			return;
		}

		// レンダーパスの作成
		_renderPass = new GrayScaleBlitterRenderPass(_shader, _renderPassEvent);
	}

	/// <summary>
	/// レンダーパスの追加（Unity公式の関数）（絶対必須！）
	/// </summary>
	/// <param name="renderer"></param>
	/// <param name="renderingData"></param>
	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass(_renderPass);
		// ConfigureInputはAddRenderPasses内で呼ぶこと
		_renderPass.ConfigureInput(ScriptableRenderPassInput.Color);
	}

	/// <summary>
	/// レンダーパスのセットアップ（Unity公式の関数）
	/// </summary>
	/// <param name="renderer"></param>
	/// <param name="renderingData"></param>
	public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
	{
		_renderPass.SetRenderTarget(renderer.cameraColorTargetHandle);
	}

	/// <summary>
	/// レンダーパスの破棄（Unity公式の関数）（絶対必須！）
	/// </summary>
	/// <param name="disposing"></param>
	protected override void Dispose(bool disposing)
	{
		_renderPass.Destroy();
		_renderPass = null;
		base.Dispose(disposing);
	}
}