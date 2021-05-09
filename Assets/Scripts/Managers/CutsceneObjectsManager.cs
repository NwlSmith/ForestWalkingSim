using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CutsceneObjectsManager : MonoBehaviour
{

    private Animation[] batch1Animators;
    private Animation[] batch2Animators;
    private Animation[] batch3Animators;

    [SerializeField] private GameObject[] smokeParticles;

    [SerializeField] private GameObject playerEndModel;
    [SerializeField] private GameObject[] npcModels;
    [SerializeField] private GameObject spiritModel;

    private int phase = 2;

    private void Awake()
    {
        batch1Animators = transform.GetChild(0).GetComponentsInChildren<Animation>();
        batch2Animators = transform.GetChild(1).GetComponentsInChildren<Animation>();
        batch3Animators = transform.GetChild(2).GetComponentsInChildren<Animation>();

        foreach (Animation anim in batch1Animators)
            anim.gameObject.SetActive(false);

        foreach (Animation anim in batch2Animators)
            anim.gameObject.SetActive(false);

        foreach (Animation anim in batch3Animators)
            anim.gameObject.SetActive(false);

        playerEndModel.SetActive(false);
        foreach (var npc in npcModels)
        {
            npc.SetActive(false);
        }
    }

    public void Transition() => StartCoroutine(TransitionEnum());

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
                phase--;
                foreach (Animation anim in batch1Animators)
                {
                    anim.gameObject.SetActive(true);
                    anim.Play();
                    yield return new WaitForSeconds(.05f);
                }
                foreach (Animation anim in batch1Animators)
                    anim.enabled = false;
                break;
            case 1:
                phase--;
                foreach (Animation anim in batch2Animators)
                {
                    anim.gameObject.SetActive(true);
                    anim.Play();
                    yield return new WaitForSeconds(.07f);
                }
                foreach (Animation anim in batch2Animators)
                    anim.enabled = false;

                break;
            case 0:
                phase--;
                batch3Animators[0].gameObject.SetActive(true);
                batch3Animators[0].Play();
                yield return new WaitForSeconds(2);
                for (int i = 1; i < batch3Animators.Length; i++)
                {
                    batch3Animators[i].gameObject.SetActive(true);
                    batch3Animators[i].Play();
                    yield return new WaitForSeconds(1);
                }
                foreach (Animation anim in batch3Animators)
                    anim.enabled = false;
                break;
            default:
                phase--;
                break;
        }
    }

    public void EndNPCs() => StartCoroutine(EndNPCSEnum());

    private IEnumerator EndNPCSEnum()
    {
        playerEndModel.SetActive(true);
        spiritModel.SetActive(false);

        List<Animator> npcAnims = new List<Animator>();

        foreach (var npc in npcModels)
        {
            npc.SetActive(true);
            Animator anim = npc.GetComponentInChildren<Animator>();
            anim.SetTrigger(Str.Talk);
            npcAnims.Add(anim);
        }

        while (true)
        {
            yield return new WaitForSeconds(2f);

            foreach (var anim in npcAnims)
            {
                anim.SetTrigger(Str.Talk);
            }
        }
    }
}
