using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/15/2021
 * Description: NPC class script.
 * 
 * Holds variables and functions for an instance of an NPC.
 * 
 * Maybe make NPC face toward player on dialogue entry?
 */
public class NPC : MonoBehaviour
{

    #region Const Strings.
    private readonly int _talk = Animator.StringToHash("Talk");
    private readonly int _inConvo = Animator.StringToHash("InConversation");
    #endregion

    public string YarnStartNode;
    public YarnProgram YarnDialogue;
    public NPCSpeakerData NPCSpeakerData;
    public Transform dialoguePos;

    public Transform npcCameraViewPosition;// { get; private set; }
    [SerializeField] protected Transform playerCameraLookAtPosition;// { get; private set; }

    [SerializeField] private GameObject _model;
    [SerializeField] private Transform _dialogueEnterPrompt;
    private Quaternion _initRot;
    private Animator _anim;

    private void Awake()
    {
        _initRot = _model.transform.rotation;
        _anim = GetComponentInChildren<Animator>();
        if (_dialogueEnterPrompt)
            _dialogueEnterPrompt.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (GetComponent<MultiNPC>() || !GetComponentInParent<MultiNPC>())
            Services.DialogueController.AddYarnDialogue(YarnDialogue);
    }

    private void OnEnable()
    {
        if (_initRot == null)
        {
            Logger.Warning("InitRot was null OnEnable");
            _initRot = _model.transform.rotation;
        }
        if (_anim == null)
        {
            Logger.Warning("_anim was null OnEnable");
            _anim = GetComponentInChildren<Animator>();
        }
    }

    public void DisplayDialoguePrompt()
    {
        Logger.Debug("Displaying dialogue prompt from NPC");
        Services.UIManager.DisplayDialogueEnterPrompt(this);
        //_dialogueEnterPrompt.gameObject.SetActive(true);
    }

    public void PositionDialoguePrompt() => Services.UIManager.PositionDialogueEntryPrompt(_dialogueEnterPrompt.position);

    public void HideDialoguePrompt()
    {
        Logger.Debug("HideDialoguePrompt from NPC");
        Services.UIManager.HideDialogueEnterPrompt();
        //_dialogueEnterPrompt.gameObject.SetActive(false);
    }

    public virtual void EnterDialogue(Transform playerPos)
    {
        Vector3 lookPos = playerPos.position - transform.position;
        lookPos.y = 0;
        _model.transform.rotation = Quaternion.LookRotation(lookPos);
        if (!_anim)
            _anim = GetComponentInChildren<Animator>();
        _anim.SetBool(_inConvo, true);
    }

    public virtual void ExitDialogue()
    {
        _model.transform.rotation = _initRot;
        _anim.SetBool(_inConvo, false);
    }

    public virtual void Speak(int npcNum = 0) => _anim.SetTrigger(_talk);

    public virtual NPCSpeakerData GetNPCSpeakerData(int npcNum = 0) => NPCSpeakerData;

    public virtual Transform GetPlayerCameraLookAtPosition(int num = 0) => playerCameraLookAtPosition;
}
