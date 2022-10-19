using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    public UnityEvent moveEnemies;
    public float moveSpeed = 5f;
    public Transform movePoint;
    public Transform flashLight;
    public LayerMask cantMove;
    public EnemyManager _enemyManager;
    public int flashlightState = 0;

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
                // moveEnemies.Invoke();
                StartCoroutine(_enemyManager.moveAgents());
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {

            switch (flashlightState)
            {
                case (0):
                    flashlightState = 1;
                    flashLight.localPosition = new Vector3(1.2f, -1.2f, 0f);
                    flashLight.rotation = Quaternion.Euler(0, 0, -45f);
                    break;

                case (1):
                    flashlightState = 2;
                    flashLight.localPosition = new Vector3(0f, -1.5f, 0f);
                    flashLight.rotation = Quaternion.Euler(0, 0, 90f);
                    break;
                case (2):
                    flashlightState = 3;
                    flashLight.localPosition = new Vector3(-1.2f, -1.2f, 0f);
                    flashLight.rotation = Quaternion.Euler(0, 0, 45);
                    break;
                case (3):
                    flashlightState = 4;
                    flashLight.localPosition = new Vector3(-1.5f, 0f, 0f);
                    flashLight.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case (4):
                    flashlightState = 5;
                    flashLight.localPosition = new Vector3(-1.2f, 1.2f, 0f);
                    flashLight.rotation = Quaternion.Euler(0, 0, -45f);
                    break;
                case (5):
                    flashlightState = 6;
                    flashLight.localPosition = new Vector3(0f, 1.5f, 0f);
                    flashLight.rotation = Quaternion.Euler(0, 0, 90f);
                    break;
                case (6):
                    flashlightState = 7;
                    flashLight.localPosition = new Vector3(1.2f, 1.2f, 0f);
                    flashLight.rotation = Quaternion.Euler(0, 0, 45f);
                    break;
                case (7):
                    flashlightState = 0;
                    flashLight.localPosition = new Vector3(1.5f, 0f, 0f);
                    flashLight.rotation = Quaternion.Euler(0, 0, 0f);
                    break;
                default:
                    break;
            }
        }
            if (Input.GetKeyDown(KeyCode.Q))
            {

                switch (flashlightState)
                {
                    case (0):
                        flashlightState = 7;
                        flashLight.localPosition = new Vector3(1.2f, 1.2f, 0f);
                        flashLight.rotation = Quaternion.Euler(0, 0, 45f);
                        break;
                                              

                    case (1):
                        flashlightState = 0;
                        flashLight.localPosition = new Vector3(1.5f, 0f, 0f);
                        flashLight.rotation = Quaternion.Euler(0, 0, 0f);
                        break;

                        
                    case (2):
                        flashlightState = 1;
                        flashLight.localPosition = new Vector3(1.2f, -1.2f, 0f);
                        flashLight.rotation = Quaternion.Euler(0, 0, -45f);
                        break;

                        
                    case (3):
                        flashlightState = 2;
                        flashLight.localPosition = new Vector3(0f, -1.5f, 0f);
                        flashLight.rotation = Quaternion.Euler(0, 0, 90f);
                        break;
                        
                    case (4):
                        flashlightState = 3;
                        flashLight.localPosition = new Vector3(-1.2f, -1.2f, 0f);
                        flashLight.rotation = Quaternion.Euler(0, 0, 45);
                        break;
                        
                    case (5):
                        flashlightState = 4;
                        flashLight.localPosition = new Vector3(-1.5f, 0f, 0f);
                        flashLight.rotation = Quaternion.Euler(0, 0, 0);
                        break;
                        
                    case (6):
                        flashlightState = 5;
                        flashLight.localPosition = new Vector3(-1.2f, 1.2f, 0f);
                        flashLight.rotation = Quaternion.Euler(0, 0, -45f);
                        break;                       
                    case (7):
                        flashlightState = 6;
                        flashLight.localPosition = new Vector3(0f, 1.5f, 0f);
                        flashLight.rotation = Quaternion.Euler(0, 0, 90f);
                        break;

                    default:
                        break;
                }
            }
    }

    

    


}
