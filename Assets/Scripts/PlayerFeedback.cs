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
    private AudioSource audioSourceLeafRustle;
    private float leafRustleMaxVol = .5f;
    [SerializeField] private AudioClip leafRustleClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip landClip;
    [SerializeField] private AudioClip[] grassClips;
    [SerializeField] private AudioClip[] dirtClips;
    [SerializeField] private AudioClip[] gravelClips;
    [SerializeField] private float audioVolume = .05f;
    [SerializeField] private ParticleSystem[] particlesGrass = new ParticleSystem[4];
    [SerializeField] private ParticleSystem[] particlesGravel = new ParticleSystem[4];
    [SerializeField] private LayerMask layerMask;
    private bool onTerrain = false;

    private void Awake()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
            audioSources[i].playOnAwake = false;
            audioSources[i].clip = grassClips[0];
            audioSources[i].volume = audioVolume;
            audioSources[i].minDistance = 3f;
            audioSources[i].maxDistance = 10f;
            audioSources[i].spatialBlend = 1f;
        }
        audioSourceLeafRustle = gameObject.AddComponent<AudioSource>();
        audioSourceLeafRustle.playOnAwake = true;
        audioSourceLeafRustle.loop = true;
        audioSourceLeafRustle.clip = leafRustleClip;
        audioSourceLeafRustle.volume = 0f;
        audioSourceLeafRustle.spatialBlend = 1f;
        audioSourceLeafRustle.minDistance = 3f;
        audioSourceLeafRustle.maxDistance = 10f;
        audioSourceLeafRustle.Play();
    }

    private void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, Vector3.down, 2f, layerMask)) // detects if the player is over terrain or not.
            onTerrain = true;

        if (Services.PlayerMovement.movingOnGround && audioSourceLeafRustle.volume < leafRustleMaxVol)
        {
            audioSourceLeafRustle.volume += .01f;
        }
        else if (!Services.PlayerMovement.movingOnGround && audioSourceLeafRustle.volume > 0)
        {
            audioSourceLeafRustle.volume -= .01f;
        }
    }

    public void StepEvent(int foot)
    {
        if (onTerrain)
        {
            audioSources[nextSource].clip = grassClips[Random.Range(0, grassClips.Length)];
            particlesGrass[foot].Play();
        }
        else
        {
            audioSources[nextSource].clip = gravelClips[Random.Range(0, gravelClips.Length)];
            particlesGravel[foot].Play();

        }
        audioSources[nextSource].Play();
        IncrementNextSource();

    }

    private void IncrementNextSource() => nextSource = (1 + nextSource) % audioSources.Length;

    public void JumpEvent()
    {
        audioSources[nextSource].clip = jumpClip;
        audioSources[nextSource].Play();
        IncrementNextSource();

        particlesGrass[0].Play();
        particlesGrass[2].Play();
    }

    public void LandEvent1()
    {
        audioSources[nextSource].clip = landClip;
        audioSources[nextSource].Play();
        IncrementNextSource();

        particlesGrass[1].Play();
        particlesGrass[3].Play();
    }

    public void LandEvent2()
    {
        audioSources[nextSource].clip = landClip;
        audioSources[nextSource].Play();
        IncrementNextSource();

        particlesGrass[0].Play();
        particlesGrass[2].Play();
    }
}
