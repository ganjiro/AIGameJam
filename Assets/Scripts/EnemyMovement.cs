using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Transform movePoint;
    public LayerMask cantMove;

    // Start is called before the first frame update
    void Start()
    {
        movePoint.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);
    }

    public void randomMovement()
    {
        if (Vector3.Distance(transform.position, movePoint.position) <= 0.05f)
        {
            int x = Random.Range(0, 5);

            if (x == 0)
            {
                moveN();
            }
            else if (x == 1)
            {
                moveS();
            }
            else if (x == 2)
            {
                moveE();
            }
            else if (x == 3)
            {
                moveW();
            }
        }
    }

    public void moveN()
    {
        if (!Physics2D.OverlapCircle(movePoint.position + new Vector3(0f, 1f, 0f), .2f, cantMove))
        {
            movePoint.position += new Vector3(0f, 1f, 0f);
        }
    }

    public void moveS()
    {
        if (!Physics2D.OverlapCircle(movePoint.position + new Vector3(0f, -1f, 0f), .2f, cantMove))
        {
            movePoint.position += new Vector3(0f, -1f, 0f);
        }
    }

    public void moveW()
    {
        if (!Physics2D.OverlapCircle(movePoint.position + new Vector3(-1f, 0f, 0f), .2f, cantMove))
        {
            movePoint.position += new Vector3(-1f, 0f, 0f);
        }
    }

    public void moveE()
    {
        if (!Physics2D.OverlapCircle(movePoint.position + new Vector3(1f, 0f, 0f), .2f, cantMove))
        {
            movePoint.position += new Vector3(1f, 0f, 0f);
        }
    }
}
