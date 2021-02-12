using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    
    [SerializeField] private Transform targetVector;
    private Cinemachine.CinemachineVirtualCamera mainFollowCamera;
    private float vRot = 0f;
    private float hRot = 0f;

    private void Awake()
    {
        Cinemachine.CinemachineVirtualCamera cam = GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>();
        cam.Follow = targetVector;
    }

    void Update()
    {
        // Retrieve mouse input. // MOVE THIS TO INPUTHANDLER!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Move camera vertically.
        vRot -= mouseY;
        vRot = Mathf.Clamp(vRot, -30, 30);
        hRot += mouseX;
        targetVector.eulerAngles = new Vector3(vRot, hRot, 0);
    }
}
