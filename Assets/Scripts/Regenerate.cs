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
        if (Input.GetKey(KeyCode.K))
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
            // Debug.Log("Devo per forza stare fermo");
        }

        return feasible;

    }

    private void setFull(float x, float y, int[,] spawnStateMatrix)
    {
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                try
                {
                    spawnStateMatrix[(int)(x + 4.5) + i, Mathf.Abs((int)(y - 4.5)) + j] = 1;
                }
                catch {}
            }
        }
    }

    private void setTorch(float x, float y, int[,] spawnStateMatrix, int state)
    {
        try { 
            switch (state)
            {
                case (0):
                    spawnStateMatrix[(int)(x + 4.5) + 2, Mathf.Abs((int)(y - 4.5))] = 1;
                    break;
                case (1):
                    spawnStateMatrix[(int)(x + 4.5) + 2, Mathf.Abs((int)(y - 4.5)) - 2] = 1;
                    break;
                case (2):
                    spawnStateMatrix[(int)(x + 4.5), Mathf.Abs((int)(y - 4.5)) - 2] = 1;
                    break;
                case (3):
                    spawnStateMatrix[(int)(x + 4.5) - 2, Mathf.Abs((int)(y - 4.5)) - 2] = 1;
                    break;
                case (4):
                    spawnStateMatrix[(int)(x + 4.5) - 2, Mathf.Abs((int)(y - 4.5))] = 1;
                    break;
                case (5):
                    spawnStateMatrix[(int)(x + 4.5) - 2, Mathf.Abs((int)(y - 4.5)) + 2] = 1;
                    break;
                case (6):
                    spawnStateMatrix[(int)(x + 4.5), Mathf.Abs((int)(y - 4.5)) + 2] = 1;
                    break;
                case (7):
                    spawnStateMatrix[(int)(x + 4.5) + 2, Mathf.Abs((int)(y - 4.5)) + 2] = 1;
                    break;
                default:
                    break;
            }
        }
        catch { }
    }



    private float[] getFensibleIndexs(int[,] spawnStateMatrix)
    {
        float xP = UnityEngine.Random.Range(-5, 4) + 0.5f;  
        float yP = UnityEngine.Random.Range(-5, 4) + 0.5f;
        int itr = 0;                                      

        while (spawnStateMatrix[(int)(xP + 4.5), Mathf.Abs((int)(yP - 4.5))] == 1 && itr < 80)
        {
            xP = UnityEngine.Random.Range(-5, 4) + 0.5f;
            yP = UnityEngine.Random.Range(-5, 4) + 0.5f;
            itr++;
        }

        if (itr == 80)
        {
            xP = UnityEngine.Random.Range(50, 60) + 0.5f;
            yP = UnityEngine.Random.Range(50, 60) + 0.5f;
        } 

        return new float[] {xP, yP};
    }

    public void CreateMap()
    {

        int itr = 0;

        foreach (Transform child in obstacles.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        // Reset enemy pool
        foreach (GameObject e in _enemyPool)
        {
            RemoveEnemyFromPool(e);
        }

        goal.transform.position = new Vector3(89f, 89f, 0f);
        player.transform.position = new Vector3(79f, 79f, 0f);

        int[,] spawnStateMatrix = new int[10, 10];

        float[] pos = getFensibleIndexs(spawnStateMatrix);
        setFull(pos[0], pos[1], spawnStateMatrix);
        player.transform.position = new Vector3(pos[0], pos[1], 0f);        
        player.GetComponent<PlayerController>().movePoint.position = new Vector3(pos[0], pos[1], 0f);
        player.GetComponent<EnemyMovement>()._isMoving = false;
        player.GetComponent<PlayerController>().randomFlashLightOrientation();
        setTorch(pos[0], pos[1], spawnStateMatrix, player.GetComponent<PlayerController>().flashlightState);

<<<<<<< HEAD
        pos = getFensibleIndexs(spawnStateMatrix);
        setFull(pos[0], pos[1], spawnStateMatrix);
        spawnEnemy(pos[0], pos[1]);
=======
        float goalRadius = Academy.Instance.EnvironmentParameters.GetWithDefault("goalRadius", 10);
        spawnEnemy();
        spawnGoal(goalRadius);
    }

    private void spawnGoal(float goalRadius)
    {
        int maxX = (int)Math.Min(agents.transform.GetChild(0).transform.position.x - 0.5f + goalRadius, 4);
        int minX = (int)Math.Max(agents.transform.GetChild(0).transform.position.x - 0.5f - goalRadius, -5);
        
        int maxY = (int)Math.Min(agents.transform.GetChild(0).transform.position.y - 0.5f + goalRadius, 4);
        int minY = (int)Math.Max(agents.transform.GetChild(0).transform.position.y - 0.5f - goalRadius, -5);
        
        float xP = UnityEngine.Random.Range(minX, maxX) + 0.5f;
        float yP = UnityEngine.Random.Range(minY, maxY) + 0.5f;
>>>>>>> 21204dd032649d2d78ed0323e66d993861c62654

        pos = getFensibleIndexs(spawnStateMatrix);
        setFull(pos[0], pos[1], spawnStateMatrix);
        goal.transform.position = new Vector3(pos[0], pos[1], 0f);

        for (int i = 0; i < nObstacles; i++)
        {
<<<<<<< HEAD
            float[] pos_obs = getFensibleIndexs(spawnStateMatrix);
            setFull(pos_obs[0], pos_obs[1], spawnStateMatrix);

            GameObject instantiatedObject = Instantiate(obstaclesPrefab, new Vector3(pos_obs[0], pos_obs[1], 0f), Quaternion.identity);
            instantiatedObject.transform.SetParent(obstacles.transform);
        }
    }

    private void spawnEnemy(float x, float y)
=======
            xP = UnityEngine.Random.Range(minX, maxX) + 0.5f;
            yP = UnityEngine.Random.Range(minY, maxY) + 0.5f;
        }

        goal.transform.position = new Vector3(xP, yP, 0f);
    }
    
    private void spawnEnemy()
>>>>>>> 21204dd032649d2d78ed0323e66d993861c62654
    {
        // Get the first non active enemy from the pool       
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
        enemy.transform.rotation = Quaternion.identity;
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
                cropStateMatrix[i, j] = 1; // default: obstacles
            }
        }

        int localX = 0;
        int localY = 0;

        for (int i = xRow-(diameter-1)/2; i < 1 + xRow + (diameter-1)/2; i++)
        {
            for (int j = yRow - (diameter - 1) / 2; j < 1 + yRow + (diameter - 1) / 2; j++)
            {
                if (i >= 0 && i <= 9 && j >= 0 && j <= 9)
                {                    
                    cropStateMatrix[localX, localY] = fullStateMatrix[i, j];                                        
                }
                localY++;
            }
            localX++;
            localY = 0;
        }
        
        return cropStateMatrix;
    }

    public int[,] getFullStateMatrix()
    {
        int[,] stateMatrix = new int[10, 10]; // 0: blank

        foreach (Transform child in obstacles.transform)
        {
            try
            {
                stateMatrix[(int)(child.position.x + 4.5), Mathf.Abs((int)(child.position.y - 4.5))] = 1; // 1: obstacles
            }
            catch
            {              
            }

        }

        foreach (Transform child in agents.transform)
        {
            stateMatrix[(int)(child.position.x + 4.5), Mathf.Abs((int)(child.position.y - 4.5))] = 2; // 2: agents
        }


        stateMatrix[(int)(goal.transform.position.x + 4.5), Mathf.Abs((int)(goal.transform.position.y - 4.5))] = 3; // 3: goal

        int posX = (int)(player.transform.position.x + 4.5);
        int posY = Mathf.Abs((int)(player.transform.position.y - 4.5));

        stateMatrix[posX, posY] = 4; // 4: player
               
        switch (player.GetComponent<PlayerController>().flashlightState)
        {
            case (0):
                try
                {
                    if (stateMatrix[posX + 1, posY] == 1)
                    {
                        
                    }
                    else if (stateMatrix[posX + 2, posY] == 1)
                    {
                        stateMatrix[posX + 1, posY] = 5; 

                    }
                    else
                    {
                        stateMatrix[posX + 1, posY] = 5;
                        stateMatrix[posX + 2, posY] = 5;
                    }


                    //inzio FOV
                    //stato 0
                    try
                    {
                        if (stateMatrix[posX, posY - 1] != 1)
                        {
                            stateMatrix[posX, posY - 1] = 5;
                        }
                    }
                    catch { }

                    try
                    {
                        if (stateMatrix[posX + 1, posY - 1] != 1)
                        {
                            stateMatrix[posX + 1, posY - 1] = 5;
                        }
                    }
                    catch { }

                    try
                    {
                        if (stateMatrix[posX + 1, posY ] != 1)
                        {
                            stateMatrix[posX + 1, posY ] = 5;
                        }
                    }
                    catch { }

                    try
                    {
                        if (stateMatrix[posX +1, posY -1] != 1)
                        {
                            stateMatrix[posX + 1, posY + 1] = 5;
                        }
                    }
                    catch { }

                    try
                    {
                        if (stateMatrix[posX , posY + 1] != 1)
                        {
                            stateMatrix[posX, posY + 1] = 5;
                        }
                    }
                    catch { }
                    //fine FOV
                }
                catch
                { }

                break;

            case (1):
                try
                {
                    if (stateMatrix[posX + 1, posY + 1] == 1)
                    {
                        
                    }                    
                    else if (stateMatrix[posX + 2, posY + 2] == 1)
                    {
                        stateMatrix[posX + 1, posY + 1] = 5;

                    }
                    else
                    {
                        stateMatrix[posX + 1, posY + 1] = 5;
                        stateMatrix[posX + 2, posY + 2] = 5;
                    }


                    //inzio FOV
                    if (player.GetComponent<PlayerController>().oldFlashlightState == 0)
                    {
                        //stato 0
                        try
                        {
                            if (stateMatrix[posX, posY - 1] != 1)
                            {
                                stateMatrix[posX, posY - 1] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX + 1, posY - 1] != 1)
                            {
                                stateMatrix[posX + 1, posY - 1] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX + 1, posY] != 1)
                            {
                                stateMatrix[posX + 1, posY] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX + 1, posY - 1] != 1)
                            {
                                stateMatrix[posX + 1, posY + 1] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX, posY + 1] != 1)
                            {
                                stateMatrix[posX, posY + 1] = 5;
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        //stato 2
                        try
                        {
                            if (stateMatrix[posX + 1, posY] != 1)
                            {
                                stateMatrix[posX + 1, posY] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX + 1, posY + 1] != 1)
                            {
                                stateMatrix[posX + 1, posY + 1] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX, posY + 1] != 1)
                            {
                                stateMatrix[posX, posY + 1] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX - 1, posY + 1] != 1)
                            {
                                stateMatrix[posX - 1, posY + 1] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX - 1, posY] != 1)
                            {
                                stateMatrix[posX - 1, posY] = 5;
                            }
                        }
                        catch { }
                    }
                    //fine FOV

                }
                catch 
                { }
                break;
            case (2):
                try
                {
                    if (stateMatrix[posX, posY + 1] == 1)
                    {
                        
                    }
                    else if (stateMatrix[posX, posY + 2] == 1)
                    {
                        stateMatrix[posX, posY + 1] = 5;
                        
                    }else
                    {
                        stateMatrix[posX, posY + 1] = 5;
                        stateMatrix[posX, posY + 2] = 5;
                    }


                    //inizio fov
                    //stato 2
                    try
                    {
                        if (stateMatrix[posX + 1, posY] != 1)
                        {
                            stateMatrix[posX + 1, posY] = 5;
                        }
                    }
                    catch { }

                    try
                    {
                        if (stateMatrix[posX + 1, posY + 1] != 1)
                        {
                            stateMatrix[posX + 1, posY + 1] = 5;
                        }
                    }
                    catch { }

                    try
                    {
                        if (stateMatrix[posX, posY + 1] != 1)
                        {
                            stateMatrix[posX, posY + 1] = 5;
                        }
                    }
                    catch { }

                    try
                    {
                        if (stateMatrix[posX - 1, posY + 1] != 1)
                        {
                            stateMatrix[posX - 1, posY + 1] = 5;
                        }
                    }
                    catch { }

                    try
                    {
                        if (stateMatrix[posX - 1, posY] != 1)
                        {
                            stateMatrix[posX - 1, posY] = 5;
                        }
                    }
                    catch { }
                    //fine fov
                }
                catch 
                { }
                break;
            case (3):
                try
                {
                    if (stateMatrix[posX - 1, posY + 1] == 1)
                    {
                        
                    }
                    else if (stateMatrix[posX - 2, posY + 2] == 1)
                    {
                        stateMatrix[posX - 1, posY + 1] = 5;                       
                    }
                    else
                    {
                        stateMatrix[posX - 1, posY + 1] = 5;
                        stateMatrix[posX - 2, posY + 2] = 5;
                    }

                    //inzio FOV
                    if (player.GetComponent<PlayerController>().oldFlashlightState == 2)
                    {
                        //stato 2
                        try
                        {
                            if (stateMatrix[posX + 1, posY] != 1)
                            {
                                stateMatrix[posX + 1, posY] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX + 1, posY + 1] != 1)
                            {
                                stateMatrix[posX + 1, posY + 1] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX, posY + 1] != 1)
                            {
                                stateMatrix[posX, posY + 1] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX - 1, posY + 1] != 1)
                            {
                                stateMatrix[posX - 1, posY + 1] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX - 1, posY] != 1)
                            {
                                stateMatrix[posX - 1, posY] = 5;
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        //stato 4
                        try
                        {
                            if (stateMatrix[posX, posY + 1] != 1)
                            {
                                stateMatrix[posX, posY + 1] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX - 1, posY + 1] != 1)
                            {
                                stateMatrix[posX - 1, posY + 1] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX - 1, posY] != 1)
                            {
                                stateMatrix[posX - 1, posY] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX - 1, posY - 1] != 1)
                            {
                                stateMatrix[posX - 1, posY - 1] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX, posY - 1] != 1)
                            {
                                stateMatrix[posX, posY - 1] = 5;
                            }
                        }
                        catch { }
                    }
                    //fine FOV

                }
                catch
                { }
                break;
            case (4):
                try
                {
                    if (stateMatrix[posX - 1, posY] == 1)
                    {
                        
                    }
                    else if (stateMatrix[posX - 2, posY] == 1)
                    {
                        stateMatrix[posX - 1, posY] = 5;
                        
                    }
                    else
                    {
                        stateMatrix[posX - 1, posY] = 5;
                        stateMatrix[posX - 2, posY] = 5;
                    }

                    //inzio FOV
                    //stato 4
                    try
                    {
                        if (stateMatrix[posX, posY + 1] != 1)
                        {
                            stateMatrix[posX, posY + 1] = 5;
                        }
                    } catch { }

                    try
                    {
                        if (stateMatrix[posX - 1, posY + 1] != 1)
                        {
                            stateMatrix[posX - 1, posY + 1] = 5;
                        }
                    }
                    catch { }

                    try
                    {
                        if (stateMatrix[posX - 1, posY] != 1)
                        {
                            stateMatrix[posX - 1, posY] = 5;
                        }
                    }
                    catch { }

                    try
                    {
                        if (stateMatrix[posX - 1, posY - 1] != 1)
                        {
                            stateMatrix[posX - 1, posY - 1] = 5;
                        }
                    }
                    catch { }

                    try
                    {
                        if (stateMatrix[posX, posY - 1] != 1)
                        {
                            stateMatrix[posX, posY - 1] = 5;
                        }
                    }
                    catch { }
                    //fine FOV
                }
                catch 
                { }
                break;
            case (5):
                try
                {
                    if (stateMatrix[posX - 1, posY - 1] == 1)
                    {
                       
                    }
                    else if(stateMatrix[posX - 2, posY - 2] == 1)
                    {
                        stateMatrix[posX - 1, posY - 1] = 5;
                        
                    }
                    else
                    {
                        stateMatrix[posX - 1, posY - 1] = 5;
                        stateMatrix[posX - 2, posY - 2] = 5;
                    }

                    //inzio FOV
                    if (player.GetComponent<PlayerController>().oldFlashlightState == 4)
                    {
                        //stato 4
                        try
                        {
                            if (stateMatrix[posX, posY + 1] != 1)
                            {
                                stateMatrix[posX, posY + 1] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX - 1, posY + 1] != 1)
                            {
                                stateMatrix[posX - 1, posY + 1] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX - 1, posY] != 1)
                            {
                                stateMatrix[posX - 1, posY] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX - 1, posY - 1] != 1)
                            {
                                stateMatrix[posX - 1, posY - 1] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX, posY - 1] != 1)
                            {
                                stateMatrix[posX, posY - 1] = 5;
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        //stato 6                 
                        try
                        {
                            if (stateMatrix[posX - 1, posY] != 1)
                            {
                                stateMatrix[posX - 1, posY] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX - 1, posY - 1] != 1)
                            {
                                stateMatrix[posX - 1, posY - 1] = 5;
                            }
                        }
                        catch { }

                        if (stateMatrix[posX, posY - 1] != 1)
                        {
                            stateMatrix[posX, posY - 1] = 5;
                        }

                        try
                        {
                            if (stateMatrix[posX + 1, posY - 1] != 1)
                            {
                                stateMatrix[posX + 1, posY - 1] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX + 1, posY] != 1)
                            {
                                stateMatrix[posX + 1, posY] = 5;
                            }
                        }
                        catch { }
                    }
                    //fine FOV
                }
                catch 
                { }
                break;
            case (6):
                try
                {
                    if (stateMatrix[posX, posY - 1] == 1)
                    {
                       
                    }
                    else if (stateMatrix[posX, posY - 2] == 1)
                    {
                        stateMatrix[posX, posY - 1] = 5;

                    }                   
                    else
                    {
                        stateMatrix[posX, posY - 1] = 5;
                        stateMatrix[posX, posY - 2] = 5;
                    }

                    //inizio FOV
                    //stato 6                 
                    try
                    {
                        if (stateMatrix[posX - 1, posY] != 1)
                        {
                            stateMatrix[posX - 1, posY] = 5;
                        }
                    }
                    catch { }

                    try
                    {
                        if (stateMatrix[posX - 1, posY - 1] != 1)
                        {
                            stateMatrix[posX - 1, posY - 1] = 5;
                        }
                    }
                    catch { }

                    if (stateMatrix[posX, posY - 1] != 1)
                    {
                        stateMatrix[posX, posY - 1] = 5;
                    }

                    try
                    {
                        if (stateMatrix[posX + 1, posY - 1] != 1)
                        {
                            stateMatrix[posX + 1, posY - 1] = 5;
                        }
                    }
                    catch { }

                    try
                    {
                        if (stateMatrix[posX + 1, posY] != 1)
                        {
                            stateMatrix[posX + 1, posY] = 5;
                        }
                    }
                    catch { }
                    //fine FOV
                }
                catch 
                { }
                break;
            case (7):
                try
                {
                    if (stateMatrix[posX + 1, posY - 1] == 1)
                    {
                        
                    }
                    else if (stateMatrix[posX + 2, posY + 2] == 1)
                    {
                        stateMatrix[posX + 1, posY - 1] = 5;
                    }
                    else
                    {
                        stateMatrix[posX + 1, posY - 1] = 5;
                        stateMatrix[posX + 2, posY + 2] = 5;
                    }

                    //inzio FOV
                    if (player.GetComponent<PlayerController>().oldFlashlightState == 6)
                    {
                        //stato 6                 
                        try
                        {
                            if (stateMatrix[posX - 1, posY] != 1)
                            {
                                stateMatrix[posX - 1, posY] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX - 1, posY - 1] != 1)
                            {
                                stateMatrix[posX - 1, posY - 1] = 5;
                            }
                        }
                        catch { }

                        if (stateMatrix[posX, posY - 1] != 1)
                        {
                            stateMatrix[posX, posY - 1] = 5;
                        }

                        try
                        {
                            if (stateMatrix[posX + 1, posY - 1] != 1)
                            {
                                stateMatrix[posX + 1, posY - 1] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX + 1, posY] != 1)
                            {
                                stateMatrix[posX + 1, posY] = 5;
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        //stato 0
                        try
                        {
                            if (stateMatrix[posX, posY - 1] != 1)
                            {
                                stateMatrix[posX, posY - 1] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX + 1, posY - 1] != 1)
                            {
                                stateMatrix[posX + 1, posY - 1] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX + 1, posY] != 1)
                            {
                                stateMatrix[posX + 1, posY] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX + 1, posY - 1] != 1)
                            {
                                stateMatrix[posX + 1, posY + 1] = 5;
                            }
                        }
                        catch { }

                        try
                        {
                            if (stateMatrix[posX, posY + 1] != 1)
                            {
                                stateMatrix[posX, posY + 1] = 5;
                            }
                        }
                        catch { }
                    }
                    //fine FOV
                }
                catch 
                { }
                break;
            default:
                break;
        }
                
        return stateMatrix;
    }

    public void checkLightOnEnemies()
    {
        foreach (GameObject e in _enemyPool)
        {
            e.GetComponent<EnemyMovement>().checkLightOnEnemy();
        }
    }
     
   
}
