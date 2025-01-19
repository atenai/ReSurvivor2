using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestMapManager : MonoBehaviour
{
    [SerializeField] GameObject[] stages;
    int currentStageNumber = 0;
    [SerializeField] Sprite currentStageSprite;
    [SerializeField] Sprite NonStageSprite;
    [SerializeField] Slider slider;

    void Start()
    {
        stages[currentStageNumber].gameObject.GetComponent<Image>().sprite = currentStageSprite;
        stages[currentStageNumber].gameObject.GetComponent<Image>().color = Color.red;

        //スライダーの最大値の設定
        slider.maxValue = stages.Length - 1;
        //スライダーの現在値の設定
        slider.value = currentStageNumber;
    }

    void Update()
    {

    }

    public void OnClickNextStage()
    {
        for (int i = 0; i < stages.Length; i++)
        {
            stages[i].gameObject.GetComponent<Image>().sprite = NonStageSprite;
            stages[i].gameObject.GetComponent<Image>().color = Color.white;
        }

        //currentStageNumber = currentStageNumber + 1;

        if (stages.Length <= currentStageNumber)
        {
            currentStageNumber = stages.Length - 1;
        }

        stages[currentStageNumber].gameObject.GetComponent<Image>().sprite = currentStageSprite;
        stages[currentStageNumber].gameObject.GetComponent<Image>().color = Color.red;
    }

    public void OnSliderNextStageNumber()
    {
        currentStageNumber = (int)slider.value;
    }
}
