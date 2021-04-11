using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLanding : StateMachineBehaviour
{
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.8f)
        {
            animator.SetBool("IsLanding", false);
        }
    }
}
