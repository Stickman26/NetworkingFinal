using System.Collections;
using System.Collections.Generic;
using System.Text;
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
                    string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    //Debug.Log("Receiving from " + connectionId + " : " + msg);
                    serverConsole.AddToConsole("Receiving from " + connectionId + " : " + msg);

                    string[] splitData = msg.Split('|');

                    switch (splitData[0])
                    {
                        case "NAMEIS":
                            OnNameIs(connectionId, splitData[1]);
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
        string msg = "ASKNAME|" + cnnId + "|";
        foreach(ServerClient sc in clients)
            msg += sc.playerName + '%' + sc.connectionID + "|";

        msg = msg.Trim('|');

        Send(msg,reliableChannel,cnnId);
    }

    private void OnDisconnection(int cnnId)
    {
        //Remove this player from client list
        clients.Remove(clients.Find(x => x.connectionID == cnnId));

        //Tell everyone that somebody else has disconnected
        Send("DC|" + cnnId, reliableChannel, clients);
    }

    private void OnNameIs(int cnnId, string playerName)
    {
        //Link name to connection ID
        clients.Find(x => x.connectionID == cnnId).playerName = playerName;

        //Tell everyone about new player connection
        Send("CNN|" + playerName + "|" + cnnId, reliableChannel, clients);
    }

    //Send
    private void Send(string message, int channelID, int cnnID)
    {
        List<ServerClient> c = new List<ServerClient>();
        c.Add(clients.Find(x => x.connectionID == cnnID));

        Send(message, channelID, c);
    }

    private void Send(string message, int channelID, List<ServerClient> c)
    {
        //Debug.Log("Sending : " + message);
        serverConsole.AddToConsole("Sending : " + message);

        byte[] msg = Encoding.Unicode.GetBytes(message);
        foreach (ServerClient sc in c)
        {
            NetworkTransport.Send(hostId, sc.connectionID, channelID, msg, message.Length * sizeof(char), out error);
        }
    }
}
