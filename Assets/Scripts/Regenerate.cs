using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Linq;
using Unity.MLAgents;
using System;

public class Regenerate : MonoBehaviour
{
    public GameObject obstacles;
    public GameObject player;
    public GameObject agents;
    public GameObject goal;
    public int nObstacles;
    public GameObject obstaclesPrefab;
    public GameObject enemyPrefab;
    public GameObject playerPrefab;
    public LayerMask cantMove;
    public bool _training;

    public List<GameObject> _enemyPool;
    
    public static Regenerate instance;

    void Awake()
    {
        //Check if instance already exists
        if (instance == null)

            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
        
        // Populate enemy pool
        for (int i = 0; i < 10; i++)
        {
            GameObject instantiatedObject = Instantiate(enemyPrefab, new Vector3(-99f, -99f, 0f), Quaternion.identity);
            instantiatedObject.SetActive(false);
            instantiatedObject.transform.SetParent(transform);
            _enemyPool.Add(instantiatedObject);            
        }

        
        // If we are training, first of all create the map
        if (_training)
        {
            CreateMap();
            Academy.Instance.OnEnvironmentReset += CreateMap;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.K))
        {
            CreateMap();
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            getFullStateMatrix();
        }

     }

    public int[] getFeasibleActionset(Vector3 objPosition)
    {
        int[] feasible = new int[9];
        
        feasible[8] = 1; // wait always feasible

        if (!Physics2D.OverlapCircle(objPosition + new Vector3(1f, 0f, 0f), .2f, cantMove)) // E
        {
            feasible[0] = 1; 
        }
        if (!Physics2D.OverlapCircle(objPosition + new Vector3(1f, -1f, 0f), .2f, cantMove)) // SE
        {
            feasible[1] = 1;
        }
        if (!Physics2D.OverlapCircle(objPosition + new Vector3(0f, -1f, 0f), .2f, cantMove)) // S
        {
            feasible[2] = 1;
        }
        if (!Physics2D.OverlapCircle(objPosition + new Vector3(-1f, -1f, 0f), .2f, cantMove)) // SO
        {
            feasible[3] = 1;
        }
        if (!Physics2D.OverlapCircle(objPosition + new Vector3(-1f, 0f, 0f), .2f, cantMove)) // O
        {
            feasible[4] = 1;
        }
        if (!Physics2D.OverlapCircle(objPosition + new Vector3(-1f, 1f, 0f), .2f, cantMove)) // NO
        {
            feasible[5] = 1;
        }
        if (!Physics2D.OverlapCircle(objPosition + new Vector3(0f, 1f, 0f), .2f, cantMove)) // N
        {
            feasible[6] = 1;
        }
        if (!Physics2D.OverlapCircle(objPosition + new Vector3(1f, 1f, 0f), .2f, cantMove)) // NE
        {
            feasible[7] = 1;
        }

        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            sum += feasible[i]; // 0: not feasible
        }
        if(sum == 1)
        {
            Debug.Log("Devo per forza stare fermo");
        }

        return feasible;

    }

    public void CreateMap()
    {
        foreach (Transform child in obstacles.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        
        // Reset enemy pool
        foreach (GameObject e in _enemyPool)
        {
            RemoveEnemyFromPool(e);
        }

        // foreach (Transform child in agents.transform)
        // {
        //     Destroy(child.GetComponent<EnemyMovement>().movePoint.gameObject);
        //     GameObject.Destroy(child.gameObject);
        // }

        for (int i=0; i < nObstacles; i++) {

            float x = UnityEngine.Random.Range(-5, 4) + 0.5f;
            float y = UnityEngine.Random.Range(-5, 4) + 0.5f;

            while (Physics2D.OverlapCircle(new Vector3(x, y, 0f), 1.2f, cantMove)) 
            {
                x = UnityEngine.Random.Range(-5, 4) + 0.5f;
                y = UnityEngine.Random.Range(-5, 4) + 0.5f;
            }

            GameObject instantiatedObject = Instantiate(obstaclesPrefab, new Vector3(x, y, 0f), Quaternion.identity);
            instantiatedObject.transform.SetParent(obstacles.transform);            

        }
        
        spawnEnemy();
        // spawnEnemy();

        float xP = UnityEngine.Random.Range(-5, 4) + 0.5f;
        float yP = UnityEngine.Random.Range(-5, 4) + 0.5f;

        while (Physics2D.OverlapCircle(new Vector3(xP, yP, 0f), .2f, cantMove))
        {
            xP = UnityEngine.Random.Range(-5, 4) + 0.5f;
            yP = UnityEngine.Random.Range(-5, 4) + 0.5f;
        }

        player.transform.position = new Vector3(xP, yP, 0f);
        player.GetComponent<PlayerController>().movePoint.position = new Vector3(xP, yP, 0f);

        xP = UnityEngine.Random.Range(-5, 4) + 0.5f;
        yP = UnityEngine.Random.Range(-5, 4) + 0.5f;

        while (Physics2D.OverlapCircle(new Vector3(xP, yP, 0f), .2f, cantMove))
        {
            xP = UnityEngine.Random.Range(-5, 4) + 0.5f;
            yP = UnityEngine.Random.Range(-5, 4) + 0.5f;
        }

        goal.transform.position = new Vector3(xP, yP, 0f);
    }

    private void spawnEnemy()
    {
        // Get the first non active enemy from the pool
        
        float x = UnityEngine.Random.Range(-5, 4) + 0.5f;
        float y = UnityEngine.Random.Range(-5, 4) + 0.5f;

        while (Physics2D.OverlapCircle(new Vector3(x, y, 0f), .2f, cantMove))
        {
            x = UnityEngine.Random.Range(-5, 4) + 0.5f;
            y = UnityEngine.Random.Range(-5, 4) + 0.5f;
        }

        GameObject enemy = null;
        foreach (GameObject e in _enemyPool)
        {
            if (!e.activeSelf)
            {
                e.SetActive(true);
                enemy = e;
                break;
            }
        }
        // GameObject instantiatedObject = Instantiate(enemyPrefab, new Vector3(x, y, 0f), Quaternion.identity);
        enemy.transform.position = new Vector3(x, y, 0f);
        Debug.Log("DA RIMUOVERREEEEEE");
        Debug.Log(enemy.transform.position.x);
        Debug.Log(enemy.transform.position.y);
        enemy.transform.rotation = Quaternion.identity;
        Debug.Log("ANCHE QUESTO");
        Debug.Log(enemy.transform.position.x);
        Debug.Log(enemy.transform.position.y);
        enemy.GetComponent<EnemyMovement>().movePoint.transform.position = enemy.transform.position;
        enemy.GetComponent<VictimAgent>()._inference = !_training;
        enemy.GetComponent<EnemyMovement>().goal = goal;
        enemy.GetComponent<EnemyMovement>().player = player;
        enemy.transform.SetParent(agents.transform);
    }

    
    
    public void RemoveEnemyFromPool(GameObject enemy)
    {
        enemy.transform.SetParent(transform);
        enemy.transform.position = new Vector3(-99f, -99f, 0f);
        enemy.GetComponent<EnemyMovement>().movePoint.transform.position = enemy.transform.position;
        enemy.SetActive(false);
    }

    public int[,] getCropStateMatrix(Vector3 agentPosition, int radius)
    {
        int xRow = (int) (agentPosition.x + 4.5);
        int yRow = (int)(Mathf.Abs((int)(agentPosition.y - 4.5)));


        int diameter = (int)(radius) * 2 + 1;
        int[,] cropStateMatrix = new int[diameter, diameter];
        int[,] fullStateMatrix = getFullStateMatrix();
    
        for (int i = 0; i < diameter; i++)
        {
            for (int j = 0; j < diameter; j++)
            {
                cropStateMatrix[i, j] = 1;
            }
        }
        // TODO FIX
        for (int i = xRow-(diameter/2); i < xRow + (diameter / 2); i++)
        {
            for (int j = yRow - (diameter / 2); i < yRow + (diameter / 2); i++)
            {
                if (i >= 0 && i <= 9 && j >= 0 && j <= 9)
                {
                    cropStateMatrix[i, j] = fullStateMatrix[i, j];
                }
            }   
        }

        return null;

    }
    public int[,] getFullStateMatrix()
    {
        int[,] stateMatrix = new int[10, 10]; // 0: blank

        foreach (Transform child in obstacles.transform)
        {
            stateMatrix[(int)(child.position.x + 4.5), Mathf.Abs((int)(child.position.y - 4.5))] = 1; // 1: obstacles
        }

        foreach (Transform child in agents.transform)
        {
            stateMatrix[(int)(child.position.x + 4.5), Mathf.Abs((int)(child.position.y - 4.5))] = 2; // 2: agents
        }


        stateMatrix[(int)(goal.transform.position.x + 4.5), Mathf.Abs((int)(goal.transform.position.y - 4.5))] = 3; // 3: goal
        stateMatrix[(int)(player.transform.position.x + 4.5), Mathf.Abs((int)(player.transform.position.y - 4.5))] = 4; // 4: player

        int flashState = player.GetComponent<PlayerController>().flashlightState;

        switch (flashState)
        {
            case (0):
                try
                {
                    if (stateMatrix[(int)(player.transform.position.x + 4.5) + 1, Mathf.Abs((int)(player.transform.position.y - 4.5))] == 1)
                    {
                        break; 
                    }
                    else
                    {
                        stateMatrix[(int)(player.transform.position.x + 4.5) + 1, Mathf.Abs((int)(player.transform.position.y - 4.5))] = 5; // luce
                        stateMatrix[(int)(player.transform.position.x + 4.5) + 2, Mathf.Abs((int)(player.transform.position.y - 4.5))] = 5;
                    }
                }
                catch
                { }

                break;

            case (1):
                try
                {
                    if (stateMatrix[(int)(player.transform.position.x + 4.5) + 1, Mathf.Abs((int)(player.transform.position.y - 4.5)) - 1] == 1)
                    {
                        break;
                    }
                    else
                    {
                        stateMatrix[(int)(player.transform.position.x + 4.5) + 1, Mathf.Abs((int)(player.transform.position.y - 4.5)) - 1] = 5;
                        stateMatrix[(int)(player.transform.position.x + 4.5) + 2, Mathf.Abs((int)(player.transform.position.y - 4.5)) - 2] = 5;
                    }
                    
                }
                catch 
                { }
                break;
            case (2):
                try
                {
                    if (stateMatrix[(int)(player.transform.position.x + 4.5), Mathf.Abs((int)(player.transform.position.y - 4.5)) - 1] == 1)
                    {
                        break;
                    }
                    else
                    {
                        stateMatrix[(int)(player.transform.position.x + 4.5), Mathf.Abs((int)(player.transform.position.y - 4.5)) - 1] = 5;
                        stateMatrix[(int)(player.transform.position.x + 4.5), Mathf.Abs((int)(player.transform.position.y - 4.5)) - 2] = 5;
                    }
                }
                catch 
                { }
                break;
            case (3):
                try
                {
                    if (stateMatrix[(int)(player.transform.position.x + 4.5) - 1, Mathf.Abs((int)(player.transform.position.y - 4.5)) - 1] == 1)
                    {
                        break;
                    }
                    else
                    {
                        stateMatrix[(int)(player.transform.position.x + 4.5) - 1, Mathf.Abs((int)(player.transform.position.y - 4.5)) - 1] = 5;
                        stateMatrix[(int)(player.transform.position.x + 4.5) - 2, Mathf.Abs((int)(player.transform.position.y - 4.5)) - 2] = 5;
                    }
                }
                catch
                { }
                break;
            case (4):
                try
                {
                    if (stateMatrix[(int)(player.transform.position.x + 4.5) - 1, Mathf.Abs((int)(player.transform.position.y - 4.5))] == 1)
                    {
                        break;
                    }
                    else
                    {
                        stateMatrix[(int)(player.transform.position.x + 4.5) - 1, Mathf.Abs((int)(player.transform.position.y - 4.5))] = 5;
                        stateMatrix[(int)(player.transform.position.x + 4.5) - 2, Mathf.Abs((int)(player.transform.position.y - 4.5))] = 5;
                    }
                }
                catch 
                { }
                break;
            case (5):
                try
                {
                    if (stateMatrix[(int)(player.transform.position.x + 4.5) - 1, Mathf.Abs((int)(player.transform.position.y - 4.5)) + 1] == 1)
                    {
                        break;
                    }
                    else
                    {
                        stateMatrix[(int)(player.transform.position.x + 4.5) - 1, Mathf.Abs((int)(player.transform.position.y - 4.5)) + 1] = 5;
                        stateMatrix[(int)(player.transform.position.x + 4.5) - 2, Mathf.Abs((int)(player.transform.position.y - 4.5)) + 2] = 5;
                    }
                }
                catch 
                { }
                break;
            case (6):
                try
                {
                    if (stateMatrix[(int)(player.transform.position.x + 4.5), Mathf.Abs((int)(player.transform.position.y - 4.5)) + 1] == 1)
                    {
                        break;
                    }
                    else
                    {
                        stateMatrix[(int)(player.transform.position.x + 4.5), Mathf.Abs((int)(player.transform.position.y - 4.5)) + 1] = 5;
                        stateMatrix[(int)(player.transform.position.x + 4.5), Mathf.Abs((int)(player.transform.position.y - 4.5)) + 2] = 5;
                    }
                }
                catch 
                { }
                break;
            case (7):
                try
                {
                    if (stateMatrix[(int)(player.transform.position.x + 4.5) + 1, Mathf.Abs((int)(player.transform.position.y - 4.5)) + 1] == 1)
                    {
                        break;
                    }
                    else
                    {
                        stateMatrix[(int)(player.transform.position.x + 4.5) + 1, Mathf.Abs((int)(player.transform.position.y - 4.5)) + 1] = 5;
                        stateMatrix[(int)(player.transform.position.x + 4.5) + 2, Mathf.Abs((int)(player.transform.position.y - 4.5)) + 2] = 5;
                    }
                }
                catch 
                { }
                break;
            default:
                break;
        }

        return stateMatrix;
    }

    public void checkLightOnEnemy(int flashlightState)
    {   
        float Threshold = 0.05f;
     
        int pX = (int) (player.transform.position.x + 0.5);
        int pY = (int) (player.transform.position.y + 0.5);

        int tlx = (int) (player.transform.Find("FlashLight").transform.position.x + 0.5);
        int tly = (int) (player.transform.Find("FlashLight").transform.position.y + 0.5);

        int plx = (int)(player.transform.Find("PointLight").transform.position.x + 0.5);
        int ply = (int)(player.transform.Find("PointLight").transform.position.y + 0.5);

        foreach (Transform agent in agents.transform)
        {
            int eX = (int)(agent.transform.position.x + 0.5);
            int eY = (int)(agent.transform.position.y + 0.5);
            /*
            switch (flashlightState)
            {
                case (0):
                    if (pX+tl)
                    {
                        Regenerate.instance.RemoveEnemyFromPool(agent.gameObject);
                        agent.gameObject.SetActive(false);
                    }
                    break;
                case (1):
                   
                    break;
                case (2):
                   
                    break;
                case (3):
                    
                    break;
                case (4):
                    
                    break;
                case (5):
                    
                    break;
                case (6):
                    
                    break;
                case (7):
                    
                    break;
            }
            */
        }
        /*
                ((pX + player.transform.Find("PintLight").transform.position.x - 0.5) == agent.transform.position.x) ||
                ((pX + player.transform.Find("PintLight").transform.position.x - 0.5) == agent.transform.position.x) ||
                ((pX + player.transform.Find("PintLight").transform.position.x - 0.5) == agent.transform.position.x) ||
                ((pX + player.transform.Find("PintLight").transform.position.x - 0.5) == agent.transform.position.x))
        */

    }
}
