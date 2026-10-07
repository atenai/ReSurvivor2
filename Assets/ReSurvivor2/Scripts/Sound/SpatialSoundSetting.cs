using UnityEngine;

/// <summary>
/// 銃の効果音を3D（鳴った位置から聞こえて、遠いほど小さくなる音）で鳴らす時の設定
/// 効果音のオブジェクトプールはプレイヤーとエネミーで同じものを使うので、鳴らすたびにどちらかの設定を Apply する
/// （プレイヤー用とエネミー用で、音量が下がり始める距離などを変えられるようにしている）
/// </summary>
[System.Serializable]
public class SpatialSoundSetting
{
	[Tooltip("3Dの度合い（0 = 2D：どこで鳴っても耳元で同じ大きさ、1 = 3D：鳴った位置から聞こえる）")]
	[Range(0.0f, 1.0f)]
	[SerializeField] float spatialBlend = 1.0f;
	[Tooltip("この距離（m）までは音量が下がらない（大きな音ほど長くする）")]
	[SerializeField] float minDistance = 5.0f;
	[Tooltip("この距離（m）より遠くでは、音量がそれ以上下がらない")]
	[SerializeField] float maxDistance = 100.0f;
	[Tooltip("ドップラー効果の強さ（0 = 無し。カメラが回った時などに音の高さが変わらないようにする）")]
	[SerializeField] float dopplerLevel = 0.0f;

	/// <summary>
	/// コンストラクタ（音の種類ごとに、音量が下がり始める距離と下がらなくなる距離を決める）
	/// </summary>
	public SpatialSoundSetting(float minDistance, float maxDistance)
	{
		this.minDistance = minDistance;
		this.maxDistance = maxDistance;
	}

	/// <summary>
	/// AudioSource をこの設定（3D）にする
	/// </summary>
	public void Apply(AudioSource audioSource)
	{
		audioSource.spatialBlend = spatialBlend;
		audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
		audioSource.minDistance = minDistance;
		audioSource.maxDistance = maxDistance;
		audioSource.dopplerLevel = dopplerLevel;
	}
}
