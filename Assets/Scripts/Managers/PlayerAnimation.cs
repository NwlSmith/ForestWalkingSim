using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/11/2021
 * Description: Given the player's movement and player state, generate animations.
 * 
 * Most of this animation will use Rigidbodies. I can't guarantee it'll look perfect but I'm giving it a good try.
 */
public static class PlayerAnimation
{
    private static Animator _animator;
    private static FIMSpace.FSpine.FSpineAnimator _spineAnimator;

    public static void Init()
    {
        _animator = Services.PlayerMovement.GetComponentInChildren<Animator>();
        _spineAnimator = Services.PlayerMovement.GetComponent<FIMSpace.FSpine.FSpineAnimator>();

        if (_animator == null)
        {
            Logger.Warning("Failed to retrieve animator.");
        }
    }

    #region Checks.

    public static bool IsMoving => _animator.GetBool(Str.Moving);

    public static bool IsSprinting => _animator.GetBool(Str.Sprinting);

    public static bool IsFalling => _animator.GetBool(Str.Falling);

    public static bool IsSitting => _animator.GetBool(Str.Sitting);

    #endregion

    #region Triggers.

    public static void Moving(bool isMoving)
    {
        if (isMoving) _spineAnimator.SpineAnimatorAmount = .5f;
        //else _spineAnimator.SpineAnimatorAmount = 0f;
        _animator.SetBool(Str.Moving, isMoving);
    }

    public static void Sprinting(bool isSprinting) => _animator.SetBool(Str.Sprinting, isSprinting);

    public static void Jump()
    {
        _animator.SetTrigger(Str.Jump);
        _spineAnimator.SpineAnimatorAmount = 0f;
    }

    public static void Falling(bool isFalling) => _animator.SetBool(Str.Falling, isFalling);

    public static void Pickup() => _animator.SetTrigger(Str.Pickup);

    public static void Talk() => _animator.SetTrigger(Str.Talk);

    public static void Sitting(bool isSitting)
    {
        _animator.SetBool(Str.Sitting, isSitting);
        _spineAnimator.SpineAnimatorAmount = 0f;
    }

    // Make a trigger to pause animation.

    #endregion

}
