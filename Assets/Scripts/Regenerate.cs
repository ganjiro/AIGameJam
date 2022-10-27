using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Linq;
using Unity.MLAgents;
using System;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Regenerate : MonoBehaviour
{   

 
    public GameObject obstacles;
    public GameObject player;
    public GameObject agents;
    public GameObject goal;
    [Range(0f, 1f)]
    public float rationNObstacles;
    public int nObstacles;
    public int nEnemies = 10;

    public GameObject obstaclesPrefab;
    public GameObject enemyPrefab;
    public GameObject playerPrefab;
    public LayerMask cantMove;
    public bool _training;
    public int width = 10;
    public int height = 10;
    public bool spawnGoalInThePlayer;
    private int[,] spawnStateMatrix;
    int[] feasible = new int[9];
    float[] pos = new float[2];
    float[] posNear = new float[2];
    Vector3 aux3dVector = new Vector3(0f, 0f, 0f);
    int[,] stateMatrix; // 0: blank

    public Canvas _gameOverCanvas;
    public Canvas _gameWonCanvas;
    public Canvas _madCanvas;

    public GlobalBlackboard _blackboard;
    public UIManager _uiManager;

    public List<GameObject> _enemyPool;
    public List<GameObject> goodSprites;
    public List<GameObject> madSrpites;
    public Image _goodBackground;
    public Image _badBackground;
    
    public static Regenerate instance;
    public string _nextLevel;


    public Vector3 setAndGetVector(float x, float y)
    {
        aux3dVector[0] = x;
        aux3dVector[1] = y;
        return aux3dVector;
    }

    // Load the same scene where we are
    public void GameOver()
    {
        if (_gameOverCanvas.gameObject.active)
        {
            return;
        }
        // Add all enemies to the pool
        foreach(Transform e in Regenerate.instance.agents.transform)
        {
            RemoveEnemyFromPool(e.gameObject);
        }
        // Increase madness value
        GlobalBlackboard.instance.IncreaseMadnessValue();
        if (GlobalBlackboard.instance.GetMadnessPerc() >= 1)
        {
            _madCanvas.gameObject.SetActive(true);
            _madCanvas.transform.GetChild(1).gameObject.SetActive(true);
            _madCanvas.transform.GetChild(2).gameObject.SetActive(false);

        }
        else
        {
            _gameOverCanvas.gameObject.SetActive(true);
            foreach(Animation a in _gameOverCanvas.GetComponentsInChildren<Animation>())
            {
                a.Play();
            }
            // StartCoroutine(fadeScreen(_gameOverCanvas));
        }
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public IEnumerator fadeScreen(Canvas canvas)
    {
        float alphaFactor = 0.01f;
        foreach (Image i in canvas.GetComponentsInChildren<Image>())
        {
            if (i.color.a >= 1)
            {

                i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
            }

            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a + alphaFactor);
        }
        Debug.Log("boh");

        if (canvas.GetComponentsInChildren<Image>()[0].color.a < 1)
        {
            Debug.Log("yeald");
            yield return null;
        }
    }

    public void GameWon()
    {
        if (_gameOverCanvas.gameObject.active)
        {
            return;
        }
        // Add all enemies to the pool
        foreach(Transform e in Regenerate.instance.agents.transform)
        {
            RemoveEnemyFromPool(e.gameObject);
        }
        if (GlobalBlackboard.instance.GetMadnessPerc() >= 1)
        {
            _madCanvas.gameObject.SetActive(true);
            _madCanvas.transform.GetChild(2).gameObject.SetActive(true);
            _madCanvas.transform.GetChild(1).gameObject.SetActive(false);

        }
        else
        {
            _gameWonCanvas.gameObject.SetActive(true);
        }

        // SceneManager.LoadScene(_nextLevel);
    }

    public void ReloadLevel(bool sameLevel)
    {
        if (sameLevel)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);    
        }
        else
        {
            SceneManager.LoadScene(_nextLevel);
        }
    }

    // Check if the player has finished its turns
    public bool CheckMaxRound()
    {
        return player.GetComponent<PlayerController>().actualRound >= player.GetComponent<PlayerController>().maxRound;
    }

    public int CheckNumberEnemies()
    {
        return player.GetComponent<PlayerController>().getNumberAliveEnemies();
    }

    public int CheckWinCondition()
    {
        return player.GetComponent<PlayerController>().getNumberEnemiesToKill();
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
        // DontDestroyOnLoad(gameObject);
        InitializeAll();
    }

    void InitializeAll()
    {
        
        // Disable game over/won canvases
        _gameWonCanvas.gameObject.SetActive(false);
        _gameOverCanvas.gameObject.SetActive(false);
        _madCanvas.gameObject.SetActive(false);

        // Get blackboard
        try
        {
            _blackboard = GameObject.Find("BlackBoard").GetComponent<GlobalBlackboard>();
            _uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
        }
        catch
        {

        }

        // Create matrices based on variables
        spawnStateMatrix = new int[width, height];
        stateMatrix = new int[width, height];
        if(SceneManager.GetActiveScene().name == "Big Scene")
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

        // Set player to UI
        if(_uiManager != null)
        {
            _uiManager.LinkButtons();
        }
        
        // If madness level is at its maximum, change the sprite
        // MADNESS MODE
        if (_blackboard != null && _blackboard.GetMadnessPerc() >= 1)
        {
            // Disable good sprites
            foreach(GameObject go in goodSprites)
            {
                go.SetActive(false);
            }
            
            // Enable mad sprites
            foreach(GameObject go in madSrpites)
            {
                go.SetActive(true);
            }
            
            // Do the same for the sprites
            // player
            player.transform.Find("happyPlayer").gameObject.SetActive(false);
            player.transform.Find("DarkSprite").gameObject.SetActive(true);
            player.GetComponent<PlayerController>().animator = player.transform.Find("DarkSprite").GetComponent<Animator>();
            // agents
            foreach (Transform a in agents.transform)
            {
                a.transform.Find("happySprite").gameObject.SetActive(false);
                a.transform.Find("DarkSprite").gameObject.SetActive(true);
                a.GetComponent<PlayerController>().animator = a.transform.Find("DarkSprite").GetComponent<Animator>();
            }
        }
        _goodBackground.color = new Color(_goodBackground.color.r, _goodBackground.color.b, _goodBackground.color.g, 1 - GlobalBlackboard.instance.GetMadnessPerc());

        // Set up the number of enemies the player has to kill as the total number of eneimes
        player.GetComponent<PlayerController>()._hasToKillInThisLevel = agents.transform.childCount;
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
        
        // Check if player has killed the right number of enemies
        if (CheckWinCondition() == 0)
        {
            GameWon();
            return;
        }

        // Check if player has reached its maxround. If so, do game over
        if(CheckMaxRound())
        {
            GameOver();
            return;
        }
        
        // Do game over eve if the are no enemies in the scene
        if (CheckNumberEnemies() <= 0)
        {
            GameOver();
            return;
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
        float[] playerPosXY = posXY;

        float enemyRadius = Academy.Instance.EnvironmentParameters.GetWithDefault("enemyRadius", width * (float)Math.Sqrt(2));

        for (int i = 0; i < nEnemies; i++)
        {
            posXY = getFensibleIndexDistance(spawnStateMatrix, posXY, (int)enemyRadius);
            spawnStateMatrix[(int)(posXY[0] + (width / 2) - 0.5f), Mathf.Abs((int)(posXY[1] - ((height / 2) - 0.5f)))] = 1;
            spawnEnemy(posXY[0], posXY[1]);    
        }
        
        float goalRadius = Academy.Instance.EnvironmentParameters.GetWithDefault("goalRadius", width * (float)Math.Sqrt(2));


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

                    try
                    {
                        if (stateMatrix[posX, posY - 1] != 1)
                        {
                            stateMatrix[posX, posY - 1] = 5;
                        }
                    }
                    catch
                    {
                        
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

                try
                {
                    if (stateMatrix[posX, posY - 1] != 1)
                    {
                        stateMatrix[posX, posY - 1] = 5;
                    }    
                }
                catch{ }
                

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