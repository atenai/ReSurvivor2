using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

	public void EnableMap()
	{
		mapActive = mapActive ? false : true;
		this.gameObject.SetActive(mapActive);
	}
}
