using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurtleRunSounds : MonoBehaviour
{
    [SerializeField] private AudioClip[] _acs = new AudioClip[3];
    [SerializeField] private GameObject particles;
    private AudioSource _as;
    private bool _running = false;
    private void Awake()
    {
        _as = gameObject.AddComponent<AudioSource>();
        _as.playOnAwake = false;
        _as.minDistance = 5f;
        _as.maxDistance = 10f;
        _as.spatialBlend = 1f;
        particles.SetActive(false);
    }

    public void StartRunning()
    {
        if (_running) return;

        _running = true;
        particles.SetActive(true);
    }

    public void StopRunning()
    {
        if (!_running) return;

        _running = false;
        particles.SetActive(false);
    }

    public void PlaySound()
    {
        _as.pitch = Random.Range(.9f, 1.1f);
        _as.PlayOneShot(_acs[Random.Range(0, 3)]);
    }
}
