using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


[Serializable]
public class BoidObject
{
    public GameObject boidGameObject;
    public bool isEnabled = true;
}

public class BoidsWorldManagerV1 : MonoBehaviour
{
    public List<BoidObject> boidsMs = new List<BoidObject>();
    
    public GameObject[] boundryObjects;
    public GameObject[] boidsPrefabs;

    private void Start()
    {
        foreach (var boundryObject in boundryObjects)
        {
            OuterPerimeterFinderV3 outerPerimeterFinderV3 = boundryObject.GetComponent<OuterPerimeterFinderV3>();
            GameObject boid = Instantiate(boidsPrefabs[0], boundryObject.transform);
            BoidsManagerV7 BMV7 = boid.GetComponent<BoidsManagerV7>();
            BMV7.polygonPoints = outerPerimeterFinderV3.loop1.ToArray();
            BMV7.antiPolygonPoints = outerPerimeterFinderV3.loop2.ToArray();
            
            BoidObject boidObject = new BoidObject { boidGameObject = boid };
            boidsMs.Add(boidObject);
            boid.transform.parent = transform;
        }
    }

    private void Update()
    {
        if (boidsMs.Count > 0)
        {
            foreach (var boidObject in boidsMs)
            {
                if (boidObject.boidGameObject != null)
                {
                    boidObject.boidGameObject.SetActive(boidObject.isEnabled);
                }
            } 
        }
      
    }
    
    public void IncreaseBoidsByRandomPoints(int pondIndex,int preyCount,int predatorCount)
    {
        BoidObject BoidObject = boidsMs[pondIndex];
        BoidsManagerV7 BMV7 = BoidObject.boidGameObject.GetComponent<BoidsManagerV7>();
        BMV7.Reset(BMV7.preyCount+preyCount,BMV7.predatorCount+predatorCount);
    }
    
    public void DecreaseBoidsByRandomPoints(int pondIndex,int preyCount ,int predatorCount)
    {
        BoidObject BoidObject = boidsMs[pondIndex];
        BoidsManagerV7 BMV7 = BoidObject.boidGameObject.GetComponent<BoidsManagerV7>();
        int newPreyCount = BMV7.preyCount - preyCount;
        int newPredatorCount = BMV7.predatorCount - predatorCount;
        if (newPreyCount > 0 && newPredatorCount > 0)
        {
            BMV7.Reset(newPreyCount,newPredatorCount);
        }
    }
}