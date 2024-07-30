#if UNITY_EDITOR
using UnityEngine;
using TMPro;

/// <summary>
/// モーションビュアのボタン情報
/// </summary>
public class MotionViewerButtonInfo : MonoBehaviour
{
    #region 非公開変数

    [SerializeField] TextMeshProUGUI _buttonName;

    #endregion

    #region プロパティ

    public TextMeshProUGUI ButtonName
    {
        get
        {
            return _buttonName;
        }
        set
        {
            _buttonName = value;
        }
    }

    #endregion
}
#endif
