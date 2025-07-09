using UnityEngine;

/// <summary>
/// SafeArea（ノッチやホームバーなどを避けた安全領域）を取得し、
/// RectTransformのAnchorを動的に調整するコンポーネント
/// このスクリプトのようなセーフエリアは基本的にスマホゲームにしか使わない
/// なぜならPCゲームはディスプレイのスクリーンサイズとセーフエリアの大きさがほぼ変わらないことが多い為
/// </summary>
[ExecuteInEditMode] // エディター上でも処理を実行（ただし、safeAreaは正確ではない）
[RequireComponent(typeof(RectTransform))] // RectTransformが必須
public class SafeArea : MonoBehaviour
{
	// このオブジェクトにアタッチされたRectTransform
	RectTransform panel;

	// 前回取得したsafeArea（変化を検出するために使用）
	Rect lastSafeArea = new Rect(0, 0, 0, 0);

	// 前回の画面サイズ
	Vector2Int lastScreenSize = new Vector2Int(0, 0);

	// 前回の画面の向き（縦向き・横向きなど）
	ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;

	// 初期化時にRectTransformを取得し、SafeAreaを即時適用
	void Awake()
	{
		panel = this.GetComponent<RectTransform>();
		UpdateSafeArea();
	}

	// 毎フレーム呼ばれ、画面サイズやSafeAreaに変化があれば再適用
	void Update()
	{
		UpdateSafeArea();
	}

	// 現在のSafeArea情報を取得し、前回と比較して変化があれば反映
	void UpdateSafeArea()
	{
		Rect safeArea = Screen.safeArea;

		// safeArea・画面サイズ・画面向きのいずれかに変化があれば処理を行う
		if (safeArea != lastSafeArea
			|| Screen.width != lastScreenSize.x
			|| Screen.height != lastScreenSize.y
			|| Screen.orientation != lastOrientation)
		{
			// 現在の画面情報を保存
			lastScreenSize.x = Screen.width;
			lastScreenSize.y = Screen.height;
			lastOrientation = Screen.orientation;

			// SafeAreaを適用
			ApplySafeArea(safeArea);
		}
	}

	// SafeAreaをanchorMinとanchorMaxに変換してUIに反映
	void ApplySafeArea(Rect safeArea)
	{
		// 最後に適用したSafeAreaを保存
		lastSafeArea = safeArea;

		// 画面サイズが正しく取得できているかチェック
		if (Screen.width > 0 && Screen.height > 0)
		{
			// safeAreaの左下（position）と右上（position + size）を取得
			Vector2 anchorMin = safeArea.position;
			Vector2 anchorMax = safeArea.position + safeArea.size;

			// anchorは0〜1の範囲なので、画面サイズで正規化する
			anchorMin.x /= Screen.width;
			anchorMin.y /= Screen.height;
			anchorMax.x /= Screen.width;
			anchorMax.y /= Screen.height;

			// anchorの値が正しい範囲内であれば反映
			if (anchorMin.x >= 0 && anchorMin.y >= 0 && anchorMax.x >= 0 && anchorMax.y >= 0)
			{
				panel.anchorMin = anchorMin;
				panel.anchorMax = anchorMax;
				// NOTE:これはアンカー座標を変更した際になぜかUnityは座標位置がずれてしまう為、必ず初期位置に戻すという意味で必要がある処理
				panel.anchoredPosition = Vector2.zero;
			}
		}
	}
}
