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

    public void Move(Vector3 pos, Vector3 vel)
    {
        PosTo = pos;
        //rb.velocity = vel;
    }

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
        body.position = Vector3.Lerp(body.position, PosTo, lerpVal);
        lookComponent.localRotation = Quaternion.Lerp(lookComponent.localRotation, Rotx, lerpVal);
        body.rotation = Quaternion.Lerp(body.rotation, Roty, lerpVal);
    }
}
