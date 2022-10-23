using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
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


}
