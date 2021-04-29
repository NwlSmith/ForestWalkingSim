using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneObjectsManager : MonoBehaviour
{

    private Animator[] animators;

    [SerializeField] private GameObject[] smokeParticles;

    private int phase = 0;

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
        float scale = (2 - phase) / 2;
        foreach (GameObject emitter in smokeParticles)
        {
            emitter.transform.localScale = new Vector3(scale, scale, scale);
        }
        phase++;

        foreach (Animator anim in animators)
        {
            anim.SetTrigger(Str.cutsceneTrans);
            yield return new WaitForSeconds(.07f);
        }
    }
}
