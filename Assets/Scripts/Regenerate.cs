using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Linq;
using Unity.MLAgents;
using System;
using Unity.VisualScripting;

public class Regenerate : MonoBehaviour
{   

 
    public GameObject obstacles;
    public GameObject player;
    public GameObject agents;
    public GameObject goal;
    [Range(0f, 1f)]
    public float rationNObstacles;
    public int nObstacles;
    public GameObject obstaclesPrefab;
    public GameObject enemyPrefab;
    public GameObject playerPrefab;
    public LayerMask cantMove;
    public bool _training;
    public int width = 10;
    public int height = 10;
    private int[,] spawnStateMatrix;
    int[] feasible = new int[9];
    float[] pos = new float[2];
    float[] posNear = new float[2];
    Vector3 aux3dVector = new Vector3(0f, 0f, 0f);
    int[,] stateMatrix; // 0: blank

    public List<GameObject> _enemyPool;
    
    public static Regenerate instance;


    public Vector3 setAndGetVector(float x, float y)
    {
        aux3dVector[0] = x;
        aux3dVector[1] = y;
        return aux3dVector;
    }
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
        
        // Create matrices based on variables
        spawnStateMatrix = new int[width, height];
        stateMatrix = new int[width, height];
        transform.localScale += new Vector3((width - 10) * 0.1f, (height - 10) * 0.1f, 0f);
        nObstacles = (int) (width * height * rationNObstacles);
        
        for (int i = 0; i < nObstacles; i++)
        {            
            GameObject instantiatedObject = Instantiate(obstaclesPrefab, setAndGetVector(69f, 69f), Quaternion.identity);
            instantiatedObject.transform.SetParent(obstacles.transform);
        }
        
        // Populate enemy pool
        for (int i = 0; i < 10; i++)
        {
            GameObject instantiatedObject = Instantiate(enemyPrefab, setAndGetVector(-99f, -99f), Quaternion.identity);
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
        for (int i = 0; i < 9; i++)
        {
            feasible[i] = 0; 
        }
        
        // TODO: FOR NOW THE WAIT ACTION IS ALWAYS INFEASIBLE
        feasible[8] = 0; // wait always feasible

        if (!Physics2D.OverlapCircle(objPosition + setAndGetVector(1f, 0f), .2f, cantMove)) // E
        {
            feasible[0] = 1; 
        }
        if (!Physics2D.OverlapCircle(objPosition + setAndGetVector(1f, -1f), .2f, cantMove)) // SE
        {
            feasible[1] = 1;
        }
        if (!Physics2D.OverlapCircle(objPosition + setAndGetVector(0f, -1f), .2f, cantMove)) // S
        {
            feasible[2] = 1;
        }
        if (!Physics2D.OverlapCircle(objPosition + setAndGetVector(-1f, -1f), .2f, cantMove)) // SO
        {
            feasible[3] = 1;
        }
        if (!Physics2D.OverlapCircle(objPosition + setAndGetVector(-1f, 0f), .2f, cantMove)) // O
        {
            feasible[4] = 1;
        }
        if (!Physics2D.OverlapCircle(objPosition + setAndGetVector(-1f, 1f), .2f, cantMove)) // NO
        {
            feasible[5] = 1;
        }
        if (!Physics2D.OverlapCircle(objPosition + setAndGetVector(0f,1f), .2f, cantMove)) // N
        {
            feasible[6] = 1;
        }
        if (!Physics2D.OverlapCircle(objPosition + setAndGetVector(1f, 1f), .2f, cantMove)) // NE
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
                    spawnStateMatrix[(int)(x + (width / 2) - 0.5f) + i, Mathf.Abs((int)(y - (height/ 2) - 0.5f)) + j] = 1;
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
                    spawnStateMatrix[(int)(x + (width / 2) - 0.5f) + 2, Mathf.Abs((int)(y - ((height / 2) - 0.5f)))] = 1;
                    break;
                case (1):
                    spawnStateMatrix[(int)(x + (width / 2) - 0.5f) + 2, Mathf.Abs((int)(y - ((height / 2) - 0.5f))) - 2] = 1;
                    break;
                case (2):
                    spawnStateMatrix[(int)(x + (width / 2) - 0.5f), Mathf.Abs((int)(y - ((height / 2) - 0.5f))) - 2] = 1;
                    break;
                case (3):
                    spawnStateMatrix[(int)(x + (width / 2) - 0.5f) - 2, Mathf.Abs((int)(y - ((height / 2) - 0.5f))) - 2] = 1;
                    break;
                case (4):
                    spawnStateMatrix[(int)(x + (width / 2) - 0.5f) - 2, Mathf.Abs((int)(y -((height / 2) - 0.5f)))] = 1;
                    break;
                case (5):
                    spawnStateMatrix[(int)(x + (width / 2) - 0.5f) - 2, Mathf.Abs((int)(y - ((height / 2) - 0.5f))) + 2] = 1;
                    break;
                case (6):
                    spawnStateMatrix[(int)(x + (width / 2) - 0.5f), Mathf.Abs((int)(y - ((height / 2) - 0.5f))) + 2] = 1;
                    break;
                case (7):
                    spawnStateMatrix[(int)(x + (width / 2) - 0.5f) + 2, Mathf.Abs((int)(y - ((height / 2) - 0.5f))) + 2] = 1;
                    break;
                default:
                    break;
            }
        }
        catch { }
    }

    private float[] getFensibleIndexs(int[,] spawnStateMatrix)
    {
        float xP = UnityEngine.Random.Range(-(width / 2), (width / 2) - 1) + 0.5f;  
        float yP = UnityEngine.Random.Range(-(height / 2), (height / 2) - 1) + 0.5f;
        int itr = 0;                                      
        
        
        while (spawnStateMatrix[(int)(xP + (width / 2) - 0.5f), Mathf.Abs((int)(yP - ((height / 2) - 0.5f)))] == 1 && itr < 80)
        {
            xP = UnityEngine.Random.Range(-(width / 2), (width / 2) - 1) + 0.5f;  
            yP = UnityEngine.Random.Range(-(height / 2), (height / 2) - 1) + 0.5f;
            itr++;
        }

        if (itr == 80)
        {
            xP = UnityEngine.Random.Range(50, 60) + 0.5f;
            yP = UnityEngine.Random.Range(50, 60) + 0.5f;
        }

        pos[0] = xP;
        pos[1] = yP;

        return pos;
    }

    public void CreateMap()
    {

        int itr = 0;

        foreach (Transform child in obstacles.transform)
        {
            child.position = setAndGetVector(69f, 69f);
        }

        // Reset enemy pool
        foreach (GameObject e in _enemyPool)
        {
            RemoveEnemyFromPool(e);
        }

        goal.transform.position = setAndGetVector(89f, 89f);
        player.transform.position = setAndGetVector(79f, 79f);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                spawnStateMatrix[i, j] = 0;
            }
        }

        float[] posXY = getFensibleIndexs(spawnStateMatrix);
        setFull(posXY[0], posXY[1], spawnStateMatrix);
        player.transform.position = setAndGetVector(posXY[0], posXY[1]);
        player.GetComponent<PlayerController>().movePoint.position = setAndGetVector(posXY[0], posXY[1]);
        player.GetComponent<EnemyMovement>()._isMoving = false;
        player.GetComponent<PlayerController>()._startedAction = false;
        player.GetComponent<EnemyMovement>()._hasStarted = false;
        player.GetComponent<PlayerController>().randomFlashLightOrientation();
        setTorch(posXY[0], posXY[1], spawnStateMatrix, player.GetComponent<PlayerController>().flashlightState);

        float enemyRadius = Academy.Instance.EnvironmentParameters.GetWithDefault("enemyRadius", 5);
        posXY = getFensibleIndexDistance(spawnStateMatrix, posXY, (int)enemyRadius);
        spawnStateMatrix[(int)(posXY[0] + (width / 2) - 0.5f), Mathf.Abs((int)(posXY[1] - ((height / 2) - 0.5f)))] = 1;
        spawnEnemy(posXY[0], posXY[1]);
        
        float goalRadius = Academy.Instance.EnvironmentParameters.GetWithDefault("goalRadius", 2);
        float[] posXY_near = getFensibleIndexDistance(spawnStateMatrix, posXY, (int)goalRadius);
        setFull(posXY_near[0], posXY_near[1], spawnStateMatrix);
        goal.transform.position = setAndGetVector(posXY_near[0], posXY_near[1]);               
        setFull(posXY[0], posXY[1], spawnStateMatrix);
                
        foreach (Transform child in obstacles.transform)
        {
            float[] posXY_obs = getFensibleIndexs(spawnStateMatrix);
            setFull(posXY_obs[0], posXY_obs[1], spawnStateMatrix);
            child.position = setAndGetVector(posXY_obs[0], posXY_obs[1]);
        }
    }

    private float[] getFensibleIndexDistance(int[,] spawnStateMatrix, float[] pos, int radius)
    {
        int maxX = (int)Math.Min(pos[0] - 0.5f + radius, (width / 2) - 1);       
        int minX = (int)Math.Max(pos[0] - 0.5f - radius, -(width / 2));

        int maxY = (int)Math.Min(pos[1] - 0.5f + radius, (height / 2) - 1);
        int minY = (int)Math.Max(pos[1] - 0.5f - radius, -(height / 2));

        float xP = UnityEngine.Random.Range(minX, maxX) + 0.5f;           
        float yP = UnityEngine.Random.Range(minY, maxY) + 0.5f;
               
        while (spawnStateMatrix[(int)(xP + (width / 2) - 0.5f), Mathf.Abs((int)(yP - ((height / 2) - 0.5f)))] == 1)
        {
            xP = UnityEngine.Random.Range(minX, maxX) + 0.5f;
            yP = UnityEngine.Random.Range(minY, maxY) + 0.5f;            
        }

        posNear[0] = xP;
        posNear[1] = yP;

        return posNear;
    }

    private void spawnEnemy(float x, float y)
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
        enemy.transform.position = setAndGetVector(x, y);
        enemy.transform.rotation = Quaternion.identity;
        enemy.GetComponent<EnemyMovement>().movePoint.transform.position = enemy.transform.position;
        enemy.GetComponent<VictimAgent>()._inference = !_training;
        enemy.GetComponent<VictimAgent>()._waitingForAction = false;
        enemy.GetComponent<EnemyMovement>().goal = goal;
        enemy.GetComponent<EnemyMovement>().player = player;
        enemy.GetComponent<EnemyMovement>()._hasStarted = false;
        enemy.GetComponent<EnemyMovement>()._isMoving = false;

        enemy.transform.SetParent(agents.transform);
    }

    
    
    public void RemoveEnemyFromPool(GameObject enemy)
    {
        enemy.transform.SetParent(transform);
        enemy.transform.position = setAndGetVector(-99f, -99f);
        enemy.GetComponent<EnemyMovement>().movePoint.transform.position = enemy.transform.position;
        enemy.SetActive(false);
    }

    public int[,] getCropStateMatrix(Vector3 agentPosition, int radius)
    {
        int xRow = (int) (agentPosition.x + (width / 2) - 0.5f);
        int yRow = (int)(Mathf.Abs((int)(agentPosition.y - ((height / 2) - 0.5f))));
                
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
                if (i >= 0 && i <= width - 1 && j >= 0 && j <= height - 1)
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
        for(int i = 0; i<width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                stateMatrix[i, j] = 0;
            }
        }
        foreach (Transform child in obstacles.transform)
        {
            try
            {
                stateMatrix[(int)(child.position.x + (width / 2) - 0.5f), Mathf.Abs((int)(child.position.y - ((height / 2) - 0.5f)))] = 1; // 1: obstacles
            }
            catch
            {              
            }

        }

        foreach (Transform child in agents.transform)
        {
            stateMatrix[(int)(child.position.x + (width / 2) - 0.5f), Mathf.Abs((int)(child.position.y - ((height / 2) - 0.5f)))] = 2; // 2: agents
        }


        stateMatrix[(int)(goal.transform.position.x + (width / 2) - 0.5f), Mathf.Abs((int)(goal.transform.position.y - ((height / 2) - 0.5f)))] = 3; // 3: goal

        int posX = (int)(player.transform.position.x + (width / 2) - 0.5f);
        int posY = Mathf.Abs((int)(player.transform.position.y - ((height / 2) - 0.5f)));

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
                }
                catch
                { }


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

                }
                catch
                { }


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
                }
                catch
                { }


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
                }
                catch
                { }

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
                }
                catch
                { }

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
                }
                catch
                { }

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
                }
                catch
                {
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
                }
                catch
                { }

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
            if(e.activeSelf)
                e.GetComponent<EnemyMovement>().checkLightOnEnemy();
        }
    }
     
   
}