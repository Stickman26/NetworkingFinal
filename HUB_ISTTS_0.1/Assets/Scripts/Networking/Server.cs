using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class ServerClient
{
    public int connectionID;
    public string playerName;
}

public class Server : MonoBehaviour
{
    private const int MAX_CONNECTIONS = 4;

    private int port = 5701;
    
    private int hostId;

    private int reliableChannel;
    private int unreliableChannel;

    private bool isStarted = false;
    private byte error;

    private List<ServerClient> clients = new List<ServerClient>();

    private ServerInterface serverConsole;

    private void Start()
    {
        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTIONS);

        hostId = NetworkTransport.AddHost(topo, port, null);

        isStarted = true;

        serverConsole = gameObject.GetComponent<ServerInterface>();
    }

    private void Update()
    {
        if (!isStarted)
        {
            return;
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
            //case NetworkEventType.Nothing: break;

            case NetworkEventType.ConnectEvent:
                {
                    //Debug.Log("Player " + connectionId + " has connected");
                    serverConsole.AddToConsole("Player " + connectionId + " has connected");
                    OnConnection(connectionId);
                }
                break;

            case NetworkEventType.DataEvent:
                {
                    NetworkStructs.MessageTypes type = (NetworkStructs.MessageTypes)recBuffer[0];

                    //string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    //Debug.Log("Receiving from " + connectionId + " : " + msg);
                    //serverConsole.AddToConsole("Receiving from " + connectionId + " : " + msg);

                    byte[] packet = new byte[1024];
                    Array.Copy(recBuffer, 1, packet, 0, bufferSize - 1);

                    switch (type)
                    {
                        case NetworkStructs.MessageTypes.NAMEIS:
                            {
                                NetworkStructs.StringData data = NetworkStructs.fromBytes<NetworkStructs.StringData>(packet);
                                OnNameIs(connectionId, data.str);
                            }
                            break;

                        case NetworkStructs.MessageTypes.ROT:
                            {
                                NetworkStructs.RotationData data = NetworkStructs.fromBytes<NetworkStructs.RotationData>(packet);
                                OnRotationChange(connectionId, data);
                            }
                            break;

                        case NetworkStructs.MessageTypes.MOVE:
                            {
                                NetworkStructs.PositionVelocityData data = NetworkStructs.fromBytes<NetworkStructs.PositionVelocityData>(packet);
                                OnMovement(connectionId, data);
                            }
                            break;

                        case NetworkStructs.MessageTypes.MESSAGE:
                            Send(recBuffer, reliableChannel);
                            break;

                        case NetworkStructs.MessageTypes.ADMIN:
                            {
                                NetworkStructs.StringData data = NetworkStructs.fromBytes<NetworkStructs.StringData>(packet);
                                adminCommands(data.str, connectionId);
                            }
                            break;

                        default: //invalid key case
                            //Debug.Log("INVALID SERVER MESSAGE : ");
                            serverConsole.AddToConsole("INVALID SERVER MESSAGE : ");
                            break;
                    }
                }
                break;

            case NetworkEventType.DisconnectEvent:
                {
                    //Debug.Log("Player " + connectionId + " has disconnected");
                    serverConsole.AddToConsole("Player " + connectionId + " has disconnected");
                    OnDisconnection(connectionId);
                }
                break;

            case NetworkEventType.BroadcastEvent:
                {

                }
                break;
        }

    }

    private void OnConnection(int cnnId)
    {
        // add connection to list
        ServerClient c = new ServerClient();
        c.connectionID = cnnId;
        c.playerName = "TEMP";
        clients.Add(c);

        // when player joins give ID
        // request name and send name of other players
        string list = "";
        foreach(ServerClient sc in clients)
            list += sc.playerName + '%' + sc.connectionID + "|";
        list = list.Trim('|');

        NetworkStructs.NameRequestData msg = new NetworkStructs.NameRequestData(cnnId, list);

        Send(NetworkStructs.AddTag(NetworkStructs.MessageTypes.ASKNAME, NetworkStructs.getBytes(msg)),reliableChannel,cnnId);
    }

    private void OnDisconnection(int cnnId)
    {
        //Remove this player from client list
        clients.Remove(clients.Find(x => x.connectionID == cnnId));

        NetworkStructs.IntData msg = new NetworkStructs.IntData(cnnId);

        //Tell everyone that somebody else has disconnected
        Send(NetworkStructs.AddTag(NetworkStructs.MessageTypes.DC, NetworkStructs.getBytes(msg)), reliableChannel, clients);
    }

    private void OnNameIs(int cnnId, string playerName)
    {
        //Link name to connection ID
        clients.Find(x => x.connectionID == cnnId).playerName = playerName;

        NetworkStructs.NameRequestData msg = new NetworkStructs.NameRequestData(cnnId, playerName);

        //Tell everyone about new player connection
        Send(NetworkStructs.AddTag(NetworkStructs.MessageTypes.CNN, NetworkStructs.getBytes(msg)), reliableChannel, clients);
    }

    private void OnRotationChange(int cnnID, NetworkStructs.RotationData data)
    {
        //NetworkStructs.DataPacket packet = new NetworkStructs.DataPacket(NetworkStructs.MessageTypes.CNN, NetworkStructs.getBytes(data));

        Send(NetworkStructs.AddTag(NetworkStructs.MessageTypes.ROT, NetworkStructs.getBytes(data)), unreliableChannel, clients, cnnID);
    }

    private void OnMovement(int cnnID, NetworkStructs.PositionVelocityData data)
    {
        Send(NetworkStructs.AddTag(NetworkStructs.MessageTypes.MOVE, NetworkStructs.getBytes(data)), unreliableChannel, clients, cnnID);
    }

    //Send
    //Send to sender
    private void Send(byte[] msg, int channelID, int cnnID)
    {
        List<ServerClient> c = new List<ServerClient>();
        c.Add(clients.Find(x => x.connectionID == cnnID));

        Send(msg, channelID, c);
    }

    //Client list minus sender
    private void Send(byte[] msg, int channelID, List<ServerClient> c, int cnnID)
    {
        List<ServerClient> recievers = new List<ServerClient>();
        recievers.AddRange(clients);
        recievers.Remove(recievers.Find(x => x.connectionID == cnnID));

        Send(msg, channelID, recievers);
    }

    //Send to list
    private void Send(byte[] msg, int channelID, List<ServerClient> c)
    {
        //Debug.Log("Sending : " + message);
        //serverConsole.AddToConsole("Sending : " + message);  

        //serverConsole.AddToConsole("to : ");

        //byte[] msg = Encoding.Unicode.GetBytes(message);
        foreach (ServerClient sc in c)
        {
            NetworkTransport.Send(hostId, sc.connectionID, channelID, msg, msg.Length, out error);
            //serverConsole.AddToConsole(sc.playerName);
        }
    }   
    
    //Send to all players
    private void Send(byte[] msg, int channelID)
    {
        //Debug.Log("Sending : " + message);
        //serverConsole.AddToConsole("Sending : " + message);

        //message = "MESSAGE|" + message;

        //serverConsole.AddToConsole("to all players");

        //byte[] msg = Encoding.Unicode.GetBytes(message);
        foreach (ServerClient sc in clients)
        {
            NetworkTransport.Send(hostId, sc.connectionID, channelID, msg, msg.Length, out error);
        }
    }

    private void adminCommands(string msg, int cnnID)
    {
        if (msg.Contains("/setjump"))
        {
            //actually set the jump and send a response back to the specific player (or all players?)
            string number = msg.Substring(8);
            NetworkStructs.StringData data = new NetworkStructs.StringData("Setting player jump height to: " + number);
            serverConsole.AddToConsole("Setting player jump height to: " + number);
            Send(NetworkStructs.getBytes(data), reliableChannel, cnnID);

            string messageToSend = "1";
            messageToSend += number;
            data = new NetworkStructs.StringData(messageToSend);
            Send(NetworkStructs.AddTag(NetworkStructs.MessageTypes.ADMIN, NetworkStructs.getBytes(data)), reliableChannel);
        }
        else if (msg.Contains("/setspeed"))
        {
            //actually change the player speed and send a response back to the specific player (or all players?)
            string number = msg.Substring(9);
            NetworkStructs.StringData data = new NetworkStructs.StringData("Setting player move speed to: " + number);
            serverConsole.AddToConsole("Setting player move speed to: " + number);
            Send(NetworkStructs.getBytes(data), reliableChannel, cnnID);

            string messageToSend = "2";
            messageToSend += number;
            data = new NetworkStructs.StringData(messageToSend);
            Send(NetworkStructs.AddTag(NetworkStructs.MessageTypes.ADMIN, NetworkStructs.getBytes(data)), reliableChannel);
        }
        else
        {
            //send message back to specific player saying that the specified command doesn't exist
            NetworkStructs.StringData data = new NetworkStructs.StringData("Command: " + msg + " :Doesn't exist!");
            serverConsole.AddToConsole("Command: " + msg + " :Doesn't exist!");

            //this will send to just the player who sent the commnad (i think)
            Send(NetworkStructs.getBytes(data), reliableChannel, cnnID);
        }
    }
}
