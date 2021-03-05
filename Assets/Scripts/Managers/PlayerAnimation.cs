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
    #endregion

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();

        if (_animator == null)
        {
            Debug.Log("Failed to retrieve animator.");
        }
    }

    #region Triggers.

    public void Moving(bool isMoving)
    {
        _animator.SetBool("Moving", isMoving);
        if (!isMoving) Sprinting(false);
    }

    public void Sprinting(bool isSprinting) => _animator.SetBool(_sprinting, isSprinting);

    public void Jump() => _animator.SetTrigger(_sprinting);

    public void Falling(bool isFalling) => _animator.SetBool(_falling, isFalling);

    public void Pickup() => _animator.SetTrigger(_pickup);

    // Make a trigger to pause animation.

    #endregion

}
