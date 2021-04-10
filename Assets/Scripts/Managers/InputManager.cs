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

    public static void ProcessPlayInput()
    {
        // Intake pause instructions.
        if (Input.GetButtonDown(Str.Pause) || Input.GetButtonDown(Str.Cancel))
        {
            Services.GameManager.Pause();
        }

        // Send movement inputs to player.
        Services.PlayerMovement.InputUpdate(
            Input.GetAxis(Str.Hor),
            Input.GetAxis(Str.Ver),
            Input.GetButtonDown(Str.JumpInput),
            Input.GetButton(Str.Sprint));

        Services.CameraManager.InputUpdate(
            Input.GetAxis(Str.MouseX),
            Input.GetAxis(Str.MouseY));


        if (Input.GetButtonDown(Str.Interact))
        {
            if (Services.PlayerItemHolder.canPickUpItem || Services.PlayerItemHolder._holdingItem)
                Services.PlayerItemHolder.InputPressed(); // CREATE HIERARCHY BETWEEN THESE TWO - Only one should be used at once.
            else
                NPCInteractionManager.InputPressed();
        }
    }

    public static void ProcessPauseMenuInput()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.M))
            QuestManager.AdvanceQuest(Str.Main);
        if (Input.GetKeyDown(KeyCode.N))
            QuestManager.AdvanceQuest(Str.Warbler);
        if (Input.GetKeyDown(KeyCode.F))
            QuestManager.AdvanceQuest(Str.Frog);
        if (Input.GetKeyDown(KeyCode.T))
            QuestManager.AdvanceQuest(Str.Turtle);
        if (Input.GetKeyDown(KeyCode.H))
            Services.PlayerMovement.ForceTransform(new Vector3(460, 21, 506), Quaternion.identity);
#endif
        if (Input.GetButtonDown(Str.Pause))
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
