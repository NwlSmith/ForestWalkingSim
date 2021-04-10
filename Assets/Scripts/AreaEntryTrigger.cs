using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaEntryTrigger : MonoBehaviour
{
    [SerializeField] SpacialAudioManager.AreaEnum _area = SpacialAudioManager.AreaEnum.Start;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(Str.PlayerTag)) return;

        Services.SpacialAudioManager.FadeIn(_area);
    }
}
