using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Creator: Nate Smith
 * Creation Date: 2/11/2021
 * Description: A processor for inputs.
 * 
 * Different input is processed based on which game state the player is in.
 */

public class InputManager : MonoBehaviour
{
    
    public void ProcessPlayInput()
    {
        if (Input.GetButtonDown("Pause"))
        {
            Services.GameManager.Pause();
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
        if (Input.GetButtonDown("Jump") || Input.GetButtonDown("Fire"))
        {
            // Select dialogue option?
        }
    }
}
