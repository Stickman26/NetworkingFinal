using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMineBot : MonoBehaviour
{
    private int clientId;
    private Client netClient;
    public int id;
    public bool isControlled = false;

    [SerializeField]
    float distance = 5f;

    private Vector3 pos1;
    private Vector3 pos2;
    //private bool switchBack = false;

    private Vector3 PosTo;

    //Timing
    //private float sendDelay = 0.1f;
    //private float lastSent;

    // Start is called before the first frame update
    void Start()
    {
        netClient = GameObject.Find("Networking").GetComponent<Client>();
        clientId = netClient.ourClientId;

        pos2 = transform.position;
        PosTo = pos2;
        pos1 = transform.position + (transform.forward * distance);
    }

    public void SendAIInitalData()
    {
        netClient.SetupAI(id, pos1, pos2);
    }

    //Depricated Function
    /*
    public void CheckIsControlled()
    {
        Debug.Log(id % netClient.players.Count);
        isControlled = (clientId == (id % netClient.players.Count));
    }
    */

    // Update is called once per frame
    void Update()
    {
        //if (isControlled)
            //Move();
        //else
        transform.position = Vector3.Lerp(transform.position, PosTo, 0.9f);
    }

    //Depricated Function
    /*
    private void Move()
    {
        if (!switchBack)
        {
            transform.position = Vector3.MoveTowards(transform.position, pos1, 0.01f);
            if (Vector3.Magnitude(transform.position - pos1) < 0.1f)
            {
                switchBack = true;
            }
        }
        else
        {
            //transform.position = Vector3.Lerp(pos2, transform.position, 0.2f);
            transform.position = Vector3.MoveTowards(transform.position, pos2, 0.01f);
            if (Vector3.Magnitude(transform.position - pos2) < 0.1f)
            {
                switchBack = false;
            }
        }

        if (sendDelay <= lastSent && netClient.isConnected)
        {
            lastSent = 0;
            netClient.AIMove(id, gameObject.transform.position);
        }
        else
            lastSent += Time.deltaTime;
    }*/

    public void RecieveMove(Vector3 pos)
    {
        PosTo = pos;
    }

    public void KillButton()
    {

    }
}
