using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Creator: Nate Smith
 * Creation Date: 2/11/2021
 * Description: A retriever and director for inputs.
 * 
 * Different input is processed based on which game state the player is in.
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
        Services.PlayerMovement.InputUpdate(Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical"),
            Input.GetAxis("Jump"),
            Input.GetButton("Sprint"));

        Services.CameraManager.InputUpdate(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y"));
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
        if (Input.GetButtonDown("Jump") || Input.GetButtonDown("Fire"))
        {
            // Select dialogue option?
        }
    }
}
