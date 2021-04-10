using UnityEngine;

/*
 * Creator: Nate Smith
 * Creation Date: 2/11/2021
 * Description: A retriever and director for inputs.
 * 
 * Different input is processed based on which game state the player is in.
 * 
 * Each method is called in the GameManager StateMachine.
 * 
 * Issues:
 * - Need hierachy between entering conversation and picking up items.
 */

public static class InputManager
{

    #region String Cache.
    private const string _pause = "Pause";
    private const string _cancel = "Cancel";
    private const string _hor = "Horizontal";
    private const string _ver = "Vertical";
    private const string _jump = "Jump";
    private const string _sprint = "Sprint";
    private const string _mouseX = "Mouse X";
    private const string _mouseY = "Mouse Y";
    private const string _interact = "Interact";
    #endregion

    public static void ProcessPlayInput()
    {
        // Intake pause instructions.
        if (Input.GetButtonDown(_pause) || Input.GetButtonDown(_cancel))
        {
            Services.GameManager.Pause();
        }

        // Send movement inputs to player.
        Services.PlayerMovement.InputUpdate(
            Input.GetAxis(_hor),
            Input.GetAxis(_ver),
            Input.GetButtonDown(_jump),
            Input.GetButton(_sprint));

        Services.CameraManager.InputUpdate(
            Input.GetAxis(_mouseX),
            Input.GetAxis(_mouseY));


        if (Input.GetButtonDown(_interact))
        {
            if (Services.PlayerItemHolder.canPickUpItem || Services.PlayerItemHolder._holdingItem)
                Services.PlayerItemHolder.InputPressed(); // CREATE HIERARCHY BETWEEN THESE TWO - Only one should be used at once.
            else
                Services.NPCInteractionManager.InputPressed();
        }
    }

    public static void ProcessPauseMenuInput()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.M))
            Services.QuestManager.AdvanceQuest("Main");
        if (Input.GetKeyDown(KeyCode.N))
            Services.QuestManager.AdvanceQuest("Warbler");
        if (Input.GetKeyDown(KeyCode.F))
            Services.QuestManager.AdvanceQuest("Frog");
        if (Input.GetKeyDown(KeyCode.T))
            Services.QuestManager.AdvanceQuest("Turtle");
        if (Input.GetKeyDown(KeyCode.H))
            Services.PlayerMovement.ForceTransform(new Vector3(460, 21, 506), Quaternion.identity);
#endif
        if (Input.GetButtonDown(_pause))
        {
            Services.GameManager.Unpause();
        }
    }

    public static void ProcessDialogueInput()
    {
        /*
        if (Input.GetButtonDown("Jump"))
        {
            Services.GameManager.ExitDialogue();
        }*/
    }
}
