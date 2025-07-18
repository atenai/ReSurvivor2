using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractObjectNotification : MonoBehaviour
{

    readonly string TAG_PLAYER = "Player";

    [SerializeField] UnityEngine.UI.Image currentButtonUI;
    [SerializeField] Sprite spriteKeyboard;

    [Tooltip("上下の振幅")]
    [SerializeField] float amplitude = 10.0f;
    [Tooltip("周期")]
    [SerializeField] float timeSpan = 2.0f;
    RectTransform startRectTransform;
    bool isHit = false;


    void Start()
    {
        currentButtonUI.sprite = spriteKeyboard;
        startRectTransform = currentButtonUI.rectTransform;
        currentButtonUI.enabled = false;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag(TAG_PLAYER))
        {
            isHit = true;
            currentButtonUI.enabled = true;
            StartCoroutine(UpdateNotification());
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.CompareTag(TAG_PLAYER))
        {
            isHit = false;
            currentButtonUI.enabled = false;
        }
    }

    /// <summary>
    /// 通知アイコンの更新
    /// </summary>
    IEnumerator UpdateNotification()
    {
        while (isHit == true)
        {
            MoveNotification();

            yield return null;
        }
    }

    /// <summary>
    /// 通知アイコンを上下に移動させる
    /// </summary>
    private void MoveNotification()
    {
        float posY = amplitude * Mathf.Sin(2 * Mathf.PI * Time.time / timeSpan);
        currentButtonUI.rectTransform.anchoredPosition = new Vector2(startRectTransform.anchoredPosition.x, posY);
    }
}


