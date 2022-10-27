using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalBlackboard : MonoBehaviour
{
    public int Level = 0;
    public int madnessValue = 0;
    public int maxMadnessValue = 100;
    public static GlobalBlackboard instance;

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
    }

    public float GetMadnessPerc()
    {
        return (float) madnessValue / (float) maxMadnessValue;
    }

    public void IncreaseMadnessValue()
    {
        madnessValue = Math.Min(madnessValue + 1, maxMadnessValue);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}
