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
    private AudioSource audioSourceWaterOngoing;
    private AudioSource audioSourceEntry;
    private float leafRustleMaxVol = 1f;
    [SerializeField] private AudioClip leafRustleClip;
    [SerializeField] private AudioClip leafRustleEntryClip;
    [SerializeField] private AudioClip waterClip;
    [SerializeField] private AudioClip waterEntryClip;
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
    private bool currentlyInWater = false;
    private bool prevInWater = false;

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

        audioSourceWaterOngoing = gameObject.AddComponent<AudioSource>();
        audioSourceWaterOngoing.playOnAwake = true;
        audioSourceWaterOngoing.loop = true;
        audioSourceWaterOngoing.clip = waterClip;
        audioSourceWaterOngoing.volume = 0f;
        audioSourceWaterOngoing.spatialBlend = 1f;
        audioSourceWaterOngoing.minDistance = 3f;
        audioSourceWaterOngoing.maxDistance = 10f;
        audioSourceWaterOngoing.Play();

        audioSourceEntry = gameObject.AddComponent<AudioSource>();
        audioSourceEntry.playOnAwake = false;
        audioSourceEntry.loop = false;
        audioSourceEntry.volume = .5f;
        audioSourceEntry.spatialBlend = 1f;
        audioSourceEntry.minDistance = 3f;
        audioSourceEntry.maxDistance = 10f;
    }

    private void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, Vector3.down, 2f, layerMask)) // detects if the player is over terrain or not.
            onTerrain = true;

        currentlyInBush = Services.PlayerMovement.inBush;
        if (!prevInBush && currentlyInBush)
        {
            audioSourceEntry.pitch = Random.Range(.9f, 1.1f);
            audioSourceEntry.PlayOneShot(leafRustleEntryClip);
        }
        currentlyInWater = Services.PlayerMovement.inWater;
        if (!prevInWater && currentlyInWater)
        {
            audioSourceEntry.pitch = Random.Range(.9f, 1.1f);
            audioSourceEntry.PlayOneShot(waterEntryClip);
        }

        prevInBush = currentlyInBush;
        prevInWater = currentlyInWater;

        if (currentlyInBush && audioSourceLeafRustleOngoing.volume < leafRustleMaxVol)
        {
            audioSourceLeafRustleOngoing.volume += .03f;
        }
        else if (!currentlyInBush && audioSourceLeafRustleOngoing.volume > 0)
        {
            audioSourceLeafRustleOngoing.volume -= .01f;
        }
        if (currentlyInWater && audioSourceWaterOngoing.volume < leafRustleMaxVol)
        {
            audioSourceWaterOngoing.volume += .03f;
        }
        else if (!currentlyInWater && audioSourceWaterOngoing.volume > 0)
        {
            audioSourceWaterOngoing.volume -= .03f;
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
