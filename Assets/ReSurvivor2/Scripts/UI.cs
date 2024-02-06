using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] Image imageCrosshair;

    void Start()
    {

    }

    void Update()
    {
        if (player.GetComponent<Player>().IsAim == false)
        {
            imageCrosshair.gameObject.SetActive(false);
        }
        else
        {
            imageCrosshair.gameObject.SetActive(true);
        }
    }
}
