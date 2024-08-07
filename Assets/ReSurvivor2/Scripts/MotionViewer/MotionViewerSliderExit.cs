using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MotionViewerSliderExit : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Tooltip("モーションビュアシーンスクリプト")]
    [SerializeField] MotionViewerManager _motionViewerManager;

    Slider _slider;

    void Awake()
    {
        _slider = this.GetComponent<Slider>();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log(_slider.value);

        if (_motionViewerManager.IsPlayAnimation == false)
        {
            StartCoroutine(_motionViewerManager.SliderReflectAnimation(_slider.value));
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {

    }
}