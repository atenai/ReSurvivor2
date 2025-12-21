using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// マップUIの管理
/// </summary>
public class MapUI : MonoBehaviour
{
	[SerializeField] GameObject[] stages;
	[SerializeField] Sprite currentStageSprite;
	[SerializeField] Sprite NonStageSprite;
	int currentStageNumber = 0;
	public int CurrentStageNumber
	{
		get { return currentStageNumber; }
		set { currentStageNumber = value; }
	}

	bool mapActive = false;

	void Start()
	{
		stages[currentStageNumber].gameObject.GetComponent<Image>().sprite = currentStageSprite;
		stages[currentStageNumber].gameObject.GetComponent<Image>().color = Color.red;
		this.gameObject.SetActive(mapActive);
	}

	void Update()
	{

	}

	/// <summary>
	/// ステージ番号の設定
	/// </summary>
	/// <param name="stageNumber">ステージ番号</param>
	public void SetStageNumber(int stageNumber)
	{
		currentStageNumber = stageNumber;

		for (int i = 0; i < stages.Length; i++)
		{
			stages[i].gameObject.GetComponent<Image>().sprite = NonStageSprite;
			stages[i].gameObject.GetComponent<Image>().color = Color.white;
		}

		stages[currentStageNumber].gameObject.GetComponent<Image>().sprite = currentStageSprite;
		stages[currentStageNumber].gameObject.GetComponent<Image>().color = Color.red;
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
