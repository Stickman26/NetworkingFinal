using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Net;

public class ServerInterface : MonoBehaviour
{
    private string addressStart = "Network Address: ";
    [SerializeField]
    TMP_Text addressHolder;
    [SerializeField]
    TMP_Text consoleOutput;

    private void Start()
    {
        //check the public IP and display it
        string address = CheckIP();
        addressHolder.text = addressStart + address;
    }

    //retrieve the local IP
    private string GetLocalIP()
    {
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }

    //add output to the server's console
    public void AddToConsole(string output)
    {
        consoleOutput.text += output;
        consoleOutput.text += "\n";
    }

    //check the public IP
    public string CheckIP()
    {
        //we reach out to a website which responds with the current machine's public IP along with some other information that we don't need
        string externalIpString = new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
        //store only the public IP
        var externalIp = IPAddress.Parse(externalIpString);

        return externalIp.ToString();
    }
}
