using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlay : MonoBehaviour
{
    [SerializeField] AudioClip audioClip;
    AudioSource audioSource;

    void Start()
    {
        audioSource = this.GetComponent<AudioSource>();
        audioSource.PlayOneShot(audioClip);
    }

    void Update()
    {

    }
}
