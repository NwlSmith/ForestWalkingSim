using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/14/2021
 * Description: Handler for item holding on the player.
 * 
 * When a holdable item enters the trigger area, they are prompted to pick up the item.
 * 
 * If they press the button they attach the closest item to a pre-defined place.
 * 
 * If triggered by an item receptacle, the player will drop the item.
 */

public class PlayerItemHolder : MonoBehaviour
{

    private List<HoldableItem> itemsInCollider = new List<HoldableItem>();
    private bool holdingItem = false;
    private HoldableItem currentlyHeldItem = null;
    [SerializeField] private Transform itemAttachmentPoint;

    private void OnTriggerEnter(Collider other)
    {
        HoldableItem holdableItem = other.GetComponent<HoldableItem>();
        if (holdableItem == null) return;
        if (itemsInCollider.Contains(holdableItem)) return;

        itemsInCollider.Add(holdableItem);
    }

    private void OnTriggerExit(Collider other)
    {
        HoldableItem holdableItem = other.GetComponent<HoldableItem>();
        if (holdableItem == null) return;

        if (itemsInCollider.Contains(holdableItem))
            itemsInCollider.Remove(holdableItem);
    }

    public void InputPressed()
    {
        // If the player has an item in their possession, drop it.
        if (holdingItem)
        {
            DropItem();
            return;
        }

        // otherwise, check if there are any items that can be picked up.

        if (itemsInCollider.Count == 0) return;
        
        HoldableItem closest = GetClosestItem();

        PickUpItem(closest);
    }

    private HoldableItem GetClosestItem()
    {
        HoldableItem closest = null;
        float shortestDist = 1000f;
        foreach (HoldableItem item in itemsInCollider)
        {
            float dist = DistanceTo(item);
            if (dist < shortestDist)
            {
                shortestDist = dist;
                closest = item;
            }
        }
        return closest;
    }

    private float DistanceTo(HoldableItem item)
    {
        return Vector3.Distance(item.transform.position, itemAttachmentPoint.position);
    }

    private void DropItem()
    {
        Debug.Log("Dropping item");
        holdingItem = false;
        currentlyHeldItem.DetachFromTransform();
    }

    private void PickUpItem(HoldableItem item)
    {
        Debug.Log("Picking up item");
        holdingItem = true;
        currentlyHeldItem = item;
        item.AttachToTransform(itemAttachmentPoint);
    }
}
