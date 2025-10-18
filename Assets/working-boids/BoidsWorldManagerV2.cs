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
            OuterPerimeterFinderV4 outerPerimeterFinderV4 = boundryObject.GetComponent<OuterPerimeterFinderV4>();
            GameObject boid = Instantiate(boidsPrefabs[0], boundryObject.transform);
            BoidsManagerV13 BM = boid.GetComponent<BoidsManagerV13>();
            BM.enabled = true;
            BM.polygonPoints = outerPerimeterFinderV4.GetOuterBoundaryPoints();
            
            // Create separate anti-boundary arrays
            List<Vector3[]> antiBoundaryArrays = new List<Vector3[]>();
            if (outerPerimeterFinderV4.GetAntiBoundaryPoints1().Length > 0)
                antiBoundaryArrays.Add(outerPerimeterFinderV4.GetAntiBoundaryPoints1());
            if (outerPerimeterFinderV4.GetAntiBoundaryPoints2().Length > 0)
                antiBoundaryArrays.Add(outerPerimeterFinderV4.GetAntiBoundaryPoints2());
            
            BM.antiPolygonPoints = antiBoundaryArrays.ToArray();
            
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
            SetRankCounts(0, new int[]{500, 80, 50});
            SetRankCounts(1, new int[]{500, 80, 50});
            SetRankCounts(2, new int[]{500, 80, 50});
            SetRankCounts(3, new int[]{500, 80, 50});
            SetRankCounts(4, new int[]{500, 80, 50});
            SetRankCounts(5, new int[]{500, 80, 50});
            SetRankCounts(6, new int[]{500, 80, 50});
            SetRankCounts(7, new int[]{500, 80, 50});
            Debug.Log("Set new distribution: [100, 20, 10]");
            
            // display total of all ponds
            
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
            BoidsManagerV13 BM = BoidObject.boidGameObject.GetComponent<BoidsManagerV13>();
            
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
            BoidsManagerV13 BM = BoidObject.boidGameObject.GetComponent<BoidsManagerV13>();
            
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
            BoidsManagerV13 BM = BoidObject.boidGameObject.GetComponent<BoidsManagerV13>();
            BM.Reset(newRankCounts);
        }
    }
    
    public int GetTotalBoidCount(int pondIndex)
    {
        if (pondIndex >= 0 && pondIndex < boidsMs.Count)
        {
            BoidObjectV2 BoidObject = boidsMs[pondIndex];
            BoidsManagerV13 BM = BoidObject.boidGameObject.GetComponent<BoidsManagerV13>();
            return BM.totalBoidCount;
        }
        return 0;
    }
    
    public int[] GetRankCounts(int pondIndex)
    {
        if (pondIndex >= 0 && pondIndex < boidsMs.Count)
        {
            BoidObjectV2 BoidObject = boidsMs[pondIndex];
            BoidsManagerV13 BM = BoidObject.boidGameObject.GetComponent<BoidsManagerV13>();
            return BM.rankCounts;
        }
        return new int[0];
    }
}