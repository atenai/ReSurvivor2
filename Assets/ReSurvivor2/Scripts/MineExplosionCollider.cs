using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineExplosionCollider : MonoBehaviour
{
    [Tooltip("ダメージ")]
    [SerializeField] int damage = 50;
    float scale = 1.0f;

    void Start()
    {
        scale = 1.0f;
        this.transform.localScale = new Vector3(scale, scale, scale);
    }

    void Update()
    {
        scale = scale + Time.deltaTime;
        this.transform.localScale = new Vector3(scale, scale, scale);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Enemy") || collider.gameObject.CompareTag("FlyingEnemy") || collider.gameObject.CompareTag("GroundEnemy"))
        {
            //ダメージ
            Target target = collider.transform.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
        }

        if (collider.CompareTag("Player"))
        {
            Player.SingletonInstance.TakeDamage(damage);
        }
    }
}
