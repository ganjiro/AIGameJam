using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
  
    }

    public IEnumerator moveAgents()
    {
        VictimAgent[] childAgents = new VictimAgent[transform.childCount];
        EnemyMovement[] childMovements = new EnemyMovement[transform.childCount];

        for(int i = 0; i < childAgents.Length; i ++)
        {
            // child.GetComponent<EnemyMovement>().randomMovement();
            // Make the action of the NN
            // This method is asynchronous, so we call it and then we wait 
            childAgents[i] = transform.GetChild(i).GetComponent<VictimAgent>();
            childMovements[i] = transform.GetChild(i).GetComponent<EnemyMovement>();
            childAgents[i].MakeAction();
        }

        for(int i = 0; i < childAgents.Length; i ++)
        {
            // Wait for the neural network to have computed the action
            while (childAgents[i]._currentAction == childAgents[i]._defaultActionValue)
            {
                yield return null;
            }
        }
        
        for(int i = 0; i < childAgents.Length; i ++)
        {
            // Once the action is computed by the NN, it is saved in the _currentAction variable of the Agent
            // Use that action to move, and then reset it to default value
            childMovements[i].actionMovement(childAgents[i]._currentAction);
            childAgents[i]._currentAction = 99;
        }

    }
}
