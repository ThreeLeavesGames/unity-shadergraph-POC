using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


[Serializable]
public class BoidObjectV2
{
    public GameObject boidGameObject;
    public bool isEnabled = true;
}
public class BoidsWorldManagerV2 : MonoBehaviour
{
    public List<BoidObjectV2> boidsMs = new List<BoidObjectV2>();
    
    public GameObject[] boundryObjects;
    public GameObject[] boidsPrefabs;

    private void Start()
    {
        foreach (var boundryObject in boundryObjects)
        {
            OuterPerimeterFinderV3 outerPerimeterFinderV3 = boundryObject.GetComponent<OuterPerimeterFinderV3>();
            GameObject boid = Instantiate(boidsPrefabs[0], boundryObject.transform);
            BoidsManagerV11 BM = boid.GetComponent<BoidsManagerV11>();
            BM.polygonPoints = outerPerimeterFinderV3.loop1.ToArray();
            BM.antiPolygonPoints = outerPerimeterFinderV3.loop2.ToArray();
            
            BoidObjectV2 boidObject = new BoidObjectV2 { boidGameObject = boid };
            boidsMs.Add(boidObject);
            boid.transform.parent = transform;
        }
        
        // Demonstrate rank-based boid management after a short delay
        Invoke(nameof(DemonstrateRankSystem), 2f);
    }
    
    private void DemonstrateRankSystem()
    {
        if (boidsMs.Count > 0)
        {
            Debug.Log("=== Demonstrating Rank-Based Boid System ===");
            
            // Get current counts for pond 0
            int[] currentCounts = GetRankCounts(0);
            Debug.Log($"Initial rank counts: [{string.Join(", ", currentCounts)}]");
            Debug.Log($"Total boids: {GetTotalBoidCount(0)}");
            
            // Set new distribution: 100 rank-0, 20 rank-1, 10 rank-2
            SetRankCounts(0, new int[]{100, 20, 10});
            Debug.Log("Set new distribution: [100, 20, 10]");
            Debug.Log($"New total boids: {GetTotalBoidCount(0)}");
            
            // Increase rank 2 (highest) boids by 5 in pond 0
            IncreaseBoidsByRank(0, 2, 5);
            Debug.Log("Increased rank-2 boids by 5");
            
            // Show final counts
            int[] finalCounts = GetRankCounts(0);
            Debug.Log($"Final rank counts: [{string.Join(", ", finalCounts)}]");
            Debug.Log($"Final total boids: {GetTotalBoidCount(0)}");
            
            Debug.Log("=== Rank System Demo Complete ===");
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
    
    public void IncreaseBoidsByRank(int pondIndex, int rankIndex, int count)
    {
        if (pondIndex >= 0 && pondIndex < boidsMs.Count)
        {
            BoidObjectV2 BoidObject = boidsMs[pondIndex];
            BoidsManagerV11 BM = BoidObject.boidGameObject.GetComponent<BoidsManagerV11>();
            
            // Create new rank counts array with increased count for specified rank
            int[] newRankCounts = (int[])BM.rankCounts.Clone();
            if (rankIndex >= 0 && rankIndex < newRankCounts.Length)
            {
                newRankCounts[rankIndex] += count;
                BM.Reset(newRankCounts);
            }
        }
    }
    
    public void DecreaseBoidsByRank(int pondIndex, int rankIndex, int count)
    {
        if (pondIndex >= 0 && pondIndex < boidsMs.Count)
        {
            BoidObjectV2 BoidObject = boidsMs[pondIndex];
            BoidsManagerV11 BM = BoidObject.boidGameObject.GetComponent<BoidsManagerV11>();
            
            // Create new rank counts array with decreased count for specified rank
            int[] newRankCounts = (int[])BM.rankCounts.Clone();
            if (rankIndex >= 0 && rankIndex < newRankCounts.Length)
            {
                newRankCounts[rankIndex] = Mathf.Max(0, newRankCounts[rankIndex] - count);
                BM.Reset(newRankCounts);
            }
        }
    }
    
    public void SetRankCounts(int pondIndex, int[] newRankCounts)
    {
        if (pondIndex >= 0 && pondIndex < boidsMs.Count)
        {
            BoidObjectV2 BoidObject = boidsMs[pondIndex];
            BoidsManagerV11 BM = BoidObject.boidGameObject.GetComponent<BoidsManagerV11>();
            BM.Reset(newRankCounts);
        }
    }
    
    public int GetTotalBoidCount(int pondIndex)
    {
        if (pondIndex >= 0 && pondIndex < boidsMs.Count)
        {
            BoidObjectV2 BoidObject = boidsMs[pondIndex];
            BoidsManagerV11 BM = BoidObject.boidGameObject.GetComponent<BoidsManagerV11>();
            return BM.totalBoidCount;
        }
        return 0;
    }
    
    public int[] GetRankCounts(int pondIndex)
    {
        if (pondIndex >= 0 && pondIndex < boidsMs.Count)
        {
            BoidObjectV2 BoidObject = boidsMs[pondIndex];
            BoidsManagerV11 BM = BoidObject.boidGameObject.GetComponent<BoidsManagerV11>();
            return BM.rankCounts;
        }
        return new int[0];
    }
}