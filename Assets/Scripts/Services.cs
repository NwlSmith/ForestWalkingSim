using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/6/2021
 * Description: A holder for systems that need to be referenced in many different scripts.
 * It is Static, so it does not need to be in the scene to work.
 * Needs to grab other systems.
 * Superior to a singleton instances because they are clumsy.
 * 
 * Using this form of Getters and Setters prevents crashes from null references.
 */

public static class Services
{
    #region Variables

    // Ensures you don't get a null reference exception.
    private static GameManager _gameManager;
    public static GameManager GameManager
    {
        get
        {
            Debug.Assert(_gameManager != null);
            return _gameManager;
        }
        private set => _gameManager = value;
    }

    private static UIManager _uimanager;
    public static UIManager UIManager
    {
        get
        {
            Debug.Assert(_uimanager != null);
            return _uimanager;
        }
        private set => _uimanager = value;
    }

    private static InputManager _inputManager;
    public static InputManager InputManager
    {
        get
        {
            Debug.Assert(_inputManager != null);
            return _inputManager;
        }
        private set => _inputManager = value;
    }

    private static PlayerMovement _playerMovement;
    public static PlayerMovement PlayerMovement
    {
        get
        {
            Debug.Assert(_playerMovement != null);
            return _playerMovement;
        }
        private set => _playerMovement = value;
    }

    private static PlayerAnimation _playerAnimation;
    public static PlayerAnimation PlayerAnimation
    {
        get
        {
            Debug.Assert(_playerAnimation != null);
            return _playerAnimation;
        }
        private set => _playerAnimation = value;
    }

    private static CameraManager _cameraManager;
    public static CameraManager CameraManager
    {
        get
        {
            Debug.Assert(_cameraManager != null);
            return _cameraManager;
        }
        private set => _cameraManager = value;
    }

    private static PlayerItemHolder _playerItemHolder;
    public static PlayerItemHolder PlayerItemHolder
    {
        get
        {
            Debug.Assert(_playerItemHolder != null);
            return _playerItemHolder;
        }
        private set => _playerItemHolder = value;
    }

    private static NPCInteractionManager _npcInteractionManager;
    public static NPCInteractionManager NPCInteractionManager
    {
        get
        {
            Debug.Assert(_npcInteractionManager != null);
            return _npcInteractionManager;
        }
        private set => _npcInteractionManager = value;
    }

    private static DialogueController _dialogueController;
    public static DialogueController DialogueController
    {
        get
        {
            Debug.Assert(_dialogueController != null);
            return _dialogueController;
        }
        private set => _dialogueController = value;
    }

    private static QuestManager _questManager;
    public static QuestManager QuestManager
    {
        get
        {
            Debug.Assert(_questManager != null);
            return _questManager;
        }
        private set => _questManager = value;
    }

    private static SaveManager _saveManager;
    public static SaveManager SaveManager
    {
        get
        {
            Debug.Assert(_saveManager != null);
            return _saveManager;
        }
        private set => _saveManager = value;
    }
    #endregion

    #region Functions

    public static void InitializeServices(GameManager gm)
    {
        GameManager = gm;
        UIManager = Object.FindObjectOfType<UIManager>();
        InputManager = Object.FindObjectOfType<InputManager>();
        PlayerMovement = Object.FindObjectOfType<PlayerMovement>();
        PlayerAnimation = Object.FindObjectOfType<PlayerAnimation>();
        CameraManager = Object.FindObjectOfType<CameraManager>();
        PlayerItemHolder = Object.FindObjectOfType<PlayerItemHolder>();
        NPCInteractionManager = Object.FindObjectOfType<NPCInteractionManager>();
        DialogueController = Object.FindObjectOfType<DialogueController>();
        QuestManager = Object.FindObjectOfType<QuestManager>();
        SaveManager = new SaveManager();
    }
    #endregion
}