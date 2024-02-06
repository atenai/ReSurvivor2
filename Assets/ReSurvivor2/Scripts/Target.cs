using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] float health = 50.0f;

    void Start()
    {

    }

    void Update()
    {

    }

    public void TakeDamage(float amount)
    {
        health = health - amount;
        Debug.Log("<color=orange>health : " + health + "</color>");
        if (health <= 0.0f)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(this.gameObject);
    }
}
