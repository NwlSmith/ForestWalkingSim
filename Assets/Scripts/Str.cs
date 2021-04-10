/*
 * Creator: Nate Smith
 * Date Created: 4/10/2021
 * Description: Static repository for frequently used strings.
 */
public static class Str
{
    #region Quest.
    public static readonly string Main    = "Main";
    public static readonly string Warbler = "Warbler";
    public static readonly string Frog    = "Frog";
    public static readonly string Turtle  = "Turtle";
    public static readonly string Seed    = "Seed";
    public static readonly string Soil    = "Soil";
    public static readonly string Rain    = "Rain";
    #endregion

    #region Item animations.
    public static readonly int Held = UnityEngine.Animator.StringToHash("Held");
    #endregion

    public static readonly string PlayerTag = "Player";
    public static readonly string ItemTag   = "Item";



    #region Input.
    public static readonly string Pause     = "Pause";
    public static readonly string Cancel    = "Cancel";
    public static readonly string Hor       = "Horizontal";
    public static readonly string Ver       = "Vertical";
    public static readonly string JumpInput = "Jump";
    public static readonly string Sprint    = "Sprint";
    public static readonly string MouseX    = "Mouse X";
    public static readonly string MouseY    = "Mouse Y";
    public static readonly string Interact  = "Interact";
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
    public static readonly string MidSequence = "MidSequence";
    public static readonly string EndSequence = "EndSequence";
    #endregion

    #region Save system.
    public static readonly string SaveName          = "/cur.save";
    public static readonly string SaveDefaultName   = "/default.save";
    public static readonly string Child1String      = "$found_warbler_child_1";
    public static readonly string Child2String      = "$found_warbler_child_2";
    public static readonly string Child3String      = "$found_warbler_child_3";
    public static readonly string SeedString        = "$found_seed";
    public static readonly string SoilString        = "$found_soil";
    public static readonly string RainString        = "$found_rain";
    #endregion
}
