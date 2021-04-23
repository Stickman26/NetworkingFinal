using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private float verticleVelocity;

    private float speedMod = 5f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Vector3 inputs = Vector3.zero;

        inputs.x = Input.GetAxis("Horizontal") * speedMod;
        inputs.z = Input.GetAxis("Vertical") * speedMod;

        if(controller.isGrounded)
        {
            verticleVelocity = 1f;

            if(Input.GetButton("Jump"))
            {
                verticleVelocity = 10f;
            }
        }
        else
        {
            verticleVelocity = 10f * Time.deltaTime;
        }

        inputs.y = verticleVelocity;

        controller.Move(inputs * Time.deltaTime);
    }
}
