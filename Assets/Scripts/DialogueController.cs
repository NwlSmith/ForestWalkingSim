using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using TMPro;


public class DialogueController : MonoBehaviour
{


    private DialogueUI _yarnDialogueUI;
    public DialogueUI DialogueUIManager
    {
        get
        {
            Debug.Assert(_yarnDialogueUI != null);
            return _yarnDialogueUI;
        }
        private set => _yarnDialogueUI = value;
    }

    private DialogueRunner _yarnDialogueRunner;
    public DialogueRunner DialogueRunner
    {
        get
        {
            Debug.Assert(_yarnDialogueRunner != null);
            return _yarnDialogueRunner;
        }
        private set => _yarnDialogueRunner = value;
    }

    private NPC _curNPC;

    [SerializeField] private NPCSpeakerData _playerSpeakerData;
    [SerializeField] private TMP_Text _speakerText;
    [SerializeField] private TMP_Text _dialogueText;
    private AudioSource _audioSource;

    private void Awake()
    {
        DialogueUIManager = GetComponent<DialogueUI>();
        DialogueRunner = GetComponent<DialogueRunner>();
        _audioSource = GetComponent<AudioSource>();

        DialogueRunner.AddCommandHandler("NPCSpeak", NPCSpeak);
        DialogueRunner.AddCommandHandler("PlayerSpeak", PlayerSpeak);
    }

    public void AddYarnDialogue(YarnProgram yarnDialogue)
    {
        DialogueRunner.Add(yarnDialogue);
    }

    public void EnterDialogue()
    {
        _curNPC = Services.NPCInteractionManager.closestNPC;
        DialogueRunner.StartDialogue(_curNPC.YarnStartNode);
        _speakerText.text = _curNPC.NPCSpeakerData.SpeakerName;
        _dialogueText.color = _curNPC.NPCSpeakerData.SpeakerColor;
        Services.CameraManager.PlayerCameraView();
    }

    private void NPCSpeak(string [] parameters)
    {
        _speakerText.text = _curNPC.NPCSpeakerData.SpeakerName;
        _dialogueText.color = _curNPC.NPCSpeakerData.SpeakerColor;
        _audioSource.clip = _curNPC.NPCSpeakerData.GetAudioClip();
        _audioSource.Play();
        Services.CameraManager.PlayerCameraView();
    }

    private void PlayerSpeak(string[] parameters)
    {
        _speakerText.text = _playerSpeakerData.SpeakerName;
        _dialogueText.color = _playerSpeakerData.SpeakerColor;
        _audioSource.clip = _playerSpeakerData.GetAudioClip();
        _audioSource.pitch = Random.Range(.9f, 1.1f);
        _audioSource.Play();
        Services.CameraManager.NPCCameraView();
    }
}
