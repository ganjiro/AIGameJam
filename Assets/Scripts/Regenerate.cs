using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

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


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.K))
        {
            foreach (Transform child in obstacles.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            foreach (Transform child in agents.transform)
            {
                Destroy(child.GetComponent<EnemyMovement>().movePoint.gameObject);
                GameObject.Destroy(child.gameObject);
            }
                                 

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

            //spawnEnemy();
            spawnEnemy();

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

    }

    private void spawnEnemy()
    {
        float x = Random.Range(-5, 4) + 0.5f;
        float y = Random.Range(-5, 4) + 0.5f;

        while (Physics2D.OverlapCircle(new Vector3(x, y, 0f), .2f, cantMove))
        {
            x = Random.Range(-5, 4) + 0.5f;
            y = Random.Range(-5, 4) + 0.5f;
        }

        GameObject instantiatedObject = Instantiate(enemyPrefab, new Vector3(x, y, 0f), Quaternion.identity);
        instantiatedObject.transform.SetParent(agents.transform);
    }
}
