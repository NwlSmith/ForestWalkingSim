using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/25/2021
 * Description: NPC speaker data.
 * 
 * Holds variables for dialogue with NPC's and player.
 */
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
