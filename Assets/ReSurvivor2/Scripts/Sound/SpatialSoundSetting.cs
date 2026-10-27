using UnityEngine;

/// <summary>
/// 効果音を3D（鳴った位置から聞こえて、遠いほど小さくなる音）で鳴らす時の設定
/// 銃の効果音のオブジェクトプールはプレイヤーとエネミーで同じものを使うので、
/// エネミーが鳴らす時は Apply3D で3Dにして、プレイヤーが鳴らす時は Apply2D で2D（今まで通り）に戻す
/// </summary>
[System.Serializable]
public class SpatialSoundSetting
{
	[Tooltip("2Dの時の3Dの度合い（どこで鳴っても耳元で同じ大きさで聞こえる）")]
	const float SpatialBlend2D = 0.0f;
	[Tooltip("3Dの時の3Dの度合い（鳴った位置から聞こえる）")]
	const float SpatialBlend3D = 1.0f;
	[Tooltip("ドップラー効果の強さ（0 = 無し。カメラが回った時などに音の高さが変わらないようにする）")]
	const float DopplerLevel = 0.0f;

	[Tooltip("この距離（m）までは音量が下がらない（大きな音ほど長くする）")]
	[SerializeField] float minDistance;
	[Tooltip("この距離（m）より遠くでは、音量がそれ以上下がらない")]
	[SerializeField] float maxDistance;

	/// <summary>
	/// コンストラクタ（音の種類ごとに、音量が下がり始める距離と下がらなくなる距離を決める）
	/// </summary>
	public SpatialSoundSetting(float minDistance, float maxDistance)
	{
		this.minDistance = minDistance;
		this.maxDistance = maxDistance;
	}

	/// <summary>
	/// AudioSource をこの設定の3Dにする
	/// 音量は minDistance までは下がらず、そこから距離に反比例して下がる（距離が2倍になると音量が半分）
	/// </summary>
	public void Apply3D(AudioSource audioSource)
	{
		audioSource.spatialBlend = SpatialBlend3D;
		audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
		audioSource.minDistance = minDistance;
		audioSource.maxDistance = maxDistance;
		audioSource.dopplerLevel = DopplerLevel;
	}

	/// <summary>
	/// AudioSource を2Dに戻す（エネミーが3Dで鳴らした後のオブジェクトを、プレイヤーが使う時のため）
	/// </summary>
	public static void Apply2D(AudioSource audioSource)
	{
		audioSource.spatialBlend = SpatialBlend2D;
	}
}
