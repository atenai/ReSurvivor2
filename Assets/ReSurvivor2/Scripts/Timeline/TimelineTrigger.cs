using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineTrigger : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector;
    bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isTriggered == false)
        {
            isTriggered = true;
            Debug.Log("当たりました"); //確認用
            PlayTimeline();
        }
    }

    //再生する
    void PlayTimeline()
    {
        playableDirector.Play();
    }

    //一時停止する
    void PauseTimeline()
    {
        playableDirector.Pause();
    }

    //一時停止を再開する
    void ResumeTimeline()
    {
        playableDirector.Resume();
    }

    //停止する
    void StopTimeline()
    {
        playableDirector.Stop();
    }
}
