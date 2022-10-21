using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;
using System.IO;
using Unity.Barracuda;
using Unity.Collections;
using System.Linq;
using UnityEditor;
using System;
using System.ComponentModel.Design;
using Unity.MLAgents.Demonstrations;
using Random = System.Random;


public class BarracudaModel : MonoBehaviour
{
    protected  Model _runtimeModel;
    protected IWorker _worker;
    public NNModel _currentModel;
    protected int _stateDim;

    public bool _actionMasking;
    public int _actionSize;

    [HideInInspector]
    public int _actionDim;
    public int _top = 3;
    protected string _outputName = "actions";
    
    Tensor input;
    Tensor output;
    
    
    protected void Start()
    {
        // Get the state dimensionality
        _stateDim = GetComponent<Unity.MLAgents.Policies.BehaviorParameters>().BrainParameters.VectorObservationSize;
        if (_currentModel != null)
        {
            LoadModel();
            if(_actionMasking)
                input = new Tensor(1, _stateDim - _actionSize);
            else
                input = new Tensor(1, _stateDim);
        }
    }

    public void LoadModel()
    {
        if(_worker != null)
        {
            _worker.Dispose();
        }
        createWorker(_currentModel);
    }
    
    // Create the worker to run the onnx model
    public void createWorker(NNModel model)
    {
        _runtimeModel = ModelLoader.Load(model);
        _currentModel = model;
        _outputName = _runtimeModel.outputs[0];
        _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, _runtimeModel);
    }
    
    // In case you have a discrete action space, this method will run the model given the state and 
    // will provide you the discrete action
    public int requestDiscreteDecision(List<float> state)
    {
        if(input == null)
            return 0;
        float[] probs = GetProbs(state);
        int action = SampleDiscreteAction(probs, false);
        return action;
    }

    public float[] Softmax(float[] logits)
    {
        var logits_exp = logits.Select(Mathf.Exp);
        var sum_logits_exp = logits_exp.Sum();
        var softmax = logits_exp.Select(i => i / (sum_logits_exp)).ToArray<float>();
        
        return softmax;
    }

    // Sample a discrete action given probabilities 
    public int SampleDiscreteAction(float[] probs, bool deterministic)
    {
        int action = 0;
        if(deterministic)
        {
            float maxProb = -Mathf.Infinity;
            for(int i = 0; i < _actionDim; i++)
            {
                if(probs[i] > maxProb) 
                {
                    action = i;
                    maxProb = probs[i];
                }
            }
        }
        else
        {
            action = Categorical(probs, _top);
        }

        return action;
    }
    
    public float GetEntropy(float[] probs, bool norm)
    {
        float entropy = 0;
        foreach(float p in probs)
        {
            entropy += p * Mathf.Log(p);
        }

        if(norm)
            return -entropy/(Mathf.Log(_actionDim));
        else
            return -entropy;
    }

    // Categorical sampling for discrete action space
    public int Categorical(float[] probs, int top)
    {
        int action = 0;

        // Define dictionary <action, probs>
        Dictionary<int, float> actionProbs = new Dictionary<int, float>();
        for (int i = 0; i < probs.Length; i++)
        {
            actionProbs.Add(i, probs[i]);
        }

        // Put the probability of the actions last NumAction - numToConsider
        // to 0, as we want to consider only the most probable numToConsider
        // action. We therefore compute the cumulative sum.
        int count = 0;
        float cumulativeSum = 0f;
        foreach (KeyValuePair<int, float> actionProb in actionProbs.OrderBy(key => key.Value))
        {
            if(count < probs.Length - top)
            {
                actionProbs[actionProb.Key] = 0f;
            }
            cumulativeSum += actionProbs[actionProb.Key];
            count++;
        }

        // Choose a random number between 0 and the cumulative sum
        float r = UnityEngine.Random.Range(0.00001f, cumulativeSum);
        float sum = 0f;
        // For each action in probability order
        foreach (KeyValuePair<int, float> actionProb in actionProbs.OrderBy(key => key.Value))
        {
            // Add to sum action prob
            sum += actionProb.Value;
            // If the random number is less then the sum at this point
            if (r <= sum)
            {
                // This is the action
                action = actionProb.Key;
                break;
            }
        }

        return action;
    }
    
    // If discrete action disctribution, get the probabilities given the state
    public float[] GetProbs(List<float> state)
    {

        if (_actionMasking)
        {
            for(int i = 0; i < _stateDim - _actionSize; i++)
            {
                input[0, i] = state[i];
            } 
            _worker.Execute(input);    
        }
        else
        {
            for(int i = 0; i < _stateDim; i++)
            {
                input[0, i] = state[i];
            } 
            _worker.Execute(input);    
        }
        
        output = _worker.PeekOutput(_outputName);
        _actionDim = output.shape.channels;

        float[] probs = new float[_actionDim];
        for(int i=0; i < _actionDim; i++)
        {
            probs[i] = output[0, i];
        }
        probs = Softmax(probs);
        output.Dispose();

        return probs;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
