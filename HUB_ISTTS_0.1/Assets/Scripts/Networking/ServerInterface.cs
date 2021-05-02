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
        string address = CheckIP();
        addressHolder.text = addressStart + address;
    }

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

    public void AddToConsole(string output)
    {
        consoleOutput.text += output;
        consoleOutput.text += "\n";
    }

    public string CheckIP()
    {

        string externalIpString = new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
        var externalIp = IPAddress.Parse(externalIpString);

        return externalIp.ToString();
    }
}
