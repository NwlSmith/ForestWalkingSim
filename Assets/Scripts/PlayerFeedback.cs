using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 3/9/2021
 * Description: Foot sound trigger.
 * 
 * Called during walking animations.
 */
public class PlayerFeedback : MonoBehaviour
{
    public int nextSource = 0;
    private AudioSource[] audioSources = new AudioSource[4];
    [SerializeField] private AudioClip[] grassClips;
    [SerializeField] private AudioClip[] dirtClips;
    [SerializeField] private AudioClip[] gravelClips;
    [SerializeField] private float audioVolume = .05f;
    [SerializeField] private ParticleSystem[] particles = new ParticleSystem[4];

    private void Awake()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
            audioSources[i].playOnAwake = false;
            audioSources[i].clip = grassClips[0];
            audioSources[i].volume = audioVolume;
        }
    }

    public void StepEvent(int foot)
    {
        audioSources[nextSource].clip = grassClips[Random.Range(0, grassClips.Length)];
        audioSources[nextSource].Play();
        nextSource++;
        nextSource = nextSource % audioSources.Length;

        particles[foot].Play();
    }
}
