using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;

    public float speedMod = 12f;
    public float gravity = -19.62f;
    public float jumpHeight = 3f;

    public Vector3 velocity;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    bool isGrounded;

    //chat vars
    public GameObject chatBox;
    public GameObject chatPanel;
    bool disableInput;
    bool inputFieldActive;

    //Timing
    private float sendDelay = 0.1f;
    private float lastSent;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        groundCheck = transform.GetChild(2);
        groundMask = LayerMask.GetMask("Ground");
        chatBox = GameObject.FindGameObjectWithTag("chatbox");
        chatPanel = GameObject.Find("Scroll View");
    }

    private void Update()
    {

        if(chatBox == null)
        {
            chatBox = GameObject.FindGameObjectWithTag("chatbox");
        }
        if(chatPanel == null)
        {
            chatPanel = GameObject.Find("Scroll View");
        }

        //checks if the chatbox is currently active
        if(disableInput == false)
        {
            //if it isn't, let the player play the game
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z;

            controller.Move(move * speedMod * Time.deltaTime);

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            velocity.y += gravity * Time.deltaTime;

            controller.Move(velocity * Time.deltaTime);
        }

        //toggle chat input (disable game inputs)
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!disableInput)
            {
                disableInput = true;
                chatPanel.GetComponent<ScrollRect>().scrollSensitivity = 1;
            }

            chatBox.GetComponent<TMP_InputField>().ActivateInputField();
            inputFieldActive = true;
        }

        //checks if the chatbox input is active
        if (inputFieldActive)
        {
            //if it is, it deactivates the chatbox input when the player presses enter/return key
            if (Input.GetKeyDown(KeyCode.Return))
            {
                //if the message box is empty, don't send the message
                if(chatBox.GetComponent<TMP_InputField>().text != "")
                {
                    GameObject sendMessage = GameObject.Find("Networking");

                    string temp = chatBox.GetComponent<TMP_InputField>().text;

                    sendMessage.GetComponent<Client>().sendMessage(temp, sendMessage.GetComponent<Client>().reliableChannel);
                    chatBox.GetComponent<TMP_InputField>().text = "";
                }

                chatBox.GetComponent<TMP_InputField>().DeactivateInputField();
                hideChatBox();
            }

            //or it will close the chat interface and clear the new message box by pressing escape
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                chatBox.GetComponent<TMP_InputField>().text = "";
                chatBox.GetComponent<TMP_InputField>().DeactivateInputField();
                hideChatBox();
            }
        }
        else if (inputFieldActive == false)
        {
            //or if the chatbox isn't active, it will make sure it stays inactive
            chatBox.GetComponent<TMP_InputField>().text = "";
            chatBox.GetComponent<TMP_InputField>().DeactivateInputField();
            //chatPanel.GetComponent<Scrollbar>().
        }

        //send delay so we aren't sending a shit ton of messages over the network and clogging up the pipeline
        if (sendDelay <= lastSent)
        {
            lastSent = 0;
            GameObject.Find("Networking").GetComponent<Client>().SendMovement(gameObject.transform.position, velocity);
        }
        else
            lastSent += Time.deltaTime;
    }

    //sets the players position
    public void SetPlayerPosition(Vector3 pos, Quaternion rot)
    {
        controller.enabled = false;
        transform.position = pos;
        transform.rotation = rot;
        controller.enabled = true;
        
        Debug.Log(transform.position);
    }

    //deactivates the chatbox, doesn't actually hide the chat box
    public void hideChatBox()
    {
        chatPanel.GetComponent<ScrollRect>().scrollSensitivity = 0;
        inputFieldActive = false;
        disableInput = false;
    }
}
