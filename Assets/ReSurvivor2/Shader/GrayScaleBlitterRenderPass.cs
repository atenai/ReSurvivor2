using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 2.ScriptableRenderPassを継承した自作クラス
/// </summary>
public sealed class GrayScaleBlitterRenderPass : ScriptableRenderPass
{
	private const string RENDER_PASS_NAME = nameof(GrayScaleBlitterRenderPass);
	private readonly Material _material;
	private RTHandle _cameraColorTarget;

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="shader"></param>
	/// <param name="passEvent"></param>
	public GrayScaleBlitterRenderPass(Shader shader, RenderPassEvent passEvent)
	{
		if (shader == null) return;

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
		ConfigureTarget(_cameraColorTarget);
	}

	/// <summary>
	/// レンダーパスの実行（Unity公式の関数）（絶対必須！）
	/// </summary>
	/// <param name="context"></param>
	/// <param name="renderingData"></param>
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (_material == null) return;

		var cmd = CommandBufferPool.Get(RENDER_PASS_NAME);
		Blitter.BlitCameraTexture(cmd, _cameraColorTarget, _cameraColorTarget, _material, 0);

		context.ExecuteCommandBuffer(cmd);
		CommandBufferPool.Release(cmd);
	}

	/// <summary>
	/// リソースの解放（Unity公式の関数）（絶対必須！）
	/// </summary>
	public void Destroy()
	{
		CoreUtils.Destroy(_material);
	}
}
