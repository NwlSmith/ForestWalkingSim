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

    #region Const Strings.
    private static readonly int _jump = Animator.StringToHash("Jump");
    private static readonly int _moving = Animator.StringToHash("Moving");
    private static readonly int _sprinting = Animator.StringToHash("Sprinting");
    private static readonly int _falling = Animator.StringToHash("Falling");
    private static readonly int _pickup = Animator.StringToHash("Pickup");
    private static readonly int _talk = Animator.StringToHash("Talk");
    private static readonly int _sitting = Animator.StringToHash("Sitting");
    #endregion

    private static Animator _animator;
    private static FIMSpace.FSpine.FSpineAnimator _spineAnimator;

    static PlayerAnimation()
    {
        _animator = Services.PlayerMovement.GetComponentInChildren<Animator>();
        _spineAnimator = Services.PlayerMovement.GetComponent<FIMSpace.FSpine.FSpineAnimator>();

        if (_animator == null)
        {
            Debug.Log("Failed to retrieve animator.");
        }
    }

    #region Checks.

    public static bool IsMoving => _animator.GetBool(_moving);

    public static bool IsSprinting => _animator.GetBool(_sprinting);

    public static bool IsFalling => _animator.GetBool(_falling);

    public static bool IsSitting => _animator.GetBool(_sitting);

    #endregion

    #region Triggers.

    public static void Moving(bool isMoving)
    {
        if (isMoving) _spineAnimator.SpineAnimatorAmount = .5f;
        //else _spineAnimator.SpineAnimatorAmount = 0f;
        _animator.SetBool(_moving, isMoving);
    }

    public static void Sprinting(bool isSprinting) => _animator.SetBool(_sprinting, isSprinting);

    public static void Jump()
    {
        _animator.SetTrigger(_jump);
        _spineAnimator.SpineAnimatorAmount = 0f;
    }

    public static void Falling(bool isFalling) => _animator.SetBool(_falling, isFalling);

    public static void Pickup() => _animator.SetTrigger(_pickup);

    public static void Talk() => _animator.SetTrigger(_talk);

    public static void Sitting(bool isSitting)
    {
        _animator.SetBool(_sitting, isSitting);
        _spineAnimator.SpineAnimatorAmount = 0f;
    }

    // Make a trigger to pause animation.

    #endregion

}
