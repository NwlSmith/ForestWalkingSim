using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneObjectsManager : MonoBehaviour
{

    private Animator[] animators;

    [SerializeField] private GameObject[] smokeParticles;

    private void Awake()
    {
        animators = GetComponentsInChildren<Animator>();
    }

    public void Transition()
    {
        foreach (Animator anim in animators)
        {
            anim.SetTrigger(Str.cutsceneTrans);
        }
    }

    private IEnumerator TransitionEnum()
    {
        foreach (Animator anim in animators)
        {
            anim.SetTrigger(Str.cutsceneTrans);
            yield return new WaitForSeconds(Random.Range(.05f, .3f));
        }
    }
}
