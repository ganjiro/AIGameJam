using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Transform movePoint;
    public LayerMask cantMove;
    public GameObject goal;
    public GameObject player;
    public bool _isMoving = false;
    private float[] _allDistances;
    public Animator animator;
    public GameObject happySprite;
    public GameObject darkSprite;
    public bool _hasStarted;

    private Animator anim;
    private bool isDying;

    private VictimAgent _agentComponent;
    public float _movementThreshold = 0.005f;

    // Start is called before the first frame update
    void Start()
    {
        movePoint.parent = null;
        _agentComponent = GetComponent<VictimAgent>();
        _allDistances = new float[Regenerate.instance.width * Regenerate.instance.height*2];
        if (!goal)
        {
            goal = GameObject.Find("Goal");
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        // If the animation is running, do nothing
        if (isDying)
        {
            checkLightOnEnemy();
            return;
        }
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);

        // Give instantaneous reward only if the movement has finished
        if (_hasStarted && (Vector3.Distance(transform.position, movePoint.position) <= _movementThreshold))
        {
            if (_agentComponent != null)
            {
                // Give a negative reward to finish as soon as it can
                _agentComponent.AddReward(-0.025f);
                // Add dense reward
                float dt = Vector3.Distance(transform.position, goal.transform.position);

                if(_agentComponent._stepCount == 0)
                {
                    _agentComponent.AddReward(0f);
                }
                else
                {
                    float minDist = float.PositiveInfinity;
                    for(int i = 0; i < _agentComponent._stepCount; i++)
                    {
                        if(_allDistances[i] < minDist)
                        {
                            minDist = _allDistances[i];
                        }
                    }

                    _agentComponent.AddReward(Mathf.Max(minDist - dt, 0));
                }
                _allDistances[_agentComponent._stepCount] = dt;
                _agentComponent._stepCount ++;
            }
            
            _isMoving = false;
            _hasStarted = false;
        
        }
        
        // Give the reward to the agent
        if (_agentComponent != null && goal.transform.position == transform.position)
        {
            // Destroy(transform.gameObject);
            if (Regenerate.instance._training)
            {
                _agentComponent.AddReward(50f);
                _isMoving = false;
                _hasStarted = false;
                _agentComponent.EndEpisode();
                return;
            }
            else
            {
                Regenerate.instance.RemoveEnemyFromPool(gameObject);
                Regenerate.instance.GameOver();    
                return;
            }
        }
        checkLightOnEnemy();

        if (_hasStarted)
            _isMoving = Vector3.Distance(transform.position, movePoint.position) >= _movementThreshold;
    }

    public void GiveDeadReward()
    {
        if (_agentComponent != null)
        {
            _agentComponent.AddReward(-5f);
            _isMoving = false;
            _hasStarted = false;
            _agentComponent.EndEpisode();
        }
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
        _hasStarted = true;
        switch (action)
        {
            case 0:
                moveE();
                break;
            case 1:
                moveSE();
                break;
            case 2:
                moveS();
                break;
            case 3:
                moveSW();
                break;
            case 4:
                moveW();
                break;
            case 5:
                moveNW();
                break;
            case 6:
                moveN();
                break;
            case 7:
                moveNE();
                break;
            case 8:
                // _hasStarted = false;
                break;
            default:
                break;
        }
    }

    public int VectorToAction(Vector2 endTile)
    {
        Vector3 endTile3 = Regenerate.instance.setAndGetVector(endTile.x, endTile.y);
        Vector3 offset = endTile3 - movePoint.position;

        if(offset.x == 0f && offset.y == 1f)
        {
            return 6;
        }
        if(offset.x == 0f && offset.y == -1f)
        {
            return 2;
        }
        if(offset.x == -1f && offset.y == 0f)
        {
            return 4;
        }
        if(offset.x == 1f && offset.y == 0f)
        {
            return 0;
        }
        if(offset.x == 1f && offset.y == 1f)
        {
            return 7;
        }
        if(offset.x == 1f && offset.y == -1f)
        {
            return 1;
        }
        if(offset.x == -1f && offset.y == -1f)
        {
            return 3;
        }
        if(offset.x == -1f && offset.y == 1f)
        {
            return 5;
        }
        return 99;
    }

    public void moveN()
    {
        if (!Physics2D.OverlapCircle(movePoint.position + Regenerate.instance.setAndGetVector(0f, 1f), .2f, cantMove))
        {
            movePoint.position += Regenerate.instance.setAndGetVector(0f, 1f);
            if (animator)
            {
                animator.SetTrigger("movement");
            }
        }
    }

    public void moveS()
    {
        if (!Physics2D.OverlapCircle(movePoint.position + Regenerate.instance.setAndGetVector(0f, -1f), .2f, cantMove))
        {
            movePoint.position += Regenerate.instance.setAndGetVector(0f, -1f);
            if (animator)
            {
                animator.SetTrigger("movement");
            }
        }
    }

    public void moveW()
    {
        if (!Physics2D.OverlapCircle(movePoint.position + Regenerate.instance.setAndGetVector(-1f, 0f), .2f, cantMove))
        {
            movePoint.position += Regenerate.instance.setAndGetVector(-1f, 0f);
            if (animator)
            {
                animator.SetTrigger("movement");
            }
        }
    }

    public void moveE()
    {
        if (!Physics2D.OverlapCircle(movePoint.position + Regenerate.instance.setAndGetVector(1f, 0f), .2f, cantMove))
        {
            movePoint.position += Regenerate.instance.setAndGetVector(1f, 0f);
            if (animator)
            {
                animator.SetTrigger("movement");
            }
        }
    }
    
    public void moveNE()
    {
        if (!Physics2D.OverlapCircle(movePoint.position + Regenerate.instance.setAndGetVector(1f, 1f), .2f, cantMove))
        {
            movePoint.position += Regenerate.instance.setAndGetVector(1f, 1f);
            if (animator)
            {
                animator.SetTrigger("movement");
            }
        }
    }
    
    public void moveSE()
    {
        if (!Physics2D.OverlapCircle(movePoint.position + Regenerate.instance.setAndGetVector(1f, -1f), .2f, cantMove))
        {
            movePoint.position += Regenerate.instance.setAndGetVector(1f, -1f);
            if (animator)
            {
                animator.SetTrigger("movement");
            }
        }
    }
    
    public void moveSW()
    {
        if (!Physics2D.OverlapCircle(movePoint.position + Regenerate.instance.setAndGetVector(-1f, -1f), .2f, cantMove))
        {
            movePoint.position += Regenerate.instance.setAndGetVector(-1f, -1f);
            if (animator)
            {
                animator.SetTrigger("movement");
            }
        }
    }
    
    public void moveNW()
    {
        if (!Physics2D.OverlapCircle(movePoint.position + Regenerate.instance.setAndGetVector(-1f, 1f), .2f, cantMove))
        {
            movePoint.position += Regenerate.instance.setAndGetVector(-1f, 1f);
            if (animator)
            {
                animator.SetTrigger("movement");
            }
        }
    }

    public void checkLightOnEnemy()
    {
        int[,] state = Regenerate.instance.getCropStateMatrix(transform.position, 1);
        
        // When the animation is concluded, kill it
        if (isDying && animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
        {
            isDying = false;
            Regenerate.instance.RemoveEnemyFromPool(transform.gameObject);
            // happySprite.transform.parent = null;
            // darkSprite.transform.parent = null;
            // animator.SetTrigger("die");
            transform.gameObject.SetActive(false);
            GlobalBlackboard.instance.IncreaseMadnessValue();
            // Increase madness value and enemy killed in this level
            Regenerate.instance.player.GetComponent<PlayerController>().IncreaseEnemyKilled();
            // Regenerate.instance.player.gameObject.GetComponent<Animator>().SetTrigger("enemyKilled");
            return;
        }

        if ((state[1,1] == 5 || state[1,1] == 4) && !gameObject.CompareTag("Player"))
        {
            if (Regenerate.instance._training)
            {
                
                GiveDeadReward();
            }
            else
            {
                if (GlobalBlackboard.instance.madnessValue == GlobalBlackboard.instance.maxMadnessValue)
                {
                    player.GetComponent<PlayerController>().animator.SetTrigger("EnemyKilled");
                }
                // Start the animation but do not kill it
                anim = GetComponentInChildren<Animator>();
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("death"))
                {
                    animator.Play("death");
                    
                    isDying = true;
                    return;
                }
            }
        }        
    }
}
