using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepSound : MonoBehaviour
{
    public int nextSource = 0;
    private AudioSource[] audioSources = new AudioSource[4];
    [SerializeField] private AudioClip[] grassClips;
    [SerializeField] private AudioClip[] dirtClips;
    [SerializeField] private AudioClip[] gravelClips;

    private void Awake()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
            audioSources[i].playOnAwake = false;
            audioSources[i].clip = grassClips[0];
            audioSources[i].volume = .2f;
        }
    }

    public void StepEvent()
    {
        audioSources[nextSource].clip = grassClips[Random.Range(0, grassClips.Length)];
        audioSources[nextSource].Play();
        nextSource++;
        nextSource = nextSource % audioSources.Length;
    }
}
