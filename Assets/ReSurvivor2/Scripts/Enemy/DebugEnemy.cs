using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

[Serializable]
public class DebugEnemy
{
	[Header("デバッグ用変数")]

	#region 非公開変数
	[Tooltip("デバッグ用処理をONにするか？を判別する変数")]
	[SerializeField] bool isDebugMode = false;

	List<GameObject> debugTextList = new List<GameObject>();

	[Tooltip("生成するデバッグ用テキストのプレハブ")]
	[SerializeField] GameObject debugTextPrefab = null;

	[Tooltip("デバッグ用テキストのグラデーションの設定")]
	[SerializeField] Gradient debugGradient = default;

	bool isCreateDebugText = false;

	[SerializeField] float debugTextPosY = 60.0f;//GroundEnemy = 60.0f, FlyingEnemy = 20.0f

	int count = 0;

	[Tooltip("デバッグ用ゲームオブジェクトを破棄する時間")]
	[SerializeField] float debugGameObjectDestroyTime = 3.0f;

	[Tooltip("デバッグ用の赤いキューブ")]
	[SerializeField] GameObject debugLeftRedCubePrefab = null;

	[Tooltip("デバッグ用の青いキューブ")]
	[SerializeField] GameObject debugRightBlueCubePrefab = null;

	[Tooltip("デバッグ用の大スフィア")]
	[SerializeField] GameObject debugLargeSpherePrefab = null;

	[Tooltip("デバッグ用の中スフィア")]
	[SerializeField] GameObject debugMediumSpherePrefab = null;

	[Tooltip("デバッグ用の小スフィア")]
	[SerializeField] GameObject debugSmallSpherePrefab = null;

	[Tooltip("前方の地面チェックのメッシュ")]
	[SerializeField] MeshRenderer meshRendererGroundCheckFront = null;
	#endregion

	#region プロパティ

	public bool IsDebugMode
	{
		get { return isDebugMode; }
		set { isDebugMode = value; }
	}

	public bool IsCreateDebugText => isCreateDebugText;

	public float DebugGameObjectDestroyTime => debugGameObjectDestroyTime;

	public GameObject DebugLeftRedCubePrefab => debugLeftRedCubePrefab;

	public GameObject DebugRightBlueCubePrefab => debugRightBlueCubePrefab;

	public GameObject DebugLargeSpherePrefab => debugLargeSpherePrefab;

	public GameObject DebugMediumSpherePrefab => debugMediumSpherePrefab;

	public GameObject DebugSmallSpherePrefab => debugSmallSpherePrefab;

	#endregion

	#region 公開関数

	/// <summary>
	/// デバッグ関連の処理
	/// </summary>
	public void DebugSystem(bool isDebugMode, Canvas canvas, int count)
	{
		this.count = count;

		if (isDebugMode == true)
		{
			DebugCreateText(canvas);

			if (isCreateDebugText == true)
			{
				foreach (var text in debugTextList)
				{
					text.GetComponent<TextMeshProUGUI>().color = DebugChangeTextColor();
				}
			}
		}
		else
		{
			DestroyAllDebugText();
		}
	}

	/// <summary>
	/// デバッグ用テキストに文字列をセットする
	/// </summary>
	public void SetDebugText(string[] debugTexts)
	{
		if (isCreateDebugText == true)
		{
			for (int i = 0; i < count; i++)
			{
				debugTextList[i].GetComponent<TextMeshProUGUI>().text = debugTexts[i];
			}
		}
	}

	/// <summary>
	/// 前方の地面チェックメッシュの表示を変更
	/// </summary>
	public void DebugGroundCheckFrontMesh(bool isDebugMode)
	{
		if (meshRendererGroundCheckFront == null)
		{
			return;
		}

		if (isDebugMode == false)
		{
			meshRendererGroundCheckFront.enabled = false;
			return;
		}

		meshRendererGroundCheckFront.enabled = true;
	}

	#endregion

	#region 非公開関数

	/// <summary>
	/// デバッグ用テキストを作成する処理
	/// </summary>
	void DebugCreateText(Canvas canvas)
	{
		if (isCreateDebugText == false)
		{
			for (int i = 0; i < count; i++)
			{
				GameObject debugTextGameObject = UnityEngine.Object.Instantiate(debugTextPrefab, canvas.transform);
				float heightCalculation = (i * 10);
				debugTextGameObject.GetComponent<RectTransform>().localPosition = new Vector3(debugTextGameObject.GetComponent<RectTransform>().localPosition.x, debugTextPosY + heightCalculation, debugTextGameObject.GetComponent<RectTransform>().localPosition.z);
				debugTextList.Add(debugTextGameObject);
				debugTextList[i].GetComponent<TextMeshProUGUI>().color = debugGradient.Evaluate(0);
			}

			isCreateDebugText = true;
		}
	}

	/// <summary>
	/// デバッグテキストカラーの色を変更
	/// </summary>
	Color DebugChangeTextColor()
	{
		return debugGradient.Evaluate(Mathf.PingPong(Time.time, 1.0f));
	}

	/// <summary>
	/// 全てのデバッグテキストを削除する
	/// </summary>
	void DestroyAllDebugText()
	{
		foreach (var text in debugTextList)
		{
			UnityEngine.Object.Destroy(text);
		}

		debugTextList.Clear();

		isCreateDebugText = false;
	}

	#endregion
}