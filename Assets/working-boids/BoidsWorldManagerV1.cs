using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class BoidsM
{
    public string flockName = "Flock";
    public int preyCount = 100;
    public int predatorCount = 3;
    public Vector3[] boundrypoints = new Vector3[100];
    public Vector3[] antiBoundrypoints1 = new Vector3[100];
    public Matrix4x4[] preyMatrices;     
    public Matrix4x4[] predatorMatrices; 
    public NativeArray<float3> newPreyPositions;     // Next frame prey positions
    public NativeArray<float3> newPreyVelocities;    // Next frame prey velocities
    public NativeArray<float3> newPredatorPositions;  // Next frame predator positions
    public NativeArray<float3> newPredatorVelocities; // Next frame predator velocities

}

[Serializable]
public class BoidObject
{
    public GameObject boidGameObject;
    public bool isEnabled = true;
    public BoidsM boidsM = new BoidsM();
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
    
    public void IncreaseBoidsByRandomPoints(int pondIndex,int predatorCount,int preyCount)
    {
        BoidObject tempBoidObject = boidsMs[pondIndex];
        BoidsM tempBoidsM = tempBoidObject.boidsM;
        BoidsManagerV7 BMV7 = tempBoidObject.boidGameObject.GetComponent<BoidsManagerV7>();
        tempBoidsM.preyCount = BMV7.preyCount;
        tempBoidsM.predatorCount = BMV7.predatorCount;
        tempBoidsM.newPreyPositions = BMV7.newPreyPositions;
        tempBoidsM.newPreyVelocities = BMV7.newPreyVelocities;
        tempBoidsM.newPredatorPositions = BMV7.newPredatorPositions;
        tempBoidsM.newPredatorVelocities = BMV7.newPredatorVelocities;



    }
    
    public void DecreaseBoidsByRandomPoints(int pondIndex,int predatorCount,int preyCount)
    {
        Debug.Log("editor");
    }
}