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

    //gets the mouse's current state so it can be sent over the network
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
        //get the mouse x and y
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);

        //handle the rotation
        transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);

        //make sure the player exists and has a body
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
