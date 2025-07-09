using UnityEngine;

/// <summary>
/// 3DUIの大きさを画面の解像度に合わせてスケールを変更するクラス
/// </summary>
[System.Obsolete("使えないが参考になるかも")]
public class UIScaler : MonoBehaviour
{
	[SerializeField] private Canvas worldCanvas;
	[SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);
	[SerializeField] private float baseScale = 0.01f; // 基準スケール

	void Start()
	{
		UpdateCanvasScale();
	}

	void UpdateCanvasScale()
	{
		float screenRatio = Mathf.Min(
			(float)Screen.width / referenceResolution.x,
			(float)Screen.height / referenceResolution.y
		);

		worldCanvas.transform.localScale = Vector3.one * baseScale * screenRatio;
	}
}
