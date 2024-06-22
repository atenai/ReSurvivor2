using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandIK : MonoBehaviour
{
    //左手の位置
    [SerializeField] Transform leftHand;
    Animator animator;

    void Start()
    {
        animator = this.GetComponent<Animator>();
    }

    void OnAnimatorIK()
    {
        //キャラクターの左手の位置と角度を合わせる

        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);

        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHand.position);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHand.rotation);
    }
}
