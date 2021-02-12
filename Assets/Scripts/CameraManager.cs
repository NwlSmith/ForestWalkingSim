using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    
    [SerializeField] private Transform targetVector;
    private Cinemachine.CinemachineVirtualCamera mainFollowCamera;
    private float _curVertRot = 0f;
    private float _curHorRot = 0f;

    private float _mouseY = 0f;
    private float _mouseX = 0f;

    private readonly float _minVert = -30f;
    private readonly float _maxVert = 30f;

    private void Awake()
    {
        Cinemachine.CinemachineVirtualCamera cam = GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>();
        cam.Follow = targetVector;
    }

    private void LateUpdate()
    {
        // Calculate new vertical rotation.
        _curVertRot -= _mouseY * mouseSensitivity * Time.deltaTime;
        _curVertRot = Mathf.Clamp(_curVertRot, _minVert, _maxVert);

        // Calculate new horizontal rotation.
        _curHorRot += _mouseX * mouseSensitivity * Time.deltaTime;

        // Calculate new rotate targetVector.
        targetVector.eulerAngles = new Vector3(_curVertRot, _curHorRot, 0);
    }

    // Updates the camera movement inputs. Called in InputManager.
    public void InputUpdate(float mouseX, float mouseY)
    {
        _mouseX = mouseX;
        _mouseY = mouseY;
    }
}
