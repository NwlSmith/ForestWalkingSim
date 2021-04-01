using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/14/2021
 * Description: Holdable item base script.
 * 
 * When the player enters the trigger area of the item, the item prompts the player to pick it up through the UI.
 * 
 * The player can press a button to pick up the item, at which point the item will go to the player's mouth position. Call a script in player to manage this.
 * 
 * The player can press the button again to drop the item, which will reenable physics for the item.
 * 
 * If the player drops the item in a quest area, trigger something.
 * 
 * 
 */
public class HoldableItem : MonoBehaviour
{
    public bool holdable = true;
    public bool beingHeld = false;

    public Rigidbody rb { get; private set; }

    protected Vector3 pickupPromptOffset = new Vector3(0, 1, 0);

    [SerializeField] protected static LayerMask layerMask = 17;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public Vector3 PickupPromptOffset => transform.position + pickupPromptOffset;

    public virtual void DetachFromTransform()
    {
        rb.isKinematic = false;
        transform.parent = transform.root.parent;
        beingHeld = false;

        ResetPosition();
    }

    protected void ResetPosition()
    {
        transform.rotation = Quaternion.identity;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 5f, layerMask, QueryTriggerInteraction.Ignore))
        {
            transform.position = hit.point + Vector3.up / 2;
        }
        else
        {
            Logger.Warning($"Item {name} was dropped, raycast did not hit the ground.");
        }
    }

    public virtual void AttachToTransform(Transform newParent)
    {
        rb.isKinematic = true;
        transform.position = newParent.position;
        transform.rotation = newParent.rotation;
        transform.parent = newParent;
        beingHeld = true;
    }
}
