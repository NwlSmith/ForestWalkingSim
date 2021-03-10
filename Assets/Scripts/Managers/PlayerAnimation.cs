using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/11/2021
 * Description: Given the player's movement and player state, generate animations.
 * 
 * Most of this animation will use Rigidbodies. I can't guarantee it'll look perfect but I'm giving it a good try.
 */
public class PlayerAnimation : MonoBehaviour
{

    #region Const Strings.
    private const string _jump = "Jump";
    private const string _moving = "Moving";
    private const string _sprinting = "Sprinting";
    private const string _falling = "Falling";
    private const string _pickup = "Pickup";
    private const string _talk = "Talk";
    private const string _sitting = "Sitting";
    #endregion

    private Animator _animator;
    private FIMSpace.FSpine.FSpineAnimator _spineAnimator;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _spineAnimator = GetComponent<FIMSpace.FSpine.FSpineAnimator>();

        if (_animator == null)
        {
            Debug.Log("Failed to retrieve animator.");
        }
    }

    #region Triggers.

    public void Moving(bool isMoving)
    {
        if (isMoving)
            _spineAnimator.SpineAnimatorAmount = .5f;
        else
        {
            _spineAnimator.SpineAnimatorAmount = 0f;
        }
        _animator.SetBool("Moving", isMoving);
    }

    public void Sprinting(bool isSprinting) => _animator.SetBool(_sprinting, isSprinting);

    public void Jump()
    {
        _animator.SetTrigger(_jump);
        _spineAnimator.SpineAnimatorAmount = 0f;
    }

    public void Falling(bool isFalling)
    {
        _animator.SetBool(_falling, isFalling);
    }

    public void Pickup() => _animator.SetTrigger(_pickup);

    public void Talk() => _animator.SetTrigger(_talk);

    public void Sitting(bool isSitting)
    {
        _animator.SetBool(_sitting, isSitting);
        _spineAnimator.SpineAnimatorAmount = 0f;
    }

    // Make a trigger to pause animation.

    #endregion

}
