/*
 * Creator: Nate Smith
 * Date Created: 4/10/2021
 * Description: Static repository for frequently used strings.
 */
public static class Str
{
    #region Quest.
    public const string Main    = "Main";
    public const string Warbler = "Warbler";
    public const string Frog    = "Frog";
    public const string Turtle  = "Turtle";
    public const string Seed    = "Seed";
    public const string Soil    = "Soil";
    public const string Rain    = "Rain";
    #endregion

    #region Item animations.
    public static readonly int Held = UnityEngine.Animator.StringToHash("Held");
    #endregion

    public const string PlayerTag = "Player";
    public const string ItemTag   = "Item";



    #region Input.
    public const string Pause       = "Pause";
    public const string Cancel      = "Cancel";
    public const string Hor         = "Horizontal";
    public const string Ver         = "Vertical";
    public const string JumpInput   = "Jump";
    public const string Sprint      = "Sprint";
    public const string MouseX      = "Mouse X";
    public const string MouseY      = "Mouse Y";
    public const string Interact    = "Interact";
    // Controller Inputs
    public const string PauseC      = "Start btn";
    public const string LJoystickX  = "LJoystick X";
    public const string LJoystickY  = "LJoystick Y";
    public const string JumpInputC  = "B";
    public const string SprintC     = "LBumper";
    public const string RJoystickX  = "RJoystick X";
    public const string RJoystickY  = "RJoystick Y";
    public const string InteractC   = "A";
    #endregion


    #region Player Animation.
    public static readonly int Jump         = UnityEngine.Animator.StringToHash("Jump");
    public static readonly int Moving       = UnityEngine.Animator.StringToHash("Moving");
    public static readonly int Sprinting    = UnityEngine.Animator.StringToHash("Sprinting");
    public static readonly int Falling      = UnityEngine.Animator.StringToHash("Falling");
    public static readonly int Pickup       = UnityEngine.Animator.StringToHash("Pickup");
    public static readonly int Talk         = UnityEngine.Animator.StringToHash("Talk");
    public static readonly int Sitting      = UnityEngine.Animator.StringToHash("Sitting");
    #endregion



    #region UI.
    public static readonly int FadeOut = UnityEngine.Animator.StringToHash("FadeOut");
    public static readonly int Visible = UnityEngine.Animator.StringToHash("Visible");
    #endregion

    #region Const Strings.
    public static readonly int Running = UnityEngine.Animator.StringToHash("Running");
    #endregion


    #region NPCs.
    public static readonly int InConvo = UnityEngine.Animator.StringToHash("InConversation");
    #endregion


    #region Cutscenes.
    public const string MidSequence = "MidSequence";
    public const string EndSequence = "EndSequence";
    #endregion

    #region Save system.
    public const string SaveName          = "/cur.save";
    public const string SaveDefaultName   = "/default.save";
    public const string Child1String      = "$found_warbler_child_1";
    public const string Child2String      = "$found_warbler_child_2";
    public const string Child3String      = "$found_warbler_child_3";
    public const string SeedString        = "$found_seed";
    public const string SoilString        = "$found_soil";
    public const string RainString        = "$found_rain";
    #endregion
}
