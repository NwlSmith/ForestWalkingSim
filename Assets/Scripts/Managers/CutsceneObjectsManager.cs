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
        StartCoroutine(TransitionEnum());
    }

    private IEnumerator TransitionEnum()
    {
        foreach (Animator anim in animators)
        {
            anim.SetTrigger(Str.cutsceneTrans);
            yield return new WaitForSeconds(.065f);
        }
    }
}
