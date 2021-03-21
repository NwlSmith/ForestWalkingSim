using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 3/10/2021
 * Description: UI button sound manager.
 */
public class UISound : MonoBehaviour
{

    [SerializeField] private AudioClip _positive;
    [SerializeField] private AudioClip _neutral;
    [SerializeField] private AudioClip _somber;
    private AudioSource _audioSource;

    // 0 = Positive, 1 = Neutral, 2 = Somber
    private AudioClip[] _buttonToClip = new AudioClip[3];

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _buttonToClip[0] = _positive;
        _buttonToClip[1] = _neutral;
        _buttonToClip[2] = _somber;
    }

    public void PlayUISound(int i)
    {
        _audioSource.clip = _buttonToClip[i];
        //_audioSource.pitch = Random.Range(.9f, 1.1f);
        _audioSource.Play();
    }
}
