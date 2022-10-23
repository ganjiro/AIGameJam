using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine.UI;

public class VictimAgent : Agent
{

    public int _stateDim;
    public bool _inference;
    public bool _waitingForAction;
    
    private int _stepCount = 0;
    private BarracudaModel _brain;

    public int _currentAction = 99;
    public int _defaultActionValue = 99;

    // Start is called before the first frame update
    void Start()
    {
        _brain = GetComponent<BarracudaModel>();
        _currentAction = _defaultActionValue;
    }
    
    /*public void FixedUpdate()
    {
        if(_stepCount % _timeScale == 0)	
        {
            if(_inference)
                MakeAction(_brain.requestDiscreteDecision(CreateStateObservation()));
            else
                RequestDecision();
        }

        _stepCount++;
    }*/
    
    // Call it when the episode restarts.
    public override void OnEpisodeBegin()
    {
        _stepCount = 0;
        _currentAction = _defaultActionValue;
        GetComponent<EnemyMovement>()._isMoving = false;
        _waitingForAction = false;
    }
    
    // This is the actual method that will create the observation
    // It is called both from the built in package and custom Barracuda model. 
    private List<float> CreateStateObservation()
    {
        // Create a global map observation
        List<float> obs = new List<float>();
        int[,] categoricalMap = Regenerate.instance.getFullStateMatrix();
        foreach (int v in categoricalMap)
        {
            obs.Add(v);
        }
        
        // Create the local observation, 5x5 and 3x3
        int[,] local5Map = Regenerate.instance.getCropStateMatrix(transform.position, 2);
        foreach (int v in local5Map)
        {
            obs.Add(v);
        }
        
        // Create the local observation, 5x5 and 3x3
        int[,] local3Map = Regenerate.instance.getCropStateMatrix(transform.position, 1);
        foreach (int v in local3Map)
        {
            obs.Add(v);
        }
        
        // Create action masking
        // THIS MUST BE THE LAST PART OF THE OBS
        int[] feasibleActions = Regenerate.instance.getFeasibleActionset(transform.position);
        foreach(var a in feasibleActions)
        {
            obs.Add(a);
        }
        
        
        return obs;
    }
    
    // This is the actual method that will make the action.
    // It is called both from the built in package and custom Barracuda model. 
    public void MakeAction()
    {
        // Starting from here, the enemy is moving
        GetComponent<EnemyMovement>()._isMoving = true;
        _waitingForAction = true;
        
        if (_inference)
        {
            /*int[,] map = Regenerate.instance.getFullStateMatrix();
            Tile startTile = new Tile(map[(int)(transform.position.y + 4.5f), (int) (transform.position.x + 4.5f)], 
                new []{(int)(transform.position.y + 4.5f), (int) (transform.position.x + 4.5f)},
                new []{transform.position.y, transform.position.x});

            Transform goal = GetComponent<EnemyMovement>().goal.transform;
            Tile endTile = new Tile(map[(int)(goal.position.y + 4.5f), (int) (goal.position.x + 4.5f)], 
                new []{(int)(goal.position.y + 4.5f), (int) (goal.position.x + 4.5f)},
                new []{goal.position.y, goal.position.x});
            
            Debug.Log(transform.position.x + " " + transform.position.y);
            List<Tile> path = findPath(startTile, endTile, false, null, Regenerate.instance.getFullStateMatrix());
            Tile nextTile = path[path.Count - 1];
            GetComponent<EnemyMovement>().movePoint.transform.position =
                new Vector3(nextTile.worldCoordinates[1], nextTile.worldCoordinates[0], 0);
            Debug.Log(nextTile.worldCoordinates[1] + " " +  nextTile.worldCoordinates[0]);*/

            int[] actionMasking =  Regenerate.instance.getFeasibleActionset(transform.position);
            float[] fActionMasking = new float[actionMasking.Length];
            for (int i = 0; i < actionMasking.Length; i++)
            {
                fActionMasking[i] = actionMasking[i];
            }
            _currentAction = _brain.requestDiscreteDecision(CreateStateObservation(), fActionMasking);
        }
        else
        {
            RequestDecision();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(CreateStateObservation());
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];
        _currentAction = action;
    }

    public class Tile
    {
        public int value;
        public int[] matrixCoordinates;
        public float[] worldCoordinates;

        public Tile(int value, int[] matrixCoordinates, float[] worldCoordinates)
        {
            this.value = value;
            this.matrixCoordinates = matrixCoordinates;
            this.worldCoordinates = worldCoordinates;
        }
    }
    
    // Utility class for Djkstra
    public class TileDist
    {
        public Tile tile;
        public float dist;

        public TileDist(Tile tile)
        {
            this.tile = tile;
            this.dist = float.PositiveInfinity;
        }
    }

    private List<Tile> getNeighbourhood(Tile tile, List<Tile> allTiles)
    {

        List<Tile> neighborhood = new List<Tile>();
        int x = tile.matrixCoordinates[0];
        int y = tile.matrixCoordinates[1];

        int[] xs = {-1, 0, 1};
        int[] ys = {-1, 0, 1};
        foreach(int xn in xs)
        foreach (int yn in ys)
        {
            if (xn == 0 && yn == 0)
            {
                continue;
            }
            
            if (x + xn >= 0 && x + xn <= 9 &&
                y + yn >= 0 && y + yn <= 9)
            {
                foreach (Tile t in allTiles)
                {
                    if (t.matrixCoordinates[0] == x + xn && t.matrixCoordinates[1] == y + yn)
                    {
                        neighborhood.Add(t);
                    }
                }
            }
        }

        return neighborhood;
    }
    
    // Find the best path from startTile to endTile with Djkstra
    public List<Tile> findPath(Tile startTile, Tile endTile, bool withItem, List<Tile> avoidTiles, int[,] map)
    {
        Dictionary<Vector2, Tile> prev = new Dictionary<Vector2, Tile>();
        
        // Construct a list of tiles
        List<Tile> tiles = new List<Tile>();
        string msg = "";
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                tiles.Add(new Tile(map[j, i], new []{j, i}, new []{j - 5f + 0.5f, i + 0.5f - 5f }));
            }
        }
        
        List<TileDist> allTileDist = new List<TileDist>();
        List<TileDist> allTiles = new List<TileDist>();
        
                
        foreach (Tile t in tiles)
        {
            TileDist tileDist = new TileDist(t);
            allTileDist.Add(tileDist);
            allTiles.Add(tileDist);
            prev.Add(new Vector2(t.matrixCoordinates[0], t.matrixCoordinates[1]),  null);
        }

        allTileDist.First(item => (item.tile.matrixCoordinates[0] == startTile.matrixCoordinates[0] && 
                                   item.tile.matrixCoordinates[1] == startTile.matrixCoordinates[1])).dist = 0;
        while (allTileDist.Count > 0)
        {
            allTileDist = allTileDist.OrderBy(o => o.dist).ToList();
            TileDist u = allTileDist[0];
            if (u.tile.matrixCoordinates[0] == endTile.matrixCoordinates[0] && u.tile.matrixCoordinates[1] == endTile.matrixCoordinates[1])
                break;
            allTileDist.Remove(allTileDist[0]);
            if (float.IsPositiveInfinity(u.dist))
            {
                break;
            }
            
            foreach (Tile neigh in getNeighbourhood(u.tile, tiles))
            {

                if (neigh.value == 1 || neigh.value == 4 || neigh.value == 2 || neigh.value == 5)
                {
                    continue;
                    
                }

                if (allTiles.FirstOrDefault(item => (item.tile.matrixCoordinates[0] == neigh.matrixCoordinates[0] 
                                                     && item.tile.matrixCoordinates[1] == neigh.matrixCoordinates[1])) == null)
                {
                    TileDist tileDist = new TileDist(neigh);
                    allTiles.Add(tileDist);
                    allTileDist.Add(tileDist);
                }
                
                TileDist v = allTiles.First(item => (item.tile.matrixCoordinates[0] == neigh.matrixCoordinates[0] 
                                                     && item.tile.matrixCoordinates[1] == neigh.matrixCoordinates[1]));
                float alt = u.dist + computeDistance(v.tile, u.tile);
                if (alt < v.dist)
                {
                    v.dist = alt;
                    Debug.Log(u.tile.value);

                    prev[new Vector2(v.tile.matrixCoordinates[0], v.tile.matrixCoordinates[1])] = u.tile;

                }
            }
        }

        List<Tile> path = new List<Tile>();
        path.Add(endTile);
        Tile next = prev[new Vector2(endTile.matrixCoordinates[0], endTile.matrixCoordinates[1])];
        Debug.Log("Qui");
        Debug.Log(startTile.matrixCoordinates[0] + " " + startTile.matrixCoordinates[1]);

        Debug.Log(endTile.matrixCoordinates[0] + " " + endTile.matrixCoordinates[1]);
        Debug.Log(next.matrixCoordinates[0] + " " + next.matrixCoordinates[1]);
        Debug.Log(" ");
        int count = 0;
        while ((next.matrixCoordinates[0] != startTile.matrixCoordinates[0] || next.matrixCoordinates[1] != startTile.matrixCoordinates[1]) && count < 1000)
        {
            path.Add(next);
            try
            {
                next = prev[new Vector2(next.matrixCoordinates[0], next.matrixCoordinates[0])];
                Debug.Log(next.matrixCoordinates[0] + " " + next.matrixCoordinates[1]);
                Debug.Log(" ");
            }
            catch (System.Exception)
            {
                return null;
            }

            count++;
        }
        Debug.Log(count);
        return path;
    }

    // Same as before, but avoiding collectibles
    public List<Tile> findPath(Tile startTile, Tile endTile, int[,] map)
    {
        return findPath(startTile, endTile, false, null, map);
    }

    // Compute distance value from 2 tile; the tiles in oblique position have
    // less distance then the other
    public float computeDistance(Tile tile1, Tile tile2)
    {
        Vector2 startPosition = new Vector2(tile1.worldCoordinates[0], tile1.worldCoordinates[1]);
        Vector2 endPosition = new Vector2(tile2.worldCoordinates[0], tile2.worldCoordinates[1]);

        float distance = Vector2.Distance(startPosition, endPosition);

        if (distance > 1)
        {
            return 1.0f;
        }

        return 0.75f;
    }
}
