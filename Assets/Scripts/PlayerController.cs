﻿using System;
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
    public Transform positionalLight;
    public LayerMask cantMove;
    public EnemyManager _enemyManager;
    public int flashlightState = 0;
    public int oldFlashlightState = 0;

    // Start is called before the first frame update
    void Start()
    {
        movePoint.parent = null;     
    }

    public void MakeAction(int action)
    {
        switch (action)
        {
            // Movement action
            case 0:                                 
                movePoint.position += new Vector3(1f, 0f, 0f);               
                break;
            case 1:
                movePoint.position += new Vector3(1f, -1f, 0f);
                break;
            case 2:
                movePoint.position += new Vector3(0f, -1f, 0f);
                break;
            case 3:
                movePoint.position += new Vector3(-1f, -1f, 0f);
                break;
            case 4:
                movePoint.position += new Vector3(-1f, 0f, 0f);
                break;
            case 5:
                movePoint.position += new Vector3(-1f, 1f, 0f);
                break;
            case 6:
                movePoint.position += new Vector3(0f, 1f, 0f);
                break;
            case 7:
                movePoint.position += new Vector3(1f, 1f, 0f);
                break;
            case 8:               
                break;
            case 9:
                TurnFlashlightRight();
                break;
            case 10:
                TurnFlashlightLeft();
                break;
            default:
                break;
        }
        GetComponent<EnemyMovement>()._isMoving = true;

    }

    public void randomFlashLightOrientation()
    {
        int n = UnityEngine.Random.Range(0, 8);

        for(int i = 0; i < n; i++)
        {
            TurnFlashlightRight();
        }
    }

    private void TurnFlashlightRight()
    {
        switch (flashlightState)
        {
            case (0):
                oldFlashlightState = 0;
                flashlightState = 1;
                flashLight.localPosition = new Vector3(1.2f, -1.2f, 0f);
                flashLight.rotation = Quaternion.Euler(0, 0, -45f);

                break;
            case (1):
                oldFlashlightState = 1;
                flashlightState = 2;
                flashLight.localPosition = new Vector3(0f, -1.5f, 0f);
                flashLight.rotation = Quaternion.Euler(0, 0, 90f);

                positionalLight.localPosition = new Vector3(0f, -0.5f, 0f);
                positionalLight.rotation = Quaternion.Euler(0, 0, 90f);
                break;
            case (2):
                oldFlashlightState = 2;
                flashlightState = 3;
                flashLight.localPosition = new Vector3(-1.2f, -1.2f, 0f);
                flashLight.rotation = Quaternion.Euler(0, 0, 45);
                break;
            case (3):
                oldFlashlightState = 3;
                flashlightState = 4;
                flashLight.localPosition = new Vector3(-1.5f, 0f, 0f);
                flashLight.rotation = Quaternion.Euler(0, 0, 0);

                positionalLight.localPosition = new Vector3(-0.5f, 0f, 0f);
                positionalLight.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case (4):
                oldFlashlightState = 4;
                flashlightState = 5;
                flashLight.localPosition = new Vector3(-1.2f, 1.2f, 0f);
                flashLight.rotation = Quaternion.Euler(0, 0, -45f);
                break;
            case (5):
                oldFlashlightState = 5;
                flashlightState = 6;
                flashLight.localPosition = new Vector3(0f, 1.5f, 0f);
                flashLight.rotation = Quaternion.Euler(0, 0, 90f);

                positionalLight.localPosition = new Vector3(0f, 0.5f, 0f);
                positionalLight.rotation = Quaternion.Euler(0, 0, 90f);
                break;
            case (6):
                oldFlashlightState = 6;
                flashlightState = 7;
                flashLight.localPosition = new Vector3(1.2f, 1.2f, 0f);
                flashLight.rotation = Quaternion.Euler(0, 0, 45f);
                break;
            case (7):
                oldFlashlightState = 7;
                flashlightState = 0;
                flashLight.localPosition = new Vector3(1.5f, 0f, 0f);
                flashLight.rotation = Quaternion.Euler(0, 0, 0f);

                positionalLight.localPosition = new Vector3(0.5f, 0f, 0f);
                positionalLight.rotation = Quaternion.Euler(0, 0, 0);
                break;
            default:
                break;
        }
    }

    private void TurnFlashlightLeft()
    {
        switch (flashlightState)
            {
                case (0):
                    oldFlashlightState = 0;
                    flashlightState = 7;
                    flashLight.localPosition = new Vector3(1.2f, 1.2f, 0f);
                    flashLight.rotation = Quaternion.Euler(0, 0, 45f);
                    break;


                case (1):
                    oldFlashlightState = 2;
                    flashlightState = 0;
                    flashLight.localPosition = new Vector3(1.5f, 0f, 0f);
                    flashLight.rotation = Quaternion.Euler(0, 0, 0f);


                    positionalLight.localPosition = new Vector3(0.5f, 0f, 0f);
                    positionalLight.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case (2):
                    oldFlashlightState = 2;
                    flashlightState = 1;
                    flashLight.localPosition = new Vector3(1.2f, -1.2f, 0f);
                    flashLight.rotation = Quaternion.Euler(0, 0, -45f);
                    break;


                case (3):
                    oldFlashlightState = 3;
                    flashlightState = 2;
                    flashLight.localPosition = new Vector3(0f, -1.5f, 0f);
                    flashLight.rotation = Quaternion.Euler(0, 0, 90f);

                    positionalLight.localPosition = new Vector3(0f, -0.5f, 0f);
                    positionalLight.rotation = Quaternion.Euler(0, 0, 90f);
                    break;

                case (4):
                    oldFlashlightState = 4;
                    flashlightState = 3;
                    flashLight.localPosition = new Vector3(-1.2f, -1.2f, 0f);
                    flashLight.rotation = Quaternion.Euler(0, 0, 45);
                    break;

                case (5):
                    oldFlashlightState = 5;
                    flashlightState = 4;
                    flashLight.localPosition = new Vector3(-1.5f, 0f, 0f);
                    flashLight.rotation = Quaternion.Euler(0, 0, 0);

                    positionalLight.localPosition = new Vector3(-0.5f, 0f, 0f);
                    positionalLight.rotation = Quaternion.Euler(0, 0, 0);
                    break;

                case (6):
                    oldFlashlightState = 6;
                    flashlightState = 5;
                    flashLight.localPosition = new Vector3(-1.2f, 1.2f, 0f);
                    flashLight.rotation = Quaternion.Euler(0, 0, -45f);
                    break;
                case (7):
                    oldFlashlightState = 7;
                    flashlightState = 6;
                    flashLight.localPosition = new Vector3(0f, 1.5f, 0f);
                    flashLight.rotation = Quaternion.Euler(0, 0, 90f);

                    positionalLight.localPosition = new Vector3(0f, 0.5f, 0f);
                    positionalLight.rotation = Quaternion.Euler(0, 0, 90f);
                    break;

                default:
                    break;
            }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);

        bool oneIsMoving = false;
        foreach (EnemyMovement e in Regenerate.instance.agents.GetComponentsInChildren<EnemyMovement>())
        {
            if (e._isMoving)
            {
                oneIsMoving = true;
                break;
            }
        }
        
        // if the player has ended its action, do the enemy action
        if (GetComponent<EnemyMovement>()._isMoving)
        {
            if (Vector3.Distance(transform.position, movePoint.position) <
                GetComponent<EnemyMovement>()._movementThreshold)
            {
                StartCoroutine(_enemyManager.moveAgents());
            }
        }
        
        if (!GetComponent<EnemyMovement>()._isMoving && !oneIsMoving)
        {
            // In case we are training, the player does not accept input
            if (Regenerate.instance._training)
            {
                // Do some random action
                //int action = UnityEngine.Random.Range(0, 9);

                // Do some fensible random action
                
                int[] feasibleActions = Regenerate.instance.getFeasibleActionset(transform.position);
                var actionsIdxList = new ArrayList();
                
                for (int i = 0; i < 9; i++)
                {
                    if (feasibleActions[i] > 0)
                    {
                        actionsIdxList.Add(i);  
                    }
                }
                
                // Add flashlight actions, only for player
                // The flashlight actions are always feasible
                actionsIdxList.Add(9);
                actionsIdxList.Add(10);
                
                int random = UnityEngine.Random.Range(0, actionsIdxList.Count);

                int action = (int) actionsIdxList[random];
                
                MakeAction(action);
                // StartCoroutine(_enemyManager.moveAgents());
            }
            // Otherwise, wait for input
            else
            {
                if (Input.GetKeyDown(KeyCode.P)) // P: full matrix
                {
                    Regenerate.instance.getFullStateMatrix();
                }
                if (Input.GetKeyDown(KeyCode.L)) // L: crop matrix
                {
                    Vector3 tmpVector = new Vector3(2.5f, 2.5f, 0f); // just to test
                    Regenerate.instance.getCropStateMatrix(tmpVector, 3);
                }
                if (Input.GetKeyDown(KeyCode.F)) // F: wait 
                {
                    int action = 8;
                    MakeAction(action);
                    // moveEnemies.Invoke();
                    // StartCoroutine(_enemyManager.moveAgents());
                }
                else if ((Mathf.Abs(Input.GetAxisRaw("Horizontal")) == 1) || (Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1))
                {
                    int action = -1;
                    if (Math.Ceiling(Input.GetAxisRaw("Horizontal")) > 0) 
                    {
                        action = 0;
                    }else if ((Math.Ceiling(Input.GetAxisRaw("Vertical")) < 0) && (Math.Ceiling(Input.GetAxisRaw("Horizontal")) > 0) )
                    {
                        action = 1;
                    }
                    else if ((Math.Ceiling(Input.GetAxisRaw("Vertical")) < 0))
                    {
                        action = 2;
                    }
                    else if ((Math.Ceiling(Input.GetAxisRaw("Vertical")) < 0) && (Math.Ceiling(Input.GetAxisRaw("Horizontal")) < 0))
                    {
                        action = 3;
                    }
                    else if ((Math.Ceiling(Input.GetAxisRaw("Horizontal")) < 0))
                    {
                        action = 4;
                    }
                    else if ((Math.Ceiling(Input.GetAxisRaw("Vertical")) > 0) && (Math.Ceiling(Input.GetAxisRaw("Horizontal")) < 0))
                    {
                        action = 5;
                    }
                    else if ((Math.Ceiling(Input.GetAxisRaw("Vertical")) > 0) )
                    {
                        action = 6;
                    }
                    else if ((Math.Ceiling(Input.GetAxisRaw("Vertical")) > 0) && (Math.Ceiling(Input.GetAxisRaw("Horizontal")) > 0))
                    {
                        action = 7;
                    }
                    if (Regenerate.instance.getFeasibleActionset(transform.position)[action] == 1)
                    {
                        MakeAction(action);
                        // moveEnemies.Invoke();
                        // StartCoroutine(_enemyManager.moveAgents()); 
                    }
                }

                if (Input.GetKeyDown(KeyCode.E))
                {

                    TurnFlashlightRight();
                    // StartCoroutine(_enemyManager.moveAgents());
                }
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    TurnFlashlightLeft();
                    // StartCoroutine(_enemyManager.moveAgents());
                }


                Regenerate.instance.checkLightOnEnemies();
            }
        }      
        
    }

    


}
