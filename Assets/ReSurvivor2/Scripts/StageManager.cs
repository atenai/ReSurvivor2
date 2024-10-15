using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 各ステージ用のマネージャー
/// </summary>
public class StageManager : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(UI.singletonInstance.FadeIn());
    }

    void Update()
    {

    }
}
