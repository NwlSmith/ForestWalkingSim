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

    }
    #endregion
}