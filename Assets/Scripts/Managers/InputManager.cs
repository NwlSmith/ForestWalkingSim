using System.Collections;
using System.Collections.Generic;
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
 * - 
 */

public class InputManager : MonoBehaviour
{

    public void ProcessPlayInput()
    {
        // Intake pause instructions.
        if (Input.GetButtonDown("Pause"))
        {
            Services.GameManager.Pause();
        }

        // Send movement inputs to player.
        Services.PlayerMovement.InputUpdate(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical"),
            Input.GetButtonDown("Jump"),
            Input.GetButton("Sprint"));

        Services.CameraManager.InputUpdate(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y"));


        if (Input.GetButtonDown("Interact"))
        {
            Services.PlayerItemHolder.InputPressed(); // CREATE HIERARCHY BETWEEN THESE TWO - Only one should be used at once.
            Services.NPCInteractionManager.InputPressed();
        }
    }

    public void ProcessPauseMenuInput()
    {
        if (Input.GetButtonDown("Pause"))
        {
            Services.GameManager.Unpause();
        }
    }

    public void ProcessDialogueInput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            Services.GameManager.ExitDialogue();
        }
    }
}
