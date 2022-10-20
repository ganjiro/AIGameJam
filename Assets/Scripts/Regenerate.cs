using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Linq;

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
            CreateMap();
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

    public void CreateMap()
    {
        foreach (Transform child in obstacles.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        
        // Reset enemy pool
        foreach (GameObject e in _enemyPool)
        {
            e.transform.SetParent(transform);
            e.transform.position = new Vector3(-99f, -99f, 0f);
            e.GetComponent<EnemyMovement>().movePoint.transform.position = e.transform.position;
            e.SetActive(false);
        }

        // foreach (Transform child in agents.transform)
        // {
        //     Destroy(child.GetComponent<EnemyMovement>().movePoint.gameObject);
        //     GameObject.Destroy(child.gameObject);
        // }

        for (int i=0; i < nObstacles; i++) {

            float x = Random.Range(-5, 4) + 0.5f;
            float y = Random.Range(-5, 4) + 0.5f;

            while (Physics2D.OverlapCircle(new Vector3(x, y, 0f), 1.2f, cantMove)) 
            {
                x = Random.Range(-5, 4) + 0.5f;
                y = Random.Range(-5, 4) + 0.5f;
            }

            GameObject instantiatedObject = Instantiate(obstaclesPrefab, new Vector3(x, y, 0f), Quaternion.identity);
            instantiatedObject.transform.SetParent(obstacles.transform);            

        }
        
        spawnEnemy();
        // spawnEnemy();

        float xP = Random.Range(-5, 4) + 0.5f;
        float yP = Random.Range(-5, 4) + 0.5f;

        while (Physics2D.OverlapCircle(new Vector3(xP, yP, 0f), .2f, cantMove))
        {
            xP = Random.Range(-5, 4) + 0.5f;
            yP = Random.Range(-5, 4) + 0.5f;
        }

        player.transform.position = new Vector3(xP, yP, 0f);
        player.GetComponent<PlayerController>().movePoint.position = new Vector3(xP, yP, 0f);

        xP = Random.Range(-5, 4) + 0.5f;
        yP = Random.Range(-5, 4) + 0.5f;

        while (Physics2D.OverlapCircle(new Vector3(xP, yP, 0f), .2f, cantMove))
        {
            xP = Random.Range(-5, 4) + 0.5f;
            yP = Random.Range(-5, 4) + 0.5f;
        }

        goal.transform.position = new Vector3(xP, yP, 0f);
    }

    private void spawnEnemy()
    {
        // Get the first non active enemy from the pool
        
        float x = Random.Range(-5, 4) + 0.5f;
        float y = Random.Range(-5, 4) + 0.5f;

        while (Physics2D.OverlapCircle(new Vector3(x, y, 0f), .2f, cantMove))
        {
            x = Random.Range(-5, 4) + 0.5f;
            y = Random.Range(-5, 4) + 0.5f;
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
        enemy.transform.rotation = Quaternion.identity;
        enemy.GetComponent<EnemyMovement>().movePoint.transform.position = enemy.transform.position;
        enemy.GetComponent<VictimAgent>()._inference = !_training;
        enemy.transform.SetParent(agents.transform);
    }


    public int[,] getFullStateMatrix()
    {
        int[,] stateMatrix = new int[10, 10];

        foreach (Transform child in obstacles.transform)
        {
            stateMatrix[(int)(child.position.x + 4.5), Mathf.Abs((int)(child.position.y - 4.5))] = 1;
        }

        foreach (Transform child in agents.transform)
        {
            stateMatrix[(int)(child.position.x + 4.5), Mathf.Abs((int)(child.position.y - 4.5))] = 2;
        }


        stateMatrix[(int)(goal.transform.position.x + 4.5), Mathf.Abs((int)(goal.transform.position.y - 4.5))] = 3;
        stateMatrix[(int)(player.transform.position.x + 4.5), Mathf.Abs((int)(player.transform.position.y - 4.5))] = 4;

        int flashState = player.GetComponent<PlayerController>().flashlightState;

        switch (flashState)
        {
            case (0):
                try
                {
                    stateMatrix[(int)(player.transform.position.x + 4.5), Mathf.Abs((int)(player.transform.position.y - 4.5)) + 1] = 5;
                    stateMatrix[(int)(player.transform.position.x + 4.5), Mathf.Abs((int)(player.transform.position.y - 4.5)) + 2] = 5;
                }
                catch
                { }

                break;

            case (1):
                try
                {
                    stateMatrix[(int)(player.transform.position.x + 4.5) + 1, Mathf.Abs((int)(player.transform.position.y - 4.5)) - 1] = 5;
                    stateMatrix[(int)(player.transform.position.x + 4.5) + 2, Mathf.Abs((int)(player.transform.position.y - 4.5)) - 2] = 5;
                }
                catch 
                { }
                break;
            case (2):
                try
                {
                    stateMatrix[(int)(player.transform.position.x + 4.5), Mathf.Abs((int)(player.transform.position.y - 4.5)) - 1] = 5;
                    stateMatrix[(int)(player.transform.position.x + 4.5), Mathf.Abs((int)(player.transform.position.y - 4.5)) - 2] = 5;
                }
                catch 
                { }
                break;
            case (3):
                try
                {
                    stateMatrix[(int)(player.transform.position.x + 4.5) - 1, Mathf.Abs((int)(player.transform.position.y - 4.5)) - 1] = 5;
                    stateMatrix[(int)(player.transform.position.x + 4.5) - 2, Mathf.Abs((int)(player.transform.position.y - 4.5)) - 2] = 5;
                }
                catch
                { }
                break;
            case (4):
                try
                {
                    stateMatrix[(int)(player.transform.position.x + 4.5) - 1, Mathf.Abs((int)(player.transform.position.y - 4.5))] = 5;
                    stateMatrix[(int)(player.transform.position.x + 4.5) - 2, Mathf.Abs((int)(player.transform.position.y - 4.5))] = 5;
                }
                catch 
                { }
                break;
            case (5):
                try
                {
                    stateMatrix[(int)(player.transform.position.x + 4.5) - 1, Mathf.Abs((int)(player.transform.position.y - 4.5)) + 1] = 5;
                    stateMatrix[(int)(player.transform.position.x + 4.5) - 2, Mathf.Abs((int)(player.transform.position.y - 4.5)) + 2] = 5;
                }
                catch 
                { }
                break;
            case (6):
                try
                {
                    stateMatrix[(int)(player.transform.position.x + 4.5), Mathf.Abs((int)(player.transform.position.y - 4.5)) + 1] = 5;
                    stateMatrix[(int)(player.transform.position.x + 4.5), Mathf.Abs((int)(player.transform.position.y - 4.5)) + 2] = 5;
                }
                catch 
                { }
                break;
            case (7):
                try
                {
                    stateMatrix[(int)(player.transform.position.x + 4.5) + 1, Mathf.Abs((int)(player.transform.position.y - 4.5)) + 1] = 5;
                    stateMatrix[(int)(player.transform.position.x + 4.5) + 2, Mathf.Abs((int)(player.transform.position.y - 4.5)) + 2] = 5;
                }
                catch 
                { }
                break;
            default:
                break;
        }

        // string sus;
        // for (int i = 0; i < 10; i++)
        // {
        //     sus = "";
        //     for (int j = 0; j < 10; j++)
        //     {
        //         sus += stateMatrix[i, j].ToString();
        //     }
        //     Debug.Log(sus);
        // }


        return stateMatrix;
    }
}
