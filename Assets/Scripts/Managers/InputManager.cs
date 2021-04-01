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

public class InputManager
{

    #region String Cache.
    private readonly string _pause = "Pause";
    private readonly string _cancel = "Cancel";
    private readonly string _hor = "Horizontal";
    private readonly string _ver = "Vertical";
    private readonly string _jump = "Jump";
    private readonly string _sprint = "Sprint";
    private readonly string _mouseX = "Mouse X";
    private readonly string _mouseY = "Mouse Y";
    private readonly string _interact = "Interact";
    #endregion

    public void ProcessPlayInput()
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
            Services.PlayerItemHolder.InputPressed(); // CREATE HIERARCHY BETWEEN THESE TWO - Only one should be used at once.
            Services.NPCInteractionManager.InputPressed();
        }
    }

    public void ProcessPauseMenuInput()
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

    public void ProcessDialogueInput()
    {
        /*
        if (Input.GetButtonDown("Jump"))
        {
            Services.GameManager.ExitDialogue();
        }*/
    }
}
