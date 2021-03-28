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
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip landClip;
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
            audioSources[i].maxDistance = 10f;
            audioSources[i].spatialBlend = 0f;
        }
    }

    public void StepEvent(int foot)
    {
        audioSources[nextSource].clip = grassClips[Random.Range(0, grassClips.Length)];
        audioSources[nextSource].Play();
        IncrementNextSource();

        particles[foot].Play();
    }

    private void IncrementNextSource() => nextSource = (1 + nextSource) % audioSources.Length;

    public void JumpEvent()
    {
        audioSources[nextSource].clip = jumpClip;
        audioSources[nextSource].Play();
        IncrementNextSource();

        particles[0].Play();
        particles[2].Play();
    }

    public void LandEvent1()
    {
        audioSources[nextSource].clip = landClip;
        audioSources[nextSource].Play();
        IncrementNextSource();

        particles[1].Play();
        particles[3].Play();
    }

    public void LandEvent2()
    {
        audioSources[nextSource].clip = landClip;
        audioSources[nextSource].Play();
        IncrementNextSource();

        particles[0].Play();
        particles[2].Play();
    }
}
