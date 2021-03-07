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
 * 
 */

public class PlayerItemHolder : MonoBehaviour
{

    private List<HoldableItem> _itemsInCollider = new List<HoldableItem>();
    public bool _holdingItem = false;
    public HoldableItem _currentlyHeldItem { get; private set; }
    [SerializeField] private Transform _itemAttachmentPoint;

    private void OnTriggerEnter(Collider other)
    {
        HoldableItem holdableItem = other.GetComponent<HoldableItem>();
        if (_holdingItem) return;
        if (holdableItem == null) return;
        if (_itemsInCollider.Contains(holdableItem)) return;

        _itemsInCollider.Add(holdableItem);

        Services.UIManager.DisplayItemPickupPrompt();
    }

    private void OnTriggerExit(Collider other)
    {
        HoldableItem holdableItem = other.GetComponent<HoldableItem>();
        if (holdableItem == null) return;

        if (_itemsInCollider.Contains(holdableItem))
            _itemsInCollider.Remove(holdableItem);

        if (_itemsInCollider.Count <= 0)
            Services.UIManager.HideItemPickupPrompt();
    }

    public void InputPressed()
    {
        // If the player has an item in their possession, drop it.
        if (_holdingItem && _currentlyHeldItem.GetComponent<QuestItem>() == null)
        {
            DropItem();
            return;
        }

        // otherwise, check if there are any items that can be picked up.

        if (_itemsInCollider.Count == 0) return;
        
        HoldableItem closest = GetClosestItem();

        PickUpItem(closest);
    }

    private HoldableItem GetClosestItem()
    {
        HoldableItem closest = null;
        float shortestDist = 1000f;
        foreach (HoldableItem item in _itemsInCollider)
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
        return Vector3.Distance(item.transform.position, _itemAttachmentPoint.position);
    }

    public void DropItem()
    {
        Debug.Log("Dropping item");
        _holdingItem = false;
        _currentlyHeldItem.DetachFromTransform();
        //if (itemsInCollider.Contains(currentlyHeldItem))

    }

    public void PickUpItem(HoldableItem item)
    {
        Debug.Log("Picking up item");
        _holdingItem = true;
        _currentlyHeldItem = item;
        item.AttachToTransform(_itemAttachmentPoint);
        Services.UIManager.HideItemPickupPrompt();
    }
}
