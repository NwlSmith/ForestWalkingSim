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
    private AudioSource audioSourceSteps;
    private AudioSource audioSourceLeafRustleOngoing;
    private AudioSource audioSourceLeafRustleEntry;
    private float leafRustleMaxVol = 1f;
    [SerializeField] private AudioClip leafRustleClip;
    [SerializeField] private AudioClip leafRustleEntryClip;
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
    private bool currentlyInBush = false;
    private bool prevInBush = false;

    private void Awake()
    {
        audioSourceSteps = gameObject.AddComponent<AudioSource>();
        audioSourceSteps.playOnAwake = false;
        audioSourceSteps.clip = grassClips[0];
        audioSourceSteps.volume = audioVolume;
        audioSourceSteps.minDistance = 3f;
        audioSourceSteps.maxDistance = 10f;
        audioSourceSteps.spatialBlend = 1f;

        audioSourceLeafRustleOngoing = gameObject.AddComponent<AudioSource>();
        audioSourceLeafRustleOngoing.playOnAwake = true;
        audioSourceLeafRustleOngoing.loop = true;
        audioSourceLeafRustleOngoing.clip = leafRustleClip;
        audioSourceLeafRustleOngoing.volume = 0f;
        audioSourceLeafRustleOngoing.spatialBlend = 1f;
        audioSourceLeafRustleOngoing.minDistance = 3f;
        audioSourceLeafRustleOngoing.maxDistance = 10f;
        audioSourceLeafRustleOngoing.Play();

        audioSourceLeafRustleEntry = gameObject.AddComponent<AudioSource>();
        audioSourceLeafRustleEntry.playOnAwake = false;
        audioSourceLeafRustleEntry.loop = false;
        audioSourceLeafRustleEntry.clip = leafRustleEntryClip;
        audioSourceLeafRustleEntry.volume = 1;
        audioSourceLeafRustleEntry.spatialBlend = 1f;
        audioSourceLeafRustleEntry.minDistance = 3f;
        audioSourceLeafRustleEntry.maxDistance = 10f;
    }

    private void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, Vector3.down, 2f, layerMask)) // detects if the player is over terrain or not.
            onTerrain = true;

        currentlyInBush = Services.PlayerMovement.inBush;
        if (!prevInBush && currentlyInBush)
        {
            audioSourceLeafRustleEntry.pitch = Random.Range(.9f, 1.1f);
            audioSourceLeafRustleEntry.Play();
        }

        prevInBush = currentlyInBush;

        if (currentlyInBush && audioSourceLeafRustleOngoing.volume < leafRustleMaxVol)
        {
            audioSourceLeafRustleOngoing.volume += .03f;
        }
        else if (!currentlyInBush && audioSourceLeafRustleOngoing.volume > 0)
        {
            audioSourceLeafRustleOngoing.volume -= .01f;
        }
    }

    public void StepEvent(int foot)
    {
        audioSourceSteps.pitch = Random.Range(.9f, 1.1f);
        if (onTerrain)
        {
            audioSourceSteps.PlayOneShot(grassClips[Random.Range(0, grassClips.Length)]);
            particlesGrass[foot].Play();
        }
        else
        {
            audioSourceSteps.PlayOneShot(gravelClips[Random.Range(0, gravelClips.Length)]);
            particlesGravel[foot].Play();
        }
    }

    public void JumpEvent()
    {
        audioSourceSteps.PlayOneShot(jumpClip);

        particlesGrass[0].Play();
        particlesGrass[2].Play();
    }

    public void LandEvent1()
    {
        audioSourceSteps.PlayOneShot(landClip);

        particlesGrass[1].Play();
        particlesGrass[3].Play();
    }

    public void LandEvent2()
    {
        audioSourceSteps.PlayOneShot(landClip);

        particlesGrass[0].Play();
        particlesGrass[2].Play();
    }
}
