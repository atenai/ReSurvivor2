using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 空中敵キャラクタービュー
/// MVPパターンのView担当
/// </summary>
public class FlyingEnemyCharacterView : MonoBehaviour
{
    [Header("レイキャスト")]
    [Tooltip("視界用の頭ゲームオブジェクト")]
    [SerializeField] GameObject head;
    public GameObject Head => head;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
