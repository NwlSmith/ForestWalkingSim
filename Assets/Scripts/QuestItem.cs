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
    #region Const Strings.
    private readonly int _held = Animator.StringToHash("Held");
    #endregion

    public enum QuestItemEnum { Seed, Soil, Rain, None };

    public QuestItemEnum itemEnum;

    private Animator animator;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
    }
    // Need to implement non-droppable code.

    public override void DetachFromTransform()
    {
        transform.parent = transform.root.parent;
        beingHeld = false;
        animator.SetBool(_held, false);
    }

    public override void AttachToTransform(Transform newParent)
    {
        transform.position = newParent.position;
        transform.rotation = newParent.rotation;
        transform.parent = newParent;
        beingHeld = true;
        animator.SetBool(_held, true);

    }
}
