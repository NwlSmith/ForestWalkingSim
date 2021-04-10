using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/15/2021
 * Description: Collider for entering dialogue with an NPC.
 * 
 * Can be placed away from the NPC.
 */
public class NPCCollider : MonoBehaviour
{

    #region Const Strings.
    private static readonly int _visible = Animator.StringToHash("Visible");
    #endregion

    private NPC parentNPC;

    private float _initRadius;
    private float _enteredMultiplier = 1.2f;

    private CapsuleCollider _collider;
    private Animator _animator;

    private void Awake()
    {
        parentNPC = GetComponentInParent<NPC>();
        _collider = GetComponent<CapsuleCollider>();
        _initRadius = _collider.radius;
        _animator = GetComponentInParent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Services.PlayerTag)) EncounteredPlayer();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(Services.PlayerTag)) parentNPC.PositionDialoguePrompt();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Services.PlayerTag)) PlayerLeft();
    }

    private void EncounteredPlayer()
    {
        NPCInteractionManager.PlayerEncounteredNPC(parentNPC);
        _collider.radius = _initRadius * _enteredMultiplier;
    }

    private void PlayerLeft()
    {
        NPCInteractionManager.PlayerLeftNPC();
        _collider.radius = _initRadius;
    }

    public void Appear() => _animator.SetBool(_visible, true);

    public void Disappear() => _animator.SetBool(_visible, false);
}
