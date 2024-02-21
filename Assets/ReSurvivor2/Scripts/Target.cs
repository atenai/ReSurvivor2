using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Target : MonoBehaviour
{
    float currentHp = 100.0f;
    [SerializeField] float maxHp = 100.0f;
    [SerializeField] Canvas canvas;
    [SerializeField] Slider sliderHp;

    void Start()
    {
        sliderHp.value = 1;
        currentHp = maxHp;
    }

    void Update()
    {
        //常にキャンバスをメインカメラの方を向かせる
        canvas.transform.LookAt(Camera.main.transform);
    }

    public void TakeDamage(float amount)
    {
        currentHp = currentHp - amount;
        Debug.Log("<color=orange>currentHp : " + currentHp + "</color>");
        sliderHp.value = (float)currentHp / (float)maxHp;
        if (currentHp <= 0.0f)
        {
            Destroy(this.gameObject);
        }
    }
}
