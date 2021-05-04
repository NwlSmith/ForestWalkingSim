using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneObjectsManager : MonoBehaviour
{

    private Animator[] batch1Animators;
    private Animator[] batch2Animators;
    private Animator[] batch3Animators;

    [SerializeField] private GameObject[] smokeParticles;

    private int phase = 2;

    private void Awake()
    {
        batch1Animators = transform.GetChild(0).GetComponentsInChildren<Animator>();
        batch2Animators = transform.GetChild(1).GetComponentsInChildren<Animator>();
        batch3Animators = transform.GetChild(2).GetComponentsInChildren<Animator>();
    }

    public void Transition()
    {
        StartCoroutine(TransitionEnum());
    }

    private IEnumerator TransitionEnum()
    {
        float scale = phase / 2;
        if (scale < 0)
            scale = 0;
        foreach (GameObject emitter in smokeParticles)
        {
            emitter.transform.localScale = new Vector3(scale, scale, scale);
        }

        switch (phase)
        {
            case 2:
                foreach (Animator anim in batch1Animators)
                {
                    anim.SetTrigger(Str.cutsceneTrans);
                    yield return new WaitForSeconds(.07f);
                }
                break;
            case 1:
                foreach (Animator anim in batch2Animators)
                {
                    anim.SetTrigger(Str.cutsceneTrans);
                    yield return new WaitForSeconds(.07f);
                }
                break;
            case 0:
                foreach (Animator anim in batch3Animators)
                {
                    anim.SetTrigger(Str.cutsceneTrans);
                    yield return new WaitForSeconds(.07f);
                }
                break;
            default:
                break;
        }
        phase--;
    }
}
