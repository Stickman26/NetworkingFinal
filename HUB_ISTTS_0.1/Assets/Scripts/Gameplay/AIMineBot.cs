using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMineBot : MonoBehaviour
{
    public int id;
    public bool isControlled = true;

    [SerializeField]
    float distance = 5f;

    private Vector3 pos1;
    private Vector3 pos2;
    private bool switchBack = false;

    // Start is called before the first frame update
    void Start()
    {
        pos2 = transform.position;
        pos1 = transform.position + (transform.forward * distance);
    }

    // Update is called once per frame
    void Update()
    {
        if (isControlled)
            Move();
    }

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
    }

    //Lerp(pos1, transform.position, 0.2f);

    public void KillButton()
    {

    }
}
