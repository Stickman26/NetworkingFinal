using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedPlayerAdapter : MonoBehaviour
{
    public Transform lookComponent;
    public Transform body;

    public void Move(Vector3 pos, Vector3 vel)
    {

    }

    public void Look(float xRot, float yRot)
    {
        lookComponent.localRotation = Quaternion.Euler(xRot, 0f, 0f);
        body.rotation = Quaternion.Euler(0f, yRot, 0f);
    }

}
