/*
 * Creator: Nate Smith
 * Creation Date: 2/28/2021
 * Description: Holdable Quest item script.
 * 
 * Acts as a HoldableItem, but you can only drop it if within a certain specified area, and doing so will trigger a script.
 */
using UnityEngine;

public class QuestItem : HoldableItem
{

    public enum QuestItemEnum { Seed, Soil, Rain, None };

    public QuestItemEnum itemEnum;

    private Animator _animator;
    private FMODUnity.StudioEventEmitter _emitter;

    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponent<Animator>();
        _emitter = GetComponent<FMODUnity.StudioEventEmitter>();
    }
    // Need to implement non-droppable code.

    public override void DetachFromTransform()
    {
        transform.parent = transform.root.parent;
        beingHeld = false;
        _animator.SetBool(Str.Held, false);
        ResetPosition();
    }

    public override void AttachToTransform(Transform newParent)
    {
        transform.position = newParent.position;
        transform.rotation = newParent.rotation;
        transform.parent = newParent;
        beingHeld = true;
        _animator.SetBool(Str.Held, true);
        _emitter.Stop();
    }

    public void Disappear()
    {
        transform.localScale = Vector3.zero;
        GetComponent<Collider>().enabled = false;
        _animator.enabled = false;
    }

}
