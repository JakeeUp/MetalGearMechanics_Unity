using System.Collections;
using UnityEngine;

public class ChangeBoolStatus : StateMachineBehaviour
{
    public string boolName;
    public bool status;
    public bool resetOnExit;
    public float delay = 0f;
    public bool isPlayer;

    MonoBehaviour coroutineHost;
    int boolHash;
    bool hashCached;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        CacheHash();

        if (delay <= 0)
        {
            animator.SetBool(boolHash, status);
            return;
        }

        if (coroutineHost == null)
        {
            coroutineHost = isPlayer
                ? (MonoBehaviour)animator.GetComponentInParent<Controller>()
                : animator.GetComponentInParent<AIController>();
        }

        coroutineHost.StartCoroutine(DelayedOpen(delay, animator, status));
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (resetOnExit)
        {
            CacheHash();
            animator.SetBool(boolHash, !status);
        }
    }

    void CacheHash()
    {
        if (hashCached)
            return;

        boolHash = Animator.StringToHash(boolName);
        hashCached = true;
    }

    IEnumerator DelayedOpen(float d, Animator animator, bool targetStatus)
    {
        yield return new WaitForSeconds(d);
        animator.SetBool(boolHash, targetStatus);
    }
}
