using System;
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
                    NetworkStructs.MessageTypes type = (NetworkStructs.MessageTypes)recBuffer[0];

                    byte[] packet = new byte[1024];
                    Array.Copy(recBuffer, 1, packet, 0, bufferSize - 1);

                    switch (type)
                    {
                        case NetworkStructs.MessageTypes.ASKNAME: //get username
                            {
                                NetworkStructs.NameRequestData data = NetworkStructs.fromBytes<NetworkStructs.NameRequestData>(packet);
                                OnAskName(data);
                            }
                            break;

                        case NetworkStructs.MessageTypes.CNN: //connection
                            {
                                NetworkStructs.NameRequestData data = NetworkStructs.fromBytes<NetworkStructs.NameRequestData>(packet);
                                SpawnPlayer(data.str, data.id);
                            }
                            break;

                        case NetworkStructs.MessageTypes.DC: //disconnection
                            {
                                NetworkStructs.IntData data = NetworkStructs.fromBytes<NetworkStructs.IntData>(packet);
                                PlayerDisconnected(data.data);
                            }
                            break;

                        case NetworkStructs.MessageTypes.MOVE:
                            {
                                NetworkStructs.PositionVelocityData data = NetworkStructs.fromBytes<NetworkStructs.PositionVelocityData>(packet);
                                MovementDetected(data);
                            }
                            break;

                        case NetworkStructs.MessageTypes.ROT:
                            {
                                NetworkStructs.RotationData data = NetworkStructs.fromBytes<NetworkStructs.RotationData>(packet);
                                RotationgDetected(data);
                            }
                            break;

                        case NetworkStructs.MessageTypes.MESSAGE:
                            {
                                NetworkStructs.StringData data = NetworkStructs.fromBytes<NetworkStructs.StringData>(packet);
                                recieveMessage(data);
                            }
                            break;

                        case NetworkStructs.MessageTypes.ADMIN: 
                            {
                                NetworkStructs.StringData data = NetworkStructs.fromBytes<NetworkStructs.StringData>(packet);
                                adminCommands(data.str, ourClientId);
                            }
                            break;

                        default: //invalid key case
                            //Debug.Log("INVALID CLIENT MESSAGE : " + msg);
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

    private void OnAskName(NetworkStructs.NameRequestData data)
    {
        //set this clients ID
        ourClientId = data.id;

        NetworkStructs.StringData msg = new NetworkStructs.StringData(playerName);

        //Send our name to the server
        Send(NetworkStructs.AddTag(NetworkStructs.MessageTypes.NAMEIS, NetworkStructs.getBytes(msg)), reliableChannel);

        string[] connectionList = data.str.Split('|');

        //Create all the other players
        for (int i = 0; i < connectionList.Length; ++i)
        {
            string[] d = connectionList[i].Split('%');
            if (int.Parse(d[1]) != ourClientId)
            {
                SpawnPlayer(d[0], int.Parse(d[1]));
            }
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

    private void MovementDetected(NetworkStructs.PositionVelocityData data)
    {
        if(data.id != ourClientId && players[data.id] != null)
        {
            NetworkedPlayerAdapter character = players[data.id].avatar.GetComponent<NetworkedPlayerAdapter>();
            Vector3 pos = new Vector3(data.xPos, data.yPos, data.zPos);
            Vector3 vel = new Vector3(data.xVel, data.yVel, data.zVel);

            character.Move(pos, vel);
        }
    }

    private void RotationgDetected(NetworkStructs.RotationData data)
    {
        if (data.id != ourClientId && players[data.id] != null)
        {
            NetworkedPlayerAdapter character = players[data.id].avatar.GetComponent<NetworkedPlayerAdapter>();
            float xRot = data.xRot;
            float yRot = data.yRot;
            character.Look(xRot, yRot);
        }
    }

    public void SendRotation(float xRot, float yRot)
    {
        NetworkStructs.RotationData msg = new NetworkStructs.RotationData(ourClientId, xRot, yRot);

        Send(NetworkStructs.AddTag(NetworkStructs.MessageTypes.ROT, NetworkStructs.getBytes(msg)), unreliableChannel);
    }

    public void SendMovement(Vector3 pos, Vector3 vel)
    {
        NetworkStructs.PositionVelocityData msg = new NetworkStructs.PositionVelocityData(ourClientId, pos, vel);

        Send(NetworkStructs.AddTag(NetworkStructs.MessageTypes.MOVE, NetworkStructs.getBytes(msg)), unreliableChannel);
    }

    public void sendMessage(string msg, int channelID)
    {
        if (msg.StartsWith("/"))
        {
            NetworkStructs.StringData message = new NetworkStructs.StringData(msg);
            Send(NetworkStructs.AddTag(NetworkStructs.MessageTypes.ADMIN, NetworkStructs.getBytes(message)), channelID);
        }
        else
        {
            msg = "[" + playerName + "]: " + msg;
            NetworkStructs.StringData message = new NetworkStructs.StringData(msg);

            Send(NetworkStructs.AddTag(NetworkStructs.MessageTypes.MESSAGE, NetworkStructs.getBytes(message)), channelID);
        }
    }

    public void recieveMessage(NetworkStructs.StringData data)
    {

        if(chatBox.activeSelf == false)
        {
            chatBox.SetActive(true);
        }

        chatPanel.GetComponent<TextMeshProUGUI>().text += "\n";
        chatPanel.GetComponent<TextMeshProUGUI>().text += data.str;
    }

    private void Send(byte[] msg, int channelID)
    {
        //Debug.Log("Sending : " + message);

        
        NetworkTransport.Send(hostId, connectionId, channelID, msg, msg.Length, out error);
    }

    private void adminCommands(string msg, int cnnID)
    {
        if (msg.StartsWith("1"))
        {
            //set jump height
            string height = msg.Substring(1);
            Player localPlayer;

            if (players.TryGetValue(cnnID, out localPlayer))
            {
                localPlayer.avatar.GetComponent<PlayerMovement>().jumpHeight = int.Parse(height);
            }
            else
            {
                Debug.Log("Unable to find local player!");
            }
        }
        else if (msg.StartsWith("2"))
        {
            //set move speed
            string speed = msg.Substring(1);
            Player localPlayer;

            if (players.TryGetValue(cnnID, out localPlayer))
            {
                localPlayer.avatar.GetComponent<PlayerMovement>().speedMod = int.Parse(speed);
            }
            else
            {
                Debug.Log("Unable to find local player!");
            }
        }
        else
        {
            //unrecognized command (don't necessarily need this if we set the server to respond)
        }
    }
}
