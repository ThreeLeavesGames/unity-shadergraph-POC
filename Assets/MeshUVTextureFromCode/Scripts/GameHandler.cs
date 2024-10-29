using UnityEngine;
using CodeMonkey;
using CodeMonkey.Utils;

namespace CodeMonkey_MeshUVTextureFromCode {

    public class GameHandler : MonoBehaviour {

        public Material material; // Material to be applied to the mesh

        // UV coordinates for different parts
        private Vector2[] headDownUV;
        private Vector2[] headUpUV;
        private Vector2[] headLeftUV;
        private Vector2[] headRightUV;

        private Vector2[] bodyDownUV;
        private Vector2[] bodyUpUV;
        private Vector2[] bodyLeftUV;
        private Vector2[] bodyRightUV;

        private int textWidth = 256; // Width of the texture
        private int textHeight = 128; // Height of the texture
        private int partHeight = 64; // Height of each part

        private void Start() {
            // Create arrays for UV coordinates and vertices
            Vector2[] uv = new Vector2[4];

            // Assign UV coordinates for different parts from the texture
            headDownUV = GetUVRectangleFromPixels(0, 0, partHeight, partHeight, textWidth, textHeight);
            headUpUV = GetUVRectangleFromPixels(64, 0, partHeight, partHeight, textWidth, textHeight);
            headLeftUV = GetUVRectangleFromPixels(128, 0, partHeight, partHeight, textWidth, textHeight);
            headRightUV = GetUVRectangleFromPixels(192, 0, partHeight, partHeight, textWidth, textHeight);

            bodyDownUV = GetUVRectangleFromPixels(0, 64, partHeight, partHeight, textWidth, textHeight);
            bodyUpUV = GetUVRectangleFromPixels(64, 64, partHeight, partHeight, textWidth, textHeight);
            bodyLeftUV = GetUVRectangleFromPixels(128, 64, partHeight, partHeight, textWidth, textHeight);
            bodyRightUV = GetUVRectangleFromPixels(192, 64, partHeight, partHeight, textWidth, textHeight);


            // Assume this script is attached to a GameObject with a MeshFilter and MeshRenderer
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

            // Apply initial UV coordinates to the mesh
            ApplyUVToUVArray(headLeftUV, ref uv);
            meshFilter.mesh.uv = uv;
            meshRenderer.material= material;

            // Create UI buttons to change the UV mapping
            // CMDebug.ButtonUI(new Vector2(-500, 310), "Head Down",   () => { ApplyUVToUVArray(headDownUV,    ref uv); meshFilter.mesh.uv = uv; });
            // CMDebug.ButtonUI(new Vector2(-500, 250), "Head Up",     () => { ApplyUVToUVArray(headUpUV,      ref uv); meshFilter.mesh.uv = uv; });
            // CMDebug.ButtonUI(new Vector2(-500, 190), "Head Left",   () => { ApplyUVToUVArray(headLeftUV,    ref uv); meshFilter.mesh.uv = uv; });
            // CMDebug.ButtonUI(new Vector2(-500, 130), "Head Right",  () => { ApplyUVToUVArray(headRightUV,   ref uv); meshFilter.mesh.uv = uv; });

            // CMDebug.ButtonUI(new Vector2(-350, 310), "Body Down",   () => { ApplyUVToUVArray(bodyDownUV,    ref uv); meshFilter.mesh.uv = uv; });
            // CMDebug.ButtonUI(new Vector2(-350, 250), "Body Up",     () => { ApplyUVToUVArray(bodyUpUV,      ref uv); meshFilter.mesh.uv = uv; });
            // CMDebug.ButtonUI(new Vector2(-350, 190), "Body Left",   () => { ApplyUVToUVArray(bodyLeftUV,    ref uv); meshFilter.mesh.uv = uv; });
            // CMDebug.ButtonUI(new Vector2(-350, 130), "Body Right",  () => { ApplyUVToUVArray(bodyRightUV,   ref uv); meshFilter.mesh.uv = uv; });

        }

        // Converts pixel coordinates to UV coordinates
        private Vector2 ConvertPixelsToUVCoordinates(int x, int y, int textureWidth, int textureHeight) {
            return new Vector2((float)x / textureWidth, (float)y / textureHeight);
        }

        // Gets a UV rectangle based on pixel coordinates
        private Vector2[] GetUVRectangleFromPixels(int x, int y, int width, int height, int textureWidth, int textureHeight) {
             /* 
     * Original UV Mapping:
     * 0, 1 (Top Left)
     * 1, 1 (Top Right)
     * 0, 0 (Bottom Left)
     * 1, 0 (Bottom Right)
     * 
     * Adjusted UV Mapping for Y-Inversion:
     * 0, 0 (Bottom Left)
     * 1, 0 (Bottom Right)
     * 0, 1 (Top Left)
     * 1, 1 (Top Right)
     */
            return new Vector2[] { 
           
                ConvertPixelsToUVCoordinates(x, y, textureWidth, textureHeight), // Bottom left
                ConvertPixelsToUVCoordinates(x + width, y, textureWidth, textureHeight), // Bottom right
                     ConvertPixelsToUVCoordinates(x, y + height, textureWidth, textureHeight), // Top left
                ConvertPixelsToUVCoordinates(x + width, y + height, textureWidth, textureHeight), // Top right
            };
        }

        // Applies UV coordinates to the main UV array
        private void ApplyUVToUVArray(Vector2[] uv, ref Vector2[] mainUV) {
            if (uv == null || uv.Length < 4 || mainUV == null || mainUV.Length < 4) throw new System.Exception("Invalid UV data");
            mainUV[0] = uv[0];
            mainUV[1] = uv[1];
            mainUV[2] = uv[2];
            mainUV[3] = uv[3];
        }
    }
}
