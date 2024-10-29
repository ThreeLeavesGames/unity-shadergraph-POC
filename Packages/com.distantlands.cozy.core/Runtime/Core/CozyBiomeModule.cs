//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DistantLands.Cozy.Data;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DistantLands.Cozy
{
    public interface ICozyBiomeModule
    {

        public abstract void AddBiome();
        public abstract void RemoveBiome();
        public abstract void UpdateBiomeModule();
        public abstract bool CheckBiome();
        public abstract void ComputeBiomeWeights();
        public bool isBiomeModule { get; set; }

    }

#if UNITY_EDITOR
    public interface E_BiomeModule
    {

        public abstract void DrawBiomeReports();

        public abstract void DrawInlineBiomeUI();

    }
#endif
}