using UnityEngine;

namespace Ilumisoft.Environment
{
    public class Skydome : MonoBehaviour
    {
        private void LateUpdate()
        {
            if(Camera.main == null) 
            {
                return;
            }

            // Make the skydome move with the camera, but ignore vertical movement (otherwise it would look very strange when the player jumps or falls).
            var position = Camera.main.transform.position;

            position.y = transform.position.y;

            transform.position = position;
        }
    }
}