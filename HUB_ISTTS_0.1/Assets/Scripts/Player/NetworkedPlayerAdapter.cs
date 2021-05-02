using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedPlayerAdapter : MonoBehaviour
{
    public Transform lookComponent;
    public Transform body;
    //public Rigidbody rb;
    public Vector3 lastPos;

    public void Move(Vector3 pos, Vector3 vel)
    {
        if(lastPos == null)
        {
            lastPos = new Vector3(0.0f, 0.0f, 0.0f);
        }

        body.transform.position = pos;
        //rb.velocity = vel;

        body.transform.position = Vector3.Lerp(lastPos, pos, 0.1f);

        lastPos = pos;
    }

    public void Look(float xRot, float yRot)
    {
        lookComponent.localRotation = Quaternion.Euler(xRot, 0f, 0f);
        body.rotation = Quaternion.Euler(0f, yRot, 0f);
    }

}
