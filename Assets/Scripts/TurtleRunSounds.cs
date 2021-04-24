using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurtleRunSounds : MonoBehaviour
{
    private int nextSource = 0;
    [SerializeField] private AudioClip[] _acs = new AudioClip[3];
    [SerializeField] private GameObject particles;
    private AudioSource[] _as = new AudioSource[4];
    private bool _running = false;
    private void Awake()
    {
        for (int i = 0; i < 4; i++)
        {
            _as[i] = gameObject.AddComponent<AudioSource>();
            _as[i].playOnAwake = false;
            _as[i].minDistance = 5f;
            _as[i].maxDistance = 10f;
            _as[i].spatialBlend = 1f;
        }
        particles.SetActive(false);
    }

    private void IncrementNextSource() => nextSource = (1 + nextSource) % _as.Length;

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
        _as[nextSource].clip = _acs[Random.Range(0, 3)];
        _as[nextSource].pitch = Random.Range(.9f, 1.1f);
        _as[nextSource].Play();
        IncrementNextSource();
    }
}
