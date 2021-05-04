using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGame : MonoBehaviour
{
    public Transform spawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Enter");
        if (other.gameObject.GetComponentInParent<PlayerMovement>())
        {
            //Debug.Log("Hit");
            other.gameObject.GetComponentInParent<PlayerMovement>().SetPlayerPosition(spawnPoint.position, spawnPoint.rotation);
        }
    }
}
