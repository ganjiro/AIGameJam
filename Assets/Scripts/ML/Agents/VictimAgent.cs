using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.UI;

public class VictimAgent : Agent
{

    public int _stateDim;
    public bool _inference;
    
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
        Debug.Log("Reset");
        _stepCount = 0;
        _currentAction = _defaultActionValue;

    }
    
    // This is the actual method that will create the observation
    // It is called both from the built in package and custom Barracuda model. 
    private List<float> CreateStateObservation()
    {
        // Create a dummy observation
        List<float> obs = new List<float>(_stateDim);
        for (int i = 0; i < _stateDim; i++)
            obs.Add(i);
        return obs;
    }
    
    // This is the actual method that will make the action.
    // It is called both from the built in package and custom Barracuda model. 
    public void MakeAction()
    {
        if(_inference)
            _currentAction = _brain.requestDiscreteDecision(CreateStateObservation());
        else
            RequestDecision();
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
