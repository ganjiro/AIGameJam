using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Transform movePoint;
    public LayerMask cantMove;
    public GameObject goal;
    public bool _isMoving = false;

    private VictimAgent _agentComponent;
    private float _movementThreshold = 0.005f;

    // Start is called before the first frame update
    void Start()
    {
        movePoint.parent = null;
        _agentComponent = GetComponent<VictimAgent>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);
        
        // Give instantaneous reward only if the movement has finished
        if (_isMoving && Vector3.Distance(transform.position, movePoint.position) < _movementThreshold)
        {
            if(_agentComponent != null)
                Debug.Log("Add Reward");            
        }
        
        // Give the reward to the agent
        if (_agentComponent != null && goal.transform.position == transform.position)
        {
            // Destroy(transform.gameObject);
            if (Regenerate.instance._training)
            {
                Debug.Log("Add Reward 10");
                Debug.Log("YEAHYEAHYEAH");
                _agentComponent.EndEpisode();
            }
            else
            {
                Regenerate.instance.RemoveEnemyFromPool(gameObject);
            }
        }

        // Store if this gameobject is moving or has finished the movement
        _isMoving = Vector3.Distance(transform.position, movePoint.position) >= _movementThreshold;
    }

    public void randomMovement()
    {
        if (Vector3.Distance(transform.position, movePoint.position) <= _movementThreshold)
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

    public void actionMovement(int action)
    {
        switch (action)
        {
            case 0:
                moveN();
                break;
            case 1:
                moveS();
                break;
            case 2:
                moveE();
                break;
            case 3:
                moveW();
                break;
            default:
                moveN();
                break;
        }

        _isMoving = true;
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
