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
    public bool _decision;
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
            if(_decision)
            {
                List<Tile> path = findPath(false, null, Regenerate.instance.getFullStateMatrix());
                Tile nextTile = path[path.Count - 1];

                _currentAction = GetComponent<EnemyMovement>().VectorToAction(nextTile.getVectWorldCoord());
            }
            else
            {
                int[] actionMasking =  Regenerate.instance.getFeasibleActionset(transform.position);
                float[] fActionMasking = new float[actionMasking.Length];
                for (int i = 0; i < actionMasking.Length; i++)
                {
                    fActionMasking[i] = actionMasking[i];
                }
                _currentAction = _brain.requestDiscreteDecision(CreateStateObservation(), fActionMasking);
            }
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

        public Vector2 getVectMatrixCoord()
        {
            return new Vector2(matrixCoordinates[0], matrixCoordinates[1]);
        }

        public Vector2 getVectWorldCoord()
        {
            return new Vector2(worldCoordinates[1], worldCoordinates[0]);
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

    private List<Tile> getNeighbourhood(Tile tile, List<Tile> allTiles, List<Tile> q)
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
                foreach (Tile t in q)
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
    public List<Tile> findPath(bool withItem, List<Tile> avoidTiles, int[,] map)
    {
        Dictionary<Vector2, Tile> prev = new Dictionary<Vector2, Tile>();
        Dictionary<Vector2, float> dist = new Dictionary<Vector2, float>();
        List<Tile> q = new List<Tile>();

        Tile startTile = null;
        Tile endTile = null;
        
        // Construct a list of tiles
        List<Tile> tiles = new List<Tile>();
        string msg = "";
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                msg = msg + map[j, i] + " ";
                if(map[j, i] == 0 || map[j, i] == 3 || map[j, i] == 5 || map[j, i] == 2 )
                {
                    tiles.Add(new Tile(map[j, i], new []{j, i}, new []{-(i - 5f + 0.5f), j - 5f + 0.5f }));
                }
                if(map[j, i] == 2)
                {
                    startTile = new Tile(map[j, i], new []{j, i}, new []{-(i - 5f + 0.5f), j - 5f + 0.5f });
                }
                if(map[j, i] == 3)
                {
                    endTile = new Tile(map[j, i], new []{j, i}, new []{-(i - 5f + 0.5f), j - 5f + 0.5f });
                }
                    
            }
        }
                
        foreach (Tile t in tiles)
        {

            dist.Add(t.getVectMatrixCoord(), float.PositiveInfinity);
            prev.Add(t.getVectMatrixCoord(),  null);
            q.Add(t);
        }

        dist[startTile.getVectMatrixCoord()] = 0;

        while (q.Count > 0)
        {

            // Get u with min dist[u]
            Tile u = null;
            float minDist = float.PositiveInfinity;
            int minIndex = -99;
            for(int i = 0; i < q.Count; i++)
            {
                Tile uTmp = q[i];
                if(dist[uTmp.getVectMatrixCoord()] < minDist)
                {
                    u = uTmp;
                    minDist = dist[uTmp.getVectMatrixCoord()];
                    minIndex = i;
                }
            }

            // If start, break
            if(u.getVectMatrixCoord()[0] == endTile.getVectMatrixCoord()[0] && u.getVectMatrixCoord()[1] == endTile.getVectMatrixCoord()[1])
            {
                break;
            }

            // Remove u from q
            q.RemoveAt(minIndex);

            foreach (Tile neigh in getNeighbourhood(u, tiles, q))
            {
                if (neigh.value == 1 || neigh.value == 4)
                {
                    // continue;   
                }
                else
                {
                    float alt = dist[u.getVectMatrixCoord()] + computeDistance(u, neigh);
                    
                    if(alt < dist[neigh.getVectMatrixCoord()])
                    {
                        dist[neigh.getVectMatrixCoord()] = alt;
                        prev[neigh.getVectMatrixCoord()] = u;
                    }
                }
            }
        }

        List<Tile> path = new List<Tile>();
        path.Add(endTile);
        Tile next = prev[endTile.getVectMatrixCoord()];
        int count = 0;
        while ((next.matrixCoordinates[0] != startTile.matrixCoordinates[0] || next.matrixCoordinates[1] != startTile.matrixCoordinates[1]) && count < 1000)
        {
            path.Add(next);
            try
            {
                next = prev[next.getVectMatrixCoord()];
            }
            catch (System.Exception)
            {
                return null;
            }

            count++;
        }
        return path;
    }

    // Same as before, but avoiding collectibles
    public List<Tile> findPath(Tile startTile, Tile endTile, int[,] map)
    {
        return findPath(false, null, map);
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
