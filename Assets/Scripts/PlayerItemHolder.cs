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
    private bool _inProgress = false;
    private List<HoldableItem> _itemsInCollider = new List<HoldableItem>();
    public bool canPickUpItem = false;
    public bool _holdingItem = false;
    public HoldableItem _currentlyHeldItem { get; private set; }
    [SerializeField] private Transform _itemAttachmentPoint;
    [SerializeField] private AudioClip[] _audioClips;
    private AudioSource _as;

    private void Awake() => _as = GetComponent<AudioSource>();

    private void OnTriggerEnter(Collider other)
    {
        HoldableItem holdableItem = other.GetComponent<HoldableItem>();
        if (_holdingItem) return;
        if (holdableItem == null) return;
        if (_itemsInCollider.Contains(holdableItem)) return;

        _itemsInCollider.Add(holdableItem);

        canPickUpItem = true;

        Services.UIManager.DisplayItemPickupPrompt();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(Str.ItemTag) && _itemsInCollider.Count > 0) Services.UIManager.PositionItemPrompt(_itemsInCollider[0].PickupPromptOffset);
    }

    public Vector3 ClosestItemPosition()
    {
        if (_itemsInCollider.Count == 0) return Vector3.zero;

        return GetClosestItem().transform.position;
    }

    private void OnTriggerExit(Collider other)
    {
        HoldableItem holdableItem = other.GetComponent<HoldableItem>();
        if (holdableItem == null) return;

        if (_itemsInCollider.Contains(holdableItem))
            _itemsInCollider.Remove(holdableItem);

        if (_itemsInCollider.Count <= 0)
        {
            Services.UIManager.HideItemPickupPrompt();
            canPickUpItem = false;
        }
    }

    public void InputPressed()
    {
        if (_inProgress) return;
        // If the player has an item in their possession, drop it.
        if (_holdingItem)
        {
            DropItem();
            return;
        }

        // otherwise, check if there are any items that can be picked up.

        if (_itemsInCollider.Count == 0) return;
        
        HoldableItem closest = GetClosestItem();

        StartCoroutine(PickUpItemeEnum(closest));
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

    private float DistanceTo(HoldableItem item) => Vector3.Distance(item.transform.position, _itemAttachmentPoint.position);

    public void DropItem() => StartCoroutine(DropItemEnum());

    private IEnumerator DropItemEnum()
    {
        _inProgress = true;
        Debug.Log("Dropping item");
        PlayerAnimation.Pickup();
        yield return new WaitForSeconds(.33f);
        _as.clip = _audioClips[Random.Range(0, _audioClips.Length)];
        _as.pitch = Random.Range(.95f, 1.05f);
        DetachFromTransform();
        _inProgress = false;
    }

    public IEnumerator PickUpItemeEnum(HoldableItem item)
    {
        _inProgress = true;
        Debug.Log("Picking up item");
        _currentlyHeldItem = item;
        Services.UIManager.HideItemPickupPrompt();
        PlayerAnimation.Pickup();
        yield return new WaitForSeconds(.33f);

        AttachToTransform(item);
        _inProgress = false;
    }

    public void DetachFromTransform()
    {
        _holdingItem = false;
        _currentlyHeldItem.DetachFromTransform();
    }

    public void AttachToTransform(HoldableItem item)
    {
        _holdingItem = true;
        _currentlyHeldItem.AttachToTransform(_itemAttachmentPoint);
    }
}
