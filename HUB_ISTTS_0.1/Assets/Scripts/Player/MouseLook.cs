using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{

    private float mouseSensitivity = 200f;

    public Transform playerBody;

    private float xRot = 0f;

    private float sendDelay = 0.1f;
    private float lastSent;

    public void MouseState(bool locked)
    {
        if(locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);


        transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);

        if (playerBody != null)
            playerBody.Rotate(Vector3.up * mouseX);

        if (playerBody != null && sendDelay <= lastSent)
        {
            lastSent = 0f;
            GameObject.Find("Networking").GetComponent<Client>().SendRotation(xRot, playerBody.rotation.eulerAngles.y);
        }
        else
            lastSent += Time.deltaTime;
    }

    //Method for sending current rotation
}
