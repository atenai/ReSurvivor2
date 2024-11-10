using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ビルド時にメッシュをOFFにするクラス
/// </summary>
public class MeshOff : MonoBehaviour
{
    void Start()
    {
#if UNITY_STANDALONE_WIN
        this.GetComponent<MeshRenderer>().enabled = false;
#endif

#if UNITY_EDITOR
        this.GetComponent<MeshRenderer>().enabled = true;
#endif
    }
}
