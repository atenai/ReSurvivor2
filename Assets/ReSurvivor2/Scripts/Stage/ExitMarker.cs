using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 次のエリア（ステージ）へ行く出口に、黄色い横線の目印を出すクラス
/// ・ゲームを動かしていない時（エディター）も、当たり判定の大きさを変えたらすぐに見た目に反映する（ExecuteAlways）
/// </summary>
[ExecuteAlways] //エディターで当たり判定の大きさを変えた時もすぐに反映する
public class ExitMarker : MonoBehaviour
{
	[Tooltip("この目印を出す出口")]
	[SerializeField] LoadingNextStageScene loadingNextStageScene;
	[Tooltip("出口の当たり判定（LoadNextStageSceneObject の BoxCollider）")]
	[SerializeField] BoxCollider boxCollider;

	[Header("UI")]
	[Tooltip("目印のワールド空間のキャンバス")]
	[SerializeField] RectTransform markerCanvas;
	[Tooltip("横線の左半分（右端を真ん中のすき間に合わせる）")]
	[SerializeField] RectTransform barLeft;
	[Tooltip("横線の右半分（左端を真ん中のすき間に合わせる）")]
	[SerializeField] RectTransform barRight;
	[Tooltip("横線の左端の縦線")]
	[SerializeField] RectTransform endLeft;
	[Tooltip("横線の右端の縦線")]
	[SerializeField] RectTransform endRight;
	[Tooltip("横線の真ん中の行き先のエリア番号")]
	[SerializeField] TextMeshProUGUI textAreaNumber;
	[Tooltip("横線の上の行き先のエリア名")]
	[SerializeField] TextMeshProUGUI textAreaName;
	[Tooltip("横線の上の行き先のエリアアイコン")]
	[SerializeField] Image imageAreaIcon;

	[Header("大きさ")]
	[Tooltip("横線の高さ（地面から m）")]
	[SerializeField] float barHeight = 1.3f;

	[Header("表示")]
	[Tooltip("キャンバスの1単位は 1cm（1m = 100単位。キャンバスの拡大率 0.01 と合わせる）")]
	const float CanvasUnitsPerMeter = 100.0f;
	[Tooltip("半分にする時の割合")]
	const float Half = 0.5f;
	[Tooltip("拡大していない時の拡大率")]
	const float OriginalScale = 1.0f;
	[Tooltip("エリア番号を2桁で表示する書式（例: 03）")]
	const string AreaNumberFormat = "00";
	[Tooltip("エリア名の上に小さく出す見出し（TextMeshPro のリッチテキストで小さくする）")]
	const string NextAreaHeading = "<size=70%>NEXT AREA</size>";
	[Tooltip("見出しとエリア名の間の改行")]
	const string NewLine = "\n";

	[Tooltip("横線の真ん中（ワールド座標）")]
	Vector3 barCenter;

	void Start()
	{
		ApplyLayout();

		//行き先の表示はゲーム中だけ（エディターではマップUIが無いので、プレハブの表示のままにする）
		if (Application.isPlaying == true)
		{
			SetDestination(loadingNextStageScene.NextStage);
		}
	}

	void LateUpdate()
	{
		ApplyLayout();
	}

	/// <summary>
	/// 当たり判定の大きさに合わせて、目印の位置・大きさ・横線の長さを決める
	/// </summary>
	void ApplyLayout()
	{
		Transform triggerTransform = boxCollider.transform;
		Vector3 triggerScale = triggerTransform.lossyScale;

		//当たり判定のオブジェクトの拡大（横幅に合わせて横に伸ばしている）を打ち消して、文字やアイコンが伸びないようにする
		Vector3 markerScale = new Vector3(OriginalScale / triggerScale.x, OriginalScale / triggerScale.y, OriginalScale / triggerScale.z);
		if (this.transform.localScale != markerScale)
		{
			this.transform.localScale = markerScale;
		}

		//横線は当たり判定の床から barHeight の高さ、当たり判定の真ん中に置く
		Vector3 center = triggerTransform.TransformPoint(boxCollider.center);
		float halfHeight = boxCollider.size.y * triggerScale.y * Half;
		barCenter = center + Vector3.up * (barHeight - halfHeight);
		if (markerCanvas.position != barCenter)
		{
			markerCanvas.position = barCenter;
		}

		//横線は当たり判定の横幅（X）いっぱいに伸ばす
		float barWidth = boxCollider.size.x * triggerScale.x;
		SetBarLength(barWidth * Half * CanvasUnitsPerMeter);
	}

	/// <summary>
	/// 横線の長さを設定する（真ん中のすき間を空けて左右に伸ばし、両端に縦線を置く）
	/// </summary>
	/// <param name="halfLength">横線の長さの半分（キャンバスの単位）</param>
	void SetBarLength(float halfLength)
	{
		//真ん中のすき間（エリア番号の場所）は、プレハブで作った横線の左半分の右端の位置
		float centerGap = Mathf.Abs(barLeft.anchoredPosition.x);
		float sideLength = Mathf.Max(0.0f, halfLength - centerGap);

		Vector2 barSize = new Vector2(sideLength, barLeft.sizeDelta.y);
		if (barLeft.sizeDelta != barSize)
		{
			barLeft.sizeDelta = barSize;
		}
		if (barRight.sizeDelta != barSize)
		{
			barRight.sizeDelta = barSize;
		}

		Vector2 endLeftPosition = new Vector2(-halfLength, endLeft.anchoredPosition.y);
		if (endLeft.anchoredPosition != endLeftPosition)
		{
			endLeft.anchoredPosition = endLeftPosition;
		}
		Vector2 endRightPosition = new Vector2(halfLength, endRight.anchoredPosition.y);
		if (endRight.anchoredPosition != endRightPosition)
		{
			endRight.anchoredPosition = endRightPosition;
		}
	}

	/// <summary>
	/// 行き先のエリア番号・名前・アイコンを表示する（名前とアイコンはマップUIのものを使う）
	/// </summary>
	void SetDestination(EnumManager.StageTYPE nextStage)
	{
		int stageNumber = (int)nextStage;
		textAreaNumber.text = stageNumber.ToString(AreaNumberFormat);

		MapUI.AreaNode area = ScreenUIManagerPresenter.SingletonInstance.ScreenUIView.MapUI.GetAreaNode(stageNumber);

		textAreaName.text = NextAreaHeading + NewLine + area.areaName;
		imageAreaIcon.sprite = area.icon.sprite;
	}
}
