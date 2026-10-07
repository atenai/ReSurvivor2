using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// マップUIの管理
/// エリア（ステージ）を丸いノード、エリア同士のつながりを線で表示する
/// 現在地のノードはオレンジで塗り、周りにレーダーの輪を表示する
/// ミッション終了のコンピューターがあるエリアには TARGET を表示する
/// </summary>
public class MapUI : MonoBehaviour
{
	/// <summary>
	/// マップ上の1エリア
	/// </summary>
	[System.Serializable]
	public class AreaNode
	{
		[Tooltip("エリア番号の表示（例: AREA 03）")]
		public string areaCode;
		[Tooltip("英語のエリア名（ノードの横に表示）")]
		public string areaName;
		[Tooltip("日本語のエリア名（現在地として画面左上に表示）")]
		public string areaNameJapanese;
		[Tooltip("ノードの親（マップ上の位置）")]
		public RectTransform root;
		[Tooltip("ノードの塗り")]
		public Image fill;
		[Tooltip("ノードのアイコン")]
		public Image icon;
		[Tooltip("ノードの横のエリア名ラベル")]
		public TextMeshProUGUI label;
		[Tooltip("目的地になったときに TARGET を出す向き（x: 右 1 / 左 -1、y: 上 1 / 下 -1）。ほかのラベルや線と重ならない向きにする")]
		public Vector2 targetLabelDirection = new Vector2(1.0f, 1.0f);
	}

	[Tooltip("エリアのノードリスト（要素番号 = ステージ番号。マップにないステージは root を空にする）")]
	[SerializeField] AreaNode[] areaNodes;

	[Header("現在地")]
	[Tooltip("現在地のノードに重ねるレーダーの輪")]
	[SerializeField] RectTransform currentMarker;
	[Tooltip("ゆっくり回る点線の輪")]
	[SerializeField] RectTransform currentMarkerDashedRing;
	[Tooltip("広がりながら消える輪")]
	[SerializeField] Image currentMarkerPulse;
	[Tooltip("画面左上のエリア番号・英語名")]
	[SerializeField] TextMeshProUGUI textCurrentAreaCode;
	[Tooltip("画面左上の日本語のエリア名")]
	[SerializeField] Text textCurrentAreaName;
	[Tooltip("画面左上のエリアアイコン")]
	[SerializeField] Image imageCurrentAreaIcon;

	[Header("目的地（ミッション終了のコンピューター）")]
	[Tooltip("目的地のノードに重ねる TARGET 表示")]
	[SerializeField] RectTransform targetMarker;
	[Tooltip("目的地のノードの周りで回る目印")]
	[SerializeField] RectTransform targetMarkerTicks;
	[Tooltip("TARGET の引き出し線と下線（右上向きに作り、向きに合わせて反転する）")]
	[SerializeField] RectTransform targetLeader;
	[Tooltip("TARGET の文字")]
	[SerializeField] TextMeshProUGUI targetLabel;

	[Header("色")]
	[SerializeField] Color nodeFillColor = new Color(0.08f, 0.09f, 0.09f, 0.85f);
	[SerializeField] Color nodeIconColor = Color.white;
	[SerializeField] Color currentNodeFillColor = new Color(1.0f, 0.55f, 0.1f, 1.0f);
	[SerializeField] Color currentNodeIconColor = new Color(0.12f, 0.12f, 0.12f, 1.0f);

	[Tooltip("マップの表示・非表示状態")]
	bool mapActive = false;
	[Tooltip("アニメーションの経過時間")]
	float animationTime = 0.0f;
	[Tooltip("現在地の輪が広がる周期（秒）")]
	const float PulseSeconds = 1.6f;
	[Tooltip("TARGET の文字の位置（ノード中心から右上向きの場合）")]
	static readonly Vector2 TargetLabelOffset = new Vector2(88.0f, 86.0f);

	void Start()
	{
		this.gameObject.SetActive(mapActive);
	}

	void Update()
	{
		// ポーズ中（timeScale = 0）でも動くように unscaledDeltaTime を使う
		animationTime = animationTime + Time.unscaledDeltaTime;

		if (currentMarkerDashedRing != null)
		{
			currentMarkerDashedRing.localRotation = Quaternion.Euler(0.0f, 0.0f, -animationTime * 12.0f);
		}

		if (targetMarkerTicks != null)
		{
			targetMarkerTicks.localRotation = Quaternion.Euler(0.0f, 0.0f, animationTime * 30.0f);
		}

		if (currentMarkerPulse != null)
		{
			float t = Mathf.Repeat(animationTime, PulseSeconds) / PulseSeconds;
			currentMarkerPulse.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.0f, 1.8f, t);
			Color color = currentNodeFillColor;
			color.a = (1.0f - t) * 0.8f;
			currentMarkerPulse.color = color;
		}
	}

	/// <summary>
	/// 現在のプレイヤーステージ番号の設定
	/// </summary>
	/// <param name="currentPlayerStageNumber">ステージ番号</param>
	public void SetCurrentPlayerStageNumber(int currentPlayerStageNumber)
	{
		ApplyLabels();

		AreaNode current = GetAreaNode(currentPlayerStageNumber);
		for (int i = 0; i < areaNodes.Length; i++)
		{
			AreaNode node = areaNodes[i];
			if (node.root == null)
			{
				continue;
			}

			bool isCurrent = node == current;
			if (node.fill != null)
			{
				node.fill.color = isCurrent ? currentNodeFillColor : nodeFillColor;
			}

			if (node.icon != null)
			{
				node.icon.color = isCurrent ? currentNodeIconColor : nodeIconColor;
			}
		}

		if (currentMarker != null)
		{
			currentMarker.gameObject.SetActive(current != null);
			if (current != null)
			{
				currentMarker.anchoredPosition = current.root.anchoredPosition;
			}
		}

		if (textCurrentAreaCode != null)
		{
			textCurrentAreaCode.text = current != null ? "CURRENT AREA  [ " + current.areaCode + " : " + current.areaName + " ]" : "CURRENT AREA  [ UNKNOWN ]";
		}

		if (textCurrentAreaName != null)
		{
			textCurrentAreaName.text = current != null ? current.areaNameJapanese : "不明なエリア";
		}

		if (imageCurrentAreaIcon != null)
		{
			imageCurrentAreaIcon.enabled = current != null && current.icon != null;
			if (imageCurrentAreaIcon.enabled == true)
			{
				imageCurrentAreaIcon.sprite = current.icon.sprite;
			}
		}
	}

	/// <summary>
	/// ミッション終了のコンピューターステージ番号の設定
	/// </summary>
	/// <param name="endComputerStageNumber">ステージ番号（-1 で非表示）</param>
	public void SetEndComputerStageNumber(int endComputerStageNumber = -1)
	{
		AreaNode target = GetAreaNode(endComputerStageNumber);
		if (targetMarker != null)
		{
			targetMarker.gameObject.SetActive(target != null);
			if (target != null)
			{
				targetMarker.anchoredPosition = target.root.anchoredPosition;
				SetTargetLabelDirection(target.targetLabelDirection);
			}
		}
	}

	/// <summary>
	/// TARGET の引き出し線と文字の向きを設定
	/// </summary>
	/// <param name="direction">向き（x: 右 1 / 左 -1、y: 上 1 / 下 -1）</param>
	void SetTargetLabelDirection(Vector2 direction)
	{
		float sx = direction.x < 0.0f ? -1.0f : 1.0f;
		float sy = direction.y < 0.0f ? -1.0f : 1.0f;

		if (targetLeader != null)
		{
			targetLeader.localScale = new Vector3(sx, sy, 1.0f);
		}

		if (targetLabel != null)
		{
			targetLabel.rectTransform.pivot = new Vector2(sx < 0.0f ? 1.0f : 0.0f, sy < 0.0f ? 1.0f : 0.0f);
			targetLabel.rectTransform.anchoredPosition = new Vector2(TargetLabelOffset.x * sx, TargetLabelOffset.y * sy);
			if (sx < 0.0f)
			{
				targetLabel.alignment = sy < 0.0f ? TextAlignmentOptions.TopRight : TextAlignmentOptions.BottomRight;
			}
			else
			{
				targetLabel.alignment = sy < 0.0f ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.BottomLeft;
			}
		}
	}

	/// <summary>
	/// マップの表示・非表示切り替え
	/// </summary>
	public void EnableMap()
	{
		mapActive = mapActive ? false : true;
		this.gameObject.SetActive(mapActive);
	}

	/// <summary>
	/// ステージ番号のエリアを取得（マップにないステージは null）
	/// ExitMarker（出口の黄色い横線の目印）も、行き先のエリア名とアイコンを出すのに使う
	/// </summary>
	/// <param name="stageNumber">ステージ番号</param>
	public AreaNode GetAreaNode(int stageNumber)
	{
		if (areaNodes == null || stageNumber < 0 || areaNodes.Length <= stageNumber)
		{
			return null;
		}

		AreaNode node = areaNodes[stageNumber];
		return node.root != null ? node : null;
	}

	/// <summary>
	/// ノードの横のラベルにエリア名を反映
	/// </summary>
	void ApplyLabels()
	{
		for (int i = 0; i < areaNodes.Length; i++)
		{
			if (areaNodes[i].label != null)
			{
				areaNodes[i].label.text = areaNodes[i].areaCode + " : " + areaNodes[i].areaName;
			}
		}
	}
}
