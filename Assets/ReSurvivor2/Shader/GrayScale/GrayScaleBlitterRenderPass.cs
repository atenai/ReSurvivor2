using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 2.ScriptableRenderPassを継承した自作クラス
/// </summary>
public sealed class GrayScaleBlitterRenderPass : ScriptableRenderPass
{
	private const string PASS_NAME = nameof(GrayScaleBlitterRenderPass);

	private readonly Material _material;

	private RTHandle _cameraColorTarget;
	private RTHandle _tempColorTexture;

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="shader"></param>
	/// <param name="passEvent"></param>
	public GrayScaleBlitterRenderPass(Shader shader, RenderPassEvent passEvent)
	{
		if (shader == null)
		{
			return;
		}

		_material = CoreUtils.CreateEngineMaterial(shader);
		renderPassEvent = passEvent;
	}

	public void SetRenderTarget(RTHandle colorHandle)
	{
		_cameraColorTarget = colorHandle;
	}

	/// <summary>
	/// カメラセットアップ時の処理（Unity公式の関数）
	/// </summary>
	/// <param name="cmd"></param>
	/// <param name="renderingData"></param>
	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		// カメラのRenderTextureDescriptorを取得
		RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
		desc.depthBufferBits = 0;

		// Temp RTを確保（URP推奨のRTHandle管理）
		RenderingUtils.ReAllocateIfNeeded(
			ref _tempColorTexture,
			desc,
			name: "_GrayScaleTempColorTexture"
		);

		// このPassの出力先（最終的にはカメラカラーへ戻す）
		ConfigureTarget(_cameraColorTarget);
	}

	/// <summary>
	/// レンダーパスの実行（Unity公式の関数）（絶対必須！）
	/// </summary>
	/// <param name="context"></param>
	/// <param name="renderingData"></param>
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (_material == null)
		{
			return;
		}

		CommandBuffer cmd = CommandBufferPool.Get(PASS_NAME);

		// ① カメラカラー → Temp（シェーダー適用）
		Blitter.BlitCameraTexture(cmd, _cameraColorTarget, _tempColorTexture, _material, 0);

		// ② Temp → カメラカラー（結果を書き戻す）
		Blitter.BlitCameraTexture(cmd, _tempColorTexture, _cameraColorTarget);

		context.ExecuteCommandBuffer(cmd);
		CommandBufferPool.Release(cmd);
	}

	public override void OnCameraCleanup(CommandBuffer cmd)
	{
		// 毎フレーム解放はしない（RTHandle管理に任せる）
		// どうしても解放したいなら Destroy() でまとめてやる
	}

	/// <summary>
	/// リソースの解放（Unity公式の関数）（絶対必須！）
	/// </summary>
	public void Destroy()
	{
		if (_tempColorTexture != null)
		{
			_tempColorTexture.Release();
			_tempColorTexture = null;
		}

		CoreUtils.Destroy(_material);
	}
}
