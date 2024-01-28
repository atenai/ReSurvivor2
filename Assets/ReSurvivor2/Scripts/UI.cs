using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] Image imageReticle;

    void Start()
    {

    }

    void Update()
    {
        if (player.GetComponent<Player>().IsAim == false)
        {
            imageReticle.gameObject.SetActive(false);
        }
        else
        {
            imageReticle.gameObject.SetActive(true);
        }
    }
}
