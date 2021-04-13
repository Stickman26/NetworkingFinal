﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
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

    private int reliableChannel;
    private int unreliableChannel;

    private int ourClientId;
    private int connectionId;

    private float connectionTime;
    private bool isConnected = false;
    private bool isStarted = false;
    private byte error;

    private string playerName;

    public GameObject playerPrefab;
    public List<Player> players = new List<Player>();

    public void Connect()
    {

        string pName = "";
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
        connectionId = NetworkTransport.Connect(hostId, "LOCALHOST", port, 0, out error);

        connectionTime = Time.time;
        isConnected = true;
    }

    private void Update()
    {
        if (!isConnected)
            return;

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

                    switch(splitData[0])
                    {
                        case "ASKNAME": //get username
                            OnAskName(splitData);
                            break;

                        case "CNN": //connection
                            SpawnPlayer(splitData[1], int.Parse(splitData[2]));
                            break;

                        case "DC": //disconnection
                            break;

                        default: //invalid key case
                            Debug.Log("INVALID MESSAGE : ");
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

    private void SpawnPlayer(string name, int cnnId)
    {
        GameObject go = Instantiate(playerPrefab) as GameObject;

        //Is this ours?
       if (cnnId == ourClientId)
        {
            //Remove menu
            isStarted = true;
        }

        Player p = new Player();
        p.avatar = go;
        p.playerName = playerName;
        p.connectionID = cnnId;
        p.avatar.GetComponentInChildren<TextMesh>().text = playerName;
        players.Add(p);
    }

    private void Send(string message, int channelID)
    {
        Debug.Log("Sending : " + message);

        byte[] msg = Encoding.Unicode.GetBytes(message);

        NetworkTransport.Send(hostId, connectionId, channelID, msg, message.Length * sizeof(char), out error);
    }
}