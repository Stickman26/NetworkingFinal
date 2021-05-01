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
    public float jumpHeight = 1f;

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

        if(disableInput == false)
        {
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
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (!disableInput)
            {
                disableInput = true;
                chatPanel.GetComponent<ScrollRect>().scrollSensitivity = 1;
            }

            chatBox.GetComponent<TMP_InputField>().ActivateInputField();
            inputFieldActive = true;
        }

        if (inputFieldActive)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if(chatBox.GetComponent<TMP_InputField>().text != "")
                {
                    GameObject sendMessage = GameObject.Find("Networking");

                    string temp = sendMessage.GetComponent<Client>().ourClientId.ToString() + "|" + chatBox.GetComponent<TMP_InputField>().text;

                    sendMessage.GetComponent<Client>().sendMessage(temp, sendMessage.GetComponent<Client>().reliableChannel);
                    chatBox.GetComponent<TMP_InputField>().text = "";
                    chatBox.GetComponent<TMP_InputField>().DeactivateInputField();
                    hideChatBox();
                }
                else
                {
                    chatBox.GetComponent<TMP_InputField>().DeactivateInputField();
                    hideChatBox();
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                chatBox.GetComponent<TMP_InputField>().text = "";
                chatBox.GetComponent<TMP_InputField>().DeactivateInputField();
                hideChatBox();
            }
        }

        GameObject.Find("Networking").GetComponent<Client>().SendMovement(gameObject.transform.position, velocity);
    }

    public void hideChatBox()
    {
        chatPanel.GetComponent<ScrollRect>().scrollSensitivity = 0;
        disableInput = false;
        StartCoroutine("hideChat");
    }

    IEnumerator hideChat()
    {
        yield return new WaitForSecondsRealtime(1.0f);

        chatBox.GetComponentInParent<GameObject>().SetActive(false);
    }
}
