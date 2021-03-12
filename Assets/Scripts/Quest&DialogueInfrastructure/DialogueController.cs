using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using TMPro;
/*
 * Creator: Nate Smith
 * Creation Date: 2/25/2021
 * Description: Dialogue controller class.
 * 
 * Interfaces between our code and Yarn for dialogue.
 */
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

    private InMemoryVariableStorage _inMemoryVariableStorage;
    public InMemoryVariableStorage InMemoryVariableStorage
    {
        get
        {
            Debug.Assert(_inMemoryVariableStorage != null);
            return _inMemoryVariableStorage;
        }
        private set => _inMemoryVariableStorage = value;
    }

    private NPC _curNPC;
    private int _curMultiNPCNum = 0;

    private bool _isPlayerSpeaking = false;

    [SerializeField] private NPCSpeakerData _playerSpeakerData;
    [SerializeField] private TMP_Text _speakerText;
    [SerializeField] private TMP_Text _dialogueText;
    private AudioSource _audioSource;

    private void Awake()
    {
        DialogueUIManager = GetComponent<DialogueUI>();
        DialogueRunner = GetComponent<DialogueRunner>();
        InMemoryVariableStorage = GetComponent<InMemoryVariableStorage>();
        _audioSource = GetComponent<AudioSource>();

        AddYarnCommands();
    }

    // Adds a function to the command handler in Yarn.
    private void AddYarnCommands()
    {
        DialogueRunner.AddCommandHandler("NPCSpeak", NPCSpeak);
        DialogueRunner.AddCommandHandler("PlayerSpeak", PlayerSpeak);
    }

    // Injects a Yarn Dialogue file into Yarn.
    public void AddYarnDialogue(YarnProgram yarnDialogue)
    {
        Debug.Log($"Adding dialogue: {yarnDialogue.name}");
        DialogueRunner.Add(yarnDialogue);
    }

    public void EnterDialogue()
    {
        _curNPC = Services.NPCInteractionManager.closestNPC;
        Debug.Log($"Starting dialogue: {_curNPC.YarnStartNode}");
        DialogueRunner.StartDialogue(_curNPC.YarnStartNode);
        NPCSpeak(null);
    }

    private void NPCSpeak(string [] parameters)
    {
#if !UNITY_EDITOR
        if (_curNPC.name.Equals("Turtle (home)") || _curNPC.name.Equals("Turtle (racetrack)"))
            _yarnDialogueUI.textSpeed = .1f;
        else
            _yarnDialogueUI.textSpeed = .025f;
#endif

        // Accommodate more than 1 npc, AND just 1 npc.
        _curMultiNPCNum = 0;
        MultiNPC multiNPC = _curNPC.GetComponent<MultiNPC>();
        if (multiNPC != null)
        {
            if (parameters != null && parameters.Length > 0)
            {
                Debug.Log($"parameters on {_curNPC.name} = {parameters[0]}");
                _curMultiNPCNum = int.Parse(parameters[0]);
                Services.CameraManager.SetTargetNPC(multiNPC.npcs[_curMultiNPCNum]);
            }
            else
                Debug.Log($"no parameters on {_curNPC.name}");
        }

        _speakerText.text = _curNPC.GetNPCSpeakerData(_curMultiNPCNum).SpeakerName;
        _dialogueText.color = _curNPC.GetNPCSpeakerData(_curMultiNPCNum).SpeakerColor;
        Services.CameraManager.PlayerCameraView();
        _isPlayerSpeaking = false;
    }

    private void PlayerSpeak(string[] parameters)
    {
        _yarnDialogueUI.textSpeed = .025f;
        // possibly fix character speaking problem?
        Services.CameraManager.SetTargetNPC(_curNPC);

        _speakerText.text = _playerSpeakerData.SpeakerName;
        _dialogueText.color = _playerSpeakerData.SpeakerColor;
        Services.CameraManager.NPCCameraView();
        _isPlayerSpeaking = true;
    }

    public void OnLineStart()
    {
        if (_isPlayerSpeaking)
        {
            _audioSource.clip = _playerSpeakerData.GetAudioClip();
            Services.PlayerAnimation.Talk();
        }
        else
        {
            _audioSource.clip = _curNPC.GetNPCSpeakerData(_curMultiNPCNum).GetAudioClip();
            _curNPC.Speak(_curMultiNPCNum);
        }

        _audioSource.pitch = Random.Range(.9f, 1.1f);
        _audioSource.Play();
    }
}
