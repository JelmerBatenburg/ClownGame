using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class IKArmBones : MonoBehaviour
{
    public Transform rightHandLocation,leftHandLocation;
    private Animator animator;
    public Transform lookLocation;

    public bool useIK;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnAnimatorIK()
    {
        if (animator && useIK)
        {
            if (lookLocation != null)
            {
                animator.SetLookAtWeight(1);
                animator.SetLookAtPosition(lookLocation.position);
            }

            if (rightHandLocation != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandLocation.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandLocation.rotation);
            }

            if (leftHandLocation != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandLocation.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandLocation.rotation);
            }
        }
        else if(animator)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetLookAtWeight(0);
        }
    }
}
