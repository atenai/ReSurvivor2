using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 次のエリア（ステージ）へ行く出口に、黄色い横線の目印を出すクラス
/// 出口の当たり判定はビルドでは見えない（MeshOff）ので、この目印で出口の場所を分かるようにする
/// ・出口の当たり判定の幅いっぱいに横線を張る（塹壕の壁などがある時は壁の手前まで）
/// ・横線の真ん中に行き先のエリア番号、上に行き先のエリアのアイコンとエリア名を出す（画面の真ん中のプレイヤーに隠れにくいように上に出す）
/// ・ワールド空間のUIなので、壁や物の陰では隠れる。いつもカメラの方を向き、遠くでも小さくなりすぎない
/// ・コライダーは無いので、プレイヤーや敵、弾の邪魔はしない
/// </summary>
public class ExitMarker : MonoBehaviour
{
	[Tooltip("この目印を出す出口")]
	[SerializeField] LoadingNextStageScene exit;
	[Tooltip("出口の当たり判定（LoadNextStageSceneObject の BoxCollider）")]
	[SerializeField] BoxCollider exitTrigger;

	[Header("UI")]
	[Tooltip("目印のワールド空間のキャンバス（プレハブでは非表示にしておき、Start で位置を決めてから表示する）")]
	[SerializeField] RectTransform markerCanvas;
	[Tooltip("目印全体の透明度")]
	[SerializeField] CanvasGroup canvasGroup;
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
	[Tooltip("壁がある時に、横線の端を壁からどれだけ離すか（m）")]
	[SerializeField] float wallMargin = 0.15f;
	[Tooltip("横線の最短の長さ（m）。壁が近すぎる時もこれより短くしない")]
	[SerializeField] float minBarLength = 1.0f;
	[Tooltip("カメラがこの距離（m）より遠い時は、線の太さや文字が小さくなりすぎないように大きくする")]
	[SerializeField] float constantSizeDistance = 10.0f;

	[Header("表示")]
	[Tooltip("プレイヤーがこの距離（m）より近い時に表示する")]
	[SerializeField] float showDistance = 30.0f;
	[Tooltip("showDistance からさらにこの距離（m）遠くなる間に、だんだん消える")]
	[SerializeField] float fadeDistance = 8.0f;

	[Tooltip("キャンバスの1単位を 1cm にする（1m = 100単位）")]
	const float CanvasUnitsPerMeter = 100.0f;
	[Tooltip("半分にする時の割合")]
	const float Half = 0.5f;
	[Tooltip("壁を調べる高さ（足元、m）")]
	const float WallCheckLowHeight = 0.3f;
	[Tooltip("目印の大きさの倍率の最小（近い時は実際の大きさのまま）")]
	const float MinSizeScale = 1.0f;
	[Tooltip("カメラの向きを計算できないほど近い時の、長さの2乗の下限")]
	const float MinFacingSqrLength = 0.0001f;
	[Tooltip("エリア番号を2桁で表示する書式（例: 03）")]
	const string AreaNumberFormat = "00";
	[Tooltip("エリア名の上に小さく出す見出し（TextMeshPro のリッチテキストで小さくする）")]
	const string NextAreaHeading = "<size=70%>NEXT AREA</size>";
	[Tooltip("見出しとエリア名の間の改行")]
	const string NewLine = "\n";
	[Tooltip("壁を調べる時に無視するレイヤー（動くキャラクターや弾のエフェクト）")]
	static readonly string[] IgnoredWallLayerNames = { "Player", "Enemy", "FlyingEnemy", "GroundEnemy", "BulletEffect" };

	[Tooltip("横線の真ん中（ワールド座標）")]
	Vector3 barCenter;
	[Tooltip("横線の向き（ワールド座標、水平）")]
	Vector3 barDirection;
	[Tooltip("横線の長さ（m）")]
	float barLength;
	[Tooltip("横線の真ん中のすき間の半分（エリア番号の場所、キャンバスの単位）")]
	float centerGap;
	[Tooltip("位置を決め終わったか")]
	bool isReady = false;

	void Start()
	{
		if (exit == null || exitTrigger == null)
		{
			Debug.LogError("ExitMarker：出口が設定されていません。");
			return;
		}

		CalculateLayout();
		SetDestination(exit.NextStage);
		//プレハブで作った真ん中のすき間（横線の左半分の右端の位置）を覚えておく
		centerGap = Mathf.Abs(barLeft.anchoredPosition.x);
		markerCanvas.position = barCenter;
		markerCanvas.gameObject.SetActive(true);
		isReady = true;
		UpdateMarker();
	}

	//カメラが動いた後に向きを合わせるため LateUpdate で行う
	void LateUpdate()
	{
		if (isReady == false)
		{
			return;
		}

		UpdateMarker();
	}

	/// <summary>
	/// 当たり判定の箱から、横線を張る位置・向き・長さを決める
	/// </summary>
	void CalculateLayout()
	{
		Transform triggerTransform = exitTrigger.transform;
		Vector3 center = triggerTransform.TransformPoint(exitTrigger.center);
		//箱の横（X）・奥（Z）・高さ（Y）の辺を、回転と拡大を含めたワールドの向きと長さにする
		Vector3 sideX = triggerTransform.TransformVector(new Vector3(exitTrigger.size.x, 0.0f, 0.0f));
		Vector3 sideZ = triggerTransform.TransformVector(new Vector3(0.0f, 0.0f, exitTrigger.size.z));
		Vector3 sideY = triggerTransform.TransformVector(new Vector3(0.0f, exitTrigger.size.y, 0.0f));
		//長い方の辺に沿って横線を張る
		Vector3 widthSide = sideZ.sqrMagnitude <= sideX.sqrMagnitude ? sideX : sideZ;
		widthSide.y = 0.0f;
		barDirection = widthSide.normalized;

		float groundY = center.y - Mathf.Abs(sideY.y) * Half;
		Vector3 groundCenter = new Vector3(center.x, groundY, center.z);

		//真ん中から左右に壁を調べて、壁があればその手前までにする
		float halfLength = widthSide.magnitude * Half;
		float minHalfLength = minBarLength * Half;
		float leftHalf = Mathf.Max(minHalfLength, FindFreeDistance(groundCenter, -barDirection, halfLength) - wallMargin);
		float rightHalf = Mathf.Max(minHalfLength, FindFreeDistance(groundCenter, barDirection, halfLength) - wallMargin);

		barLength = leftHalf + rightHalf;
		//左右で長さが違う時は、横線の真ん中をずらす
		barCenter = groundCenter + barDirection * ((rightHalf - leftHalf) * Half) + Vector3.up * barHeight;
	}

	/// <summary>
	/// 地面の位置 from から direction の向きに、壁に当たらずに進める距離を返す（最大 maxDistance）
	/// 足元と横線の高さの2か所で調べて、近い方を使う
	/// </summary>
	float FindFreeDistance(Vector3 from, Vector3 direction, float maxDistance)
	{
		float lowDistance = FindWallDistance(from + Vector3.up * WallCheckLowHeight, direction, maxDistance);
		float barDistance = FindWallDistance(from + Vector3.up * barHeight, direction, maxDistance);
		return Mathf.Min(lowDistance, barDistance);
	}

	/// <summary>
	/// origin から direction の向きにレイを飛ばして、一番近い壁までの距離を返す（壁が無ければ maxDistance）
	/// Physics.RaycastAll はレイに当たったコライダーを全部返す関数（近い順とは限らないので、一番近いものを探す）
	/// </summary>
	float FindWallDistance(Vector3 origin, Vector3 direction, float maxDistance)
	{
		float nearest = maxDistance;
		//トリガー（当たり判定だけのコライダー）は壁ではないので無視する
		RaycastHit[] hits = Physics.RaycastAll(origin, direction, maxDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore);
		foreach (RaycastHit hit in hits)
		{
			if (IsWall(hit.collider) == false)
			{
				continue;
			}

			if (hit.distance < nearest)
			{
				nearest = hit.distance;
			}
		}
		return nearest;
	}

	/// <summary>
	/// 壁として扱うコライダーかどうか（プレイヤーや敵など、動くものは壁ではない）
	/// </summary>
	bool IsWall(Collider collider)
	{
		//別のシーンのもの（シーンをまたいで残るプレイヤーなど）は無視する
		if (collider.gameObject.scene != this.gameObject.scene)
		{
			return false;
		}

		string layerName = LayerMask.LayerToName(collider.gameObject.layer);
		foreach (string ignoredLayerName in IgnoredWallLayerNames)
		{
			if (layerName == ignoredLayerName)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// 行き先のエリア番号・名前・アイコンを表示する（名前とアイコンはマップUIのものを使う）
	/// </summary>
	void SetDestination(EnumManager.StageTYPE nextStage)
	{
		int stageNumber = (int)nextStage;
		textAreaNumber.text = stageNumber.ToString(AreaNumberFormat);

		MapUI.AreaNode area = FindArea(stageNumber);
		if (area == null)
		{
			//マップに載っていないステージは、ステージ名だけ出す
			textAreaName.text = NextAreaHeading + NewLine + nextStage.ToString().ToUpper();
			imageAreaIcon.enabled = false;
			return;
		}

		textAreaName.text = NextAreaHeading + NewLine + area.areaName;
		imageAreaIcon.enabled = area.icon != null;
		if (area.icon != null)
		{
			imageAreaIcon.sprite = area.icon.sprite;
		}
	}

	/// <summary>
	/// マップUIからステージ番号のエリアを取り出す（無ければ null）
	/// </summary>
	MapUI.AreaNode FindArea(int stageNumber)
	{
		ScreenUIManagerPresenter screenUIManagerPresenter = ScreenUIManagerPresenter.SingletonInstance;
		if (screenUIManagerPresenter == null || screenUIManagerPresenter.ScreenUIView == null || screenUIManagerPresenter.ScreenUIView.MapUI == null)
		{
			return null;
		}

		return screenUIManagerPresenter.ScreenUIView.MapUI.GetAreaNode(stageNumber);
	}

	/// <summary>
	/// 表示・非表示、向き、大きさを更新する
	/// </summary>
	void UpdateMarker()
	{
		Camera mainCamera = Camera.main;
		if (mainCamera == null)
		{
			return;
		}

		//プレイヤーから遠い出口の目印は消す（遠くの目印で画面がうるさくならないようにする）
		canvasGroup.alpha = Mathf.Clamp01((showDistance + fadeDistance - GetPlayerDistance(mainCamera)) / fadeDistance);
		if (canvasGroup.alpha <= 0.0f)
		{
			return;
		}

		//文字が左右反転しないように、横線の右向きをカメラの右向きにそろえる
		Vector3 right = barDirection;
		if (Vector3.Dot(right, mainCamera.transform.right) < 0.0f)
		{
			right = -right;
		}

		//横線を軸にして回して、カメラの方を向かせる（横線は出口の幅の向きのまま）
		Vector3 toMarker = barCenter - mainCamera.transform.position;
		Vector3 forward = toMarker - right * Vector3.Dot(toMarker, right);
		if (forward.sqrMagnitude < MinFacingSqrLength)
		{
			return;
		}
		//Cross(forward, right) は forward と right の両方に直角な向き（ここでは上向き）
		markerCanvas.rotation = Quaternion.LookRotation(forward, Vector3.Cross(forward, right));

		//遠くでも線の太さや文字が小さくなりすぎないように、距離に合わせて大きくする（横線の長さは出口の幅のまま）
		float sizeScale = Mathf.Max(MinSizeScale, toMarker.magnitude / constantSizeDistance);
		markerCanvas.localScale = Vector3.one * (sizeScale / CanvasUnitsPerMeter);
		SetBarLength(barLength * Half * CanvasUnitsPerMeter / sizeScale);
	}

	/// <summary>
	/// プレイヤー（いなければカメラ）から横線の真ん中までの水平の距離を返す
	/// </summary>
	float GetPlayerDistance(Camera mainCamera)
	{
		Vector3 position = mainCamera.transform.position;
		if (PlayerManagerPresenter.SingletonInstance != null)
		{
			position = PlayerManagerPresenter.SingletonInstance.transform.position;
		}

		Vector3 offset = barCenter - position;
		offset.y = 0.0f;
		return offset.magnitude;
	}

	/// <summary>
	/// 横線の長さを設定する（真ん中のすき間を空けて左右に伸ばし、両端に縦線を置く）
	/// </summary>
	/// <param name="halfLength">横線の長さの半分（キャンバスの単位）</param>
	void SetBarLength(float halfLength)
	{
		float sideLength = Mathf.Max(0.0f, halfLength - centerGap);
		barLeft.sizeDelta = new Vector2(sideLength, barLeft.sizeDelta.y);
		barRight.sizeDelta = new Vector2(sideLength, barRight.sizeDelta.y);
		endLeft.anchoredPosition = new Vector2(-halfLength, endLeft.anchoredPosition.y);
		endRight.anchoredPosition = new Vector2(halfLength, endRight.anchoredPosition.y);
	}
}
