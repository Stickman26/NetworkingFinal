using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedPlayerAdapter : MonoBehaviour
{
    public Transform lookComponent;
    public Transform body;
    //public Rigidbody rb;
    
    private Vector3 PosTo;
    private Quaternion Rotx;
    private Quaternion Roty;

    private float lerpVal = 0.1f;

    //sets the values so that they're stored locally
    public void Move(Vector3 pos, Vector3 vel)
    {
        PosTo = pos;
        //rb.velocity = vel;
    }

    //sets the values so that they're stored locally
    public void Look(float xRot, float yRot)
    {
        Rotx = Quaternion.Euler(xRot, 0f, 0f);
        Roty = Quaternion.Euler(0f, yRot, 0f);
    }

    void Start()
    {
        PosTo = body.position;
    }

    void Update()
    {
        //Movement
        if (Vector3.Magnitude(PosTo - body.position) < 4f)
        {
            //Lerp between last known pos and the newly recieved pos
            body.position = Vector3.Lerp(body.position, PosTo, lerpVal);
        }
        else
        {
            body.position = PosTo;
        }

        //Rotation, lerp between last known rotation and the newlt recieved rotation
        lookComponent.localRotation = Quaternion.Lerp(lookComponent.localRotation, Rotx, lerpVal);
        body.rotation = Quaternion.Lerp(body.rotation, Roty, lerpVal);
    }
}
