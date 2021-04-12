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
 */

public static class InputManager
{
    private class Inputs
    {
        public float Hor                    = 0f;
        public readonly string HorStr;
        public float Ver                    = 0f;
        public readonly string VerStr;
        public bool Jump                    = false;
        public readonly string JumpStr;
        public bool Sprint                  = false;
        public readonly string SprintStr;

        public bool Interact                = false;
        public readonly string InteractStr;

        public bool Pause                   = false;
        public readonly string PauseStr;

        public float CameraX                = 0f;
        public readonly string CameraXStr;
        public float CameraY                = 0f;
        public readonly string CameraYStr;

        public delegate void InputIntake(string input);

        public Inputs(string hor, string ver, string jump, string sprint, string interact, string pause, string cameraX, string cameraY)
        {
            HorStr = hor;
            VerStr = ver;
            JumpStr = jump;
            SprintStr = sprint;
            InteractStr = interact;
            PauseStr = pause;
            CameraXStr = cameraX;
            CameraYStr = cameraY;
        }

        public Inputs Update()
        {
            Hor = Input.GetAxis(HorStr);
            Ver = Input.GetAxis(VerStr);

            Jump = Input.GetButtonDown(JumpStr);
            Sprint = Input.GetButton(SprintStr);
            Interact = Input.GetButtonDown(InteractStr);
            Pause = Input.GetButtonDown(PauseStr);

            CameraX = Input.GetAxis(CameraXStr);
            CameraY = Input.GetAxis(CameraYStr);

            return this;
        }

        public bool InputsEntered => (Hor > .1f || Hor < -.1f) || (Ver > .1f || Ver < -.1f) || Jump || Sprint || Interact || Pause || (CameraX > .1f || CameraX < -.1f) || (CameraY > .1f || CameraY < -.1f);

        public Inputs UpdatePaused()
        {
            Pause = Input.GetButtonDown(PauseStr);
            return this;
        }
    }

    private static Inputs Keyboard      = new Inputs(Str.Hor, Str.Ver, Str.JumpInput, Str.Sprint, Str.Interact, Str.Pause, Str.MouseX, Str.MouseY);
    private static Inputs Controller    = new Inputs(Str.LJoystickX, Str.LJoystickY, Str.JumpInputC, Str.SprintC, Str.InteractC, Str.PauseC, Str.RJoystickX, Str.RJoystickY);

    static InputManager()
    {
        XboxOneController = ControllerConnectedRecently();
    }

    public static bool ControllerConnected => Input.GetJoystickNames().Length > 0;
    private static bool XboxOneController = false;

    public static bool ControllerConnectedRecently()
    {
        string[] names = Input.GetJoystickNames();
        foreach (string name in names)
        {
            if (name.Length == 19)
            {
                Logger.Warning("PS4 CONTROLLER IS CONNECTED");
            }
            if (name.Length == 33)
            {
                Logger.Warning("XBox One CONTROLLER IS CONNECTED");
                return true;
            }
        }
        return false;
    }

    private static bool UsingController()
    {
        return XboxOneController && !Keyboard.InputsEntered;
    }

    private static Inputs DetermineInputs()
    {
        Keyboard.Update();
        if (XboxOneController)
        {
            Controller.Update();
            if (Controller.InputsEntered)
                return Controller;
        }
        return Keyboard;
    }

    private static Inputs DeterminePausedInputs()
    {
        if (!XboxOneController || !ControllerConnected)
            return Keyboard.UpdatePaused();
        else
            return Controller.UpdatePaused();
    }

    public static void ProcessPlayInput()
    {
        Inputs inputs = DetermineInputs();
        

        // Intake pause instructions.
        if (inputs.Pause || Input.GetButtonDown(Str.Cancel))
        {
            Services.GameManager.Pause();
        }

        // Send movement inputs to player.
        Services.PlayerMovement.InputUpdate(
            inputs.Hor,
            inputs.Ver,
            inputs.Jump,
            inputs.Sprint);

        Services.CameraManager.InputUpdate(
            inputs.CameraX,
            inputs.CameraY);


        if (inputs.Interact)
        {
            if (Services.PlayerItemHolder.canPickUpItem || Services.PlayerItemHolder._holdingItem)
                Services.PlayerItemHolder.InputPressed(); // CREATE HIERARCHY BETWEEN THESE TWO - Only one should be used at once.
            else
                NPCInteractionManager.InputPressed();
        }
    }

    public static void ProcessPauseMenuInput()
    {
        Inputs input = DeterminePausedInputs();
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
        if (input.Pause)
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
