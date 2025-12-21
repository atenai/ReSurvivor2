using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// マップUIの管理
/// </summary>
public class MapUI : MonoBehaviour
{
	[Tooltip("現在のプレイヤーステージイメージリスト")]
	[SerializeField] Image[] currentPlayerStages;
	[Tooltip("ミッション終了のコンピューターステージイメージリスト")]
	[SerializeField] Image[] endComputerStages;
	[Tooltip("マップの表示・非表示状態")]
	bool mapActive = false;

	void Start()
	{
		this.gameObject.SetActive(mapActive);
	}

	void Update()
	{

	}

	/// <summary>
	/// 現在のプレイヤーステージ番号の設定
	/// </summary>
	/// <param name="currentPlayerStageNumber">ステージ番号</param>
	public void SetCurrentPlayerStageNumber(int currentPlayerStageNumber)
	{
		for (int i = 0; i < currentPlayerStages.Length; i++)
		{
			currentPlayerStages[i].color = Color.clear;
		}

		currentPlayerStages[currentPlayerStageNumber].color = Color.red;
	}

	/// <summary>
	/// ミッション終了のコンピューターステージ番号の設定
	/// </summary>
	/// <param name="endComputerStageNumber">ステージ番号</param>
	public void SetEndComputerStageNumber(int endComputerStageNumber = -1)
	{
		for (int i = 0; i < endComputerStages.Length; i++)
		{
			endComputerStages[i].color = Color.white;
		}

		if (endComputerStageNumber != -1)
		{
			endComputerStages[endComputerStageNumber].color = Color.blue;
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
}
