using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/11/2021
 * Description: Given the player's movement and player state, generate animations.
 * 
 * Most of this animation will use Rigidbodies. I can't guarantee it'll look perfect but I'm giving it a good try.
 */
public class PlayerAnimation
{

    #region Const Strings.
    private readonly int _jump = Animator.StringToHash("Jump");
    private readonly int _moving = Animator.StringToHash("Moving");
    private readonly int _sprinting = Animator.StringToHash("Sprinting");
    private readonly int _falling = Animator.StringToHash("Falling");
    private readonly int _pickup = Animator.StringToHash("Pickup");
    private readonly int _talk = Animator.StringToHash("Talk");
    private readonly int _sitting = Animator.StringToHash("Sitting");
    #endregion

    private Animator _animator;
    private FIMSpace.FSpine.FSpineAnimator _spineAnimator;

    public PlayerAnimation()
    {
        _animator = Services.PlayerMovement.GetComponentInChildren<Animator>();
        _spineAnimator = Services.PlayerMovement.GetComponent<FIMSpace.FSpine.FSpineAnimator>();

        if (_animator == null)
        {
            Debug.Log("Failed to retrieve animator.");
        }
    }

    #region Checks.

    public bool IsMoving => _animator.GetBool(_moving);

    public bool IsSprinting => _animator.GetBool(_sprinting);

    public bool IsFalling => _animator.GetBool(_falling);

    public bool IsSitting => _animator.GetBool(_sitting);

    #endregion

    #region Triggers.

    public void Moving(bool isMoving)
    {
        if (isMoving) _spineAnimator.SpineAnimatorAmount = .5f;
        //else _spineAnimator.SpineAnimatorAmount = 0f;
        _animator.SetBool(_moving, isMoving);
    }

    public void Sprinting(bool isSprinting) => _animator.SetBool(_sprinting, isSprinting);

    public void Jump()
    {
        _animator.SetTrigger(_jump);
        _spineAnimator.SpineAnimatorAmount = 0f;
    }

    public void Falling(bool isFalling) => _animator.SetBool(_falling, isFalling);

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
