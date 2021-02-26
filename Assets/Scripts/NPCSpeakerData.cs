using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SpeakerData", menuName = "SpeakerData")]
public class NPCSpeakerData : ScriptableObject
{
    public string SpeakerName = "";
    public Color SpeakerColor = Color.white;
    public AudioClip[] SpeakerSounds;

    public AudioClip GetAudioClip()
    {
        return SpeakerSounds[Random.Range(0, SpeakerSounds.Length)];
    }
}
