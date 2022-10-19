using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    public UnityEvent moveEnemies;
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

        if (Vector3.Distance(transform.position, movePoint.position) <= 0.05f)
        {            
            
            if (Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1)
            {
                if (!Physics2D.OverlapCircle(movePoint.position + new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f), .2f, cantMove)) 
                {                   
                    movePoint.position += new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f);
                }
            }

            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) == 1)
            {
                if (!Physics2D.OverlapCircle(movePoint.position + new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f), .2f, cantMove))
                {
                    movePoint.position += new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f);
                }
            }
            if ((Mathf.Abs(Input.GetAxisRaw("Horizontal")) == 1) || (Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1))
            {
                Debug.Log("Invoacato");
                moveEnemies.Invoke();
            }
        }
    }
}
