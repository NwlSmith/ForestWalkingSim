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

    private static UISound _uiSound;
    public static UISound UISound
    {
        get
        {
            Debug.Assert(_uiSound != null);
            return _uiSound;
        }
        private set => _uiSound = value;
    }

    private static SpacialAudioManager _spacialAudioManager;
    public static SpacialAudioManager SpacialAudioManager
    {
        get
        {
            Debug.Assert(_spacialAudioManager != null);
            return _spacialAudioManager;
        }
        private set => _spacialAudioManager = value;
    }

    private static QuestItemRepository _questItemRepository;
    public static QuestItemRepository QuestItemRepository
    {
        get
        {
            Debug.Assert(_questItemRepository != null);
            return _questItemRepository;
        }
        private set => _questItemRepository = value;
    }
    #endregion


    #region Functions

    public static void InitializeServices(GameManager gm)
    {
        GameManager = gm;
        UIManager = Object.FindObjectOfType<UIManager>();
        PlayerMovement = Object.FindObjectOfType<PlayerMovement>();
        CameraManager = Object.FindObjectOfType<CameraManager>();
        PlayerItemHolder = Object.FindObjectOfType<PlayerItemHolder>();
        DialogueController = Object.FindObjectOfType<DialogueController>();
        SaveManager = new SaveManager();
        UISound = Object.FindObjectOfType<UISound>();
        SpacialAudioManager = Object.FindObjectOfType<SpacialAudioManager>();
        QuestItemRepository = Object.FindObjectOfType<QuestItemRepository>();
    }
    #endregion
}