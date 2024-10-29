//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.Assertions;

namespace UW.Editor {
    internal class E_CozyBuildPreprocess : IPreprocessBuildWithReport {
        private const string ROOT = "Packages/com.distantlands.cozy.core/Content/Art/Textures/Editor";

        public int callbackOrder => 0;



        public void OnPreprocessBuild(BuildReport _) {
            string[] assets = AssetDatabase.FindAssets("t:Texture2D", new[] { ROOT });
            Assert.IsTrue(assets.Length > 0);

            foreach (string asset in assets) {
                string path = AssetDatabase.GUIDToAssetPath(asset);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                Assert.IsNotNull(importer);
                importer.SaveAndReimport();
            }
        }
    }
}