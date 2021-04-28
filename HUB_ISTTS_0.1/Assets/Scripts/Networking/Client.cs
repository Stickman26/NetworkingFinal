using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class Player
{
    public string playerName;
    public GameObject avatar;
    public int connectionID;
}

public class Client : MonoBehaviour
{
    private const int MAX_CONNECTIONS = 4;

    private int port = 5701;

    private int hostId;

    public int reliableChannel;
    private int unreliableChannel;

    public int ourClientId;
    private int connectionId;

    private float connectionTime;
    private bool isConnected = false;
    private bool isStarted = false;
    private byte error;

    private string playerName;

    public GameObject playerPrefab;
    public Dictionary<int, Player> players = new Dictionary<int, Player>();

    //chat vars
    public List<Message> messageList = new List<Message>();
    public int maxMessages = 25;
    public GameObject chatPanel;
    public GameObject textObject;
    public GameObject chatBox;

    public void Connect()
    {

        string pName = GameObject.Find("UsernameField").GetComponent<TMP_InputField>().text;
        if (pName == "")
        {
            Debug.Log("You must enter name!");
            return;
        }

        playerName = pName;

        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTIONS);

        hostId = NetworkTransport.AddHost(topo, 0);
        string IP = GameObject.Find("ServerField").GetComponent<TMP_InputField>().text;

        connectionId = NetworkTransport.Connect(hostId, IP, port, 0, out error);

        connectionTime = Time.time;
        isConnected = true;
    }

    private void Update()
    {
        if (!isConnected)
        {
            return;
        }
        else
        {
            chatBox.SetActive(true);
        }

        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        switch (recData)
        {
            case NetworkEventType.DataEvent:
                {
                    string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    Debug.Log("Receiving : " + msg);

                    string[] splitData = msg.Split('|');

                    switch (splitData[0])
                    {
                        case "ASKNAME": //get username
                            OnAskName(splitData);
                            break;

                        case "CNN": //connection
                            SpawnPlayer(splitData[1], int.Parse(splitData[2]));
                            break;

                        case "DC": //disconnection
                            PlayerDisconnected(int.Parse(splitData[1]));
                            break;

                        case "MOVE":
                            //MovementDetected(int.Parse(splitData[1]), splitData);
                            break;

                        case "ROT":
                            RotationgDetected(int.Parse(splitData[1]), splitData);
                            break;

                        case "MESSAGE":
                            recieveMessage(splitData[1], splitData[2]);
                            break;

                        default: //invalid key case
                            Debug.Log("INVALID CLIENT MESSAGE : " + msg);
                            break;
                    }
                }
                break;

            case NetworkEventType.BroadcastEvent:
                {

                }
                break;
        }

    }

    private void OnAskName(string[] data)
    {
        //set this clients ID
        ourClientId = int.Parse(data[1]);

        //Send our name to the server
        Send("NAMEIS|" + playerName, reliableChannel);

        //Create all the other players
        for (int i = 2; i < data.Length - 1; ++i)
        {
            string[] d = data[i].Split('%');
            SpawnPlayer(d[0], int.Parse(d[1]));
        }
    }

    private void SpawnPlayer(string playerName, int cnnId)
    {
        GameObject go = Instantiate(playerPrefab) as GameObject;

        //Is this ours?
        if (cnnId == ourClientId)
        {
            //Hook up camera
            GameObject cam = GameObject.Find("Main Camera");
            cam.transform.SetParent(go.transform.GetChild(1));
            cam.transform.position = go.transform.GetChild(1).position;
            cam.transform.rotation = go.transform.GetChild(1).rotation;
            //Set up look script
            MouseLook ml = cam.GetComponent<MouseLook>();
            ml.MouseState(true);
            ml.playerBody = go.transform;
            //attach pointer
            Transform pointer = go.transform.GetChild(1).GetChild(0);
            Debug.Log(pointer.name);
            pointer.SetParent(cam.transform);

            //Hook up movement
            go.AddComponent<PlayerMovement>();

            //Disable connection panel and start game
            GameObject.Find("ClientConnectionPanel").SetActive(false);
            isStarted = true;
        }
        else
        {
            //add networked player adapter
            NetworkedPlayerAdapter ap = go.AddComponent<NetworkedPlayerAdapter>();
            ap.body = go.transform;
            ap.lookComponent = go.transform.GetChild(1);
        }

        Player p = new Player();
        p.avatar = go;
        p.playerName = playerName;
        p.connectionID = cnnId;
        p.avatar.GetComponentInChildren<TextMesh>().text = playerName;
        players.Add(cnnId, p);
    }

    private void PlayerDisconnected(int cnnId)
    {
        Destroy(players[cnnId].avatar);
        players.Remove(cnnId);
    }

    private void MovementDetected(int cnnId, string[] data)
    {
        if(players[cnnId] != null)
        {
            NetworkedPlayerAdapter character = players[cnnId].avatar.GetComponent<NetworkedPlayerAdapter>();
            //character.Move();
        }
    }

    private void RotationgDetected(int cnnId, string[] data)
    {
        if (players[cnnId] != null)
        {
            NetworkedPlayerAdapter character = players[cnnId].avatar.GetComponent<NetworkedPlayerAdapter>();
            float xRot = int.Parse(data[2]);
            float yRot = int.Parse(data[3]);
            character.Look(xRot, yRot);
        }
    }

    public void SendRotation(float xRot, float yRot)
    {
        string msg = "ROT|";
        msg += ((int)xRot).ToString() + "|";
        msg += ((int)yRot).ToString();

        Send(msg, unreliableChannel);
    }

    private void Send(string message, int channelID)
    {
        Debug.Log("Sending : " + message);

        byte[] msg = Encoding.Unicode.GetBytes(message);

        NetworkTransport.Send(hostId, connectionId, channelID, msg, message.Length * sizeof(char), out error);
    }

    public void sendMessage(string msg, int channelID)
    {
        msg = "MESSAGE|" + msg;
        Send(msg, channelID);
    }

    public void recieveMessage(string sentID, string msg)
    {
        if(messageList.Count >= maxMessages)
        {
            Destroy(messageList[0].textObject.gameObject);
            messageList.Remove(messageList[0]);
        }

        if(chatBox.activeSelf == false)
        {
            chatBox.SetActive(true);
        }

        Message msgToPrint = new Message();
        msgToPrint.text = sentID + ": " + msg;

        GameObject newText = Instantiate(textObject, chatPanel.transform);

        msgToPrint.textObject = newText.GetComponent<TextMeshProUGUI>();
        msgToPrint.textObject.text = msgToPrint.text;

        messageList.Add(msgToPrint);
    }

    public class Message
    {
        public string text;
        public TextMeshProUGUI textObject;
    }
}
