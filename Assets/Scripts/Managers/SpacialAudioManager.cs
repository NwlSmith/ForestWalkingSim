using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 3/27/2021
 * Description: Plays sync'd music and ambient audio for different areas.
 * 
 * Both clips will play at the same time, but only 1 will have any volume.
 */
public class SpacialAudioManager : MonoBehaviour
{
    private Dictionary<AreaEnum, AudioClip> areaToClip = new Dictionary<AreaEnum, AudioClip>();
    public enum AreaEnum { Start, Heart, Warbler, Frogs, Turtle };
    private AreaEnum _currentArea = AreaEnum.Start;
    private int _curASIndex = 0;
    [SerializeField] private float _musicVolume = .3f;
    private AudioSource[] _audioSources = new AudioSource[2];
    [SerializeField] private AudioClip _startSong;
    [SerializeField] private AudioClip _heartSong;
    [SerializeField] private AudioClip _warblerSong;
    [SerializeField] private AudioClip _frogSong;
    [SerializeField] private AudioClip _turtleSong;

    [SerializeField] private float _fadeDuration = 2f;

    private TaskManager _taskManager = new TaskManager();

    private void Awake()
    {
        areaToClip.Add(AreaEnum.Start, _startSong);
        areaToClip.Add(AreaEnum.Heart, _heartSong);
        areaToClip.Add(AreaEnum.Warbler, _warblerSong);
        areaToClip.Add(AreaEnum.Frogs, _frogSong);
        areaToClip.Add(AreaEnum.Turtle, _turtleSong);

        for (int i = 0; i < _audioSources.Length; i++)
        {
            _audioSources[i] = gameObject.AddComponent<AudioSource>();
            _audioSources[i].playOnAwake = false;
            _audioSources[i].loop = true;
            _audioSources[i].clip = _startSong;
            _audioSources[i].volume = _musicVolume;
        }

        _audioSources[0].Play();
    }

    private void Update()
    {
        _taskManager.Update();
    }

    public void FadeIn(AreaEnum areaEnum)
    {
        if (_currentArea == areaEnum)
            return;
        _taskManager.Do(FadeOutTask());
        _taskManager.Do(FadeInTask(areaEnum));
    }

    private int OtherASIndex => _curASIndex == 1 ? 0 : 1;

    private AudioSource OtherAS => _audioSources[OtherASIndex];

    private void FadeOutAll()
    {
        _taskManager.Do(FadeOutTask());
        _curASIndex = OtherASIndex;
        _taskManager.Do(FadeOutTask());
    }

    private DelegateTask FadeOutTask()
    {
        float _elapsedTime = 0f;
        AudioSource _curAS = _audioSources[_curASIndex];
        return new DelegateTask(
            () => { },
            () => {
                _elapsedTime += Time.deltaTime;
                _curAS.volume = Mathf.Lerp(_musicVolume, 0, 1 - _elapsedTime / _fadeDuration);
                return _elapsedTime >= _fadeDuration;
            }
        );
    }

    private DelegateTask FadeInTask(AreaEnum areaEnum)
    {
        float _elapsedTime = 0f;
        _curASIndex = OtherASIndex;
        AudioSource _curAS = _audioSources[_curASIndex];
        _curAS.clip = areaToClip[areaEnum];
        _curAS.Play();
        _curAS.time = OtherAS.time % _curAS.clip.length;
        _currentArea = areaEnum;
        return new DelegateTask(
            () => { },
            () => {
                _elapsedTime += Time.deltaTime;
                _curAS.volume = _elapsedTime / _fadeDuration;
                _curAS.volume = Mathf.Lerp(0, _musicVolume, _elapsedTime / _fadeDuration);
                return _elapsedTime >= _fadeDuration;
            }
        );
    }

}
