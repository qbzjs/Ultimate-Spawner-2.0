using UnityEngine;

namespace UltimateSpawner.Demo
{
    /// <summary>
    /// A simple rotate script used in the demo scenes.
    /// Causes the attached game object to rotate around the Y axis at the specified speed.
    /// </summary>
    public class SimpleRotate : MonoBehaviour
    {
        // Public
        /// <summary>
        /// The speed that the object will rotate.
        /// </summary>
        public float rotateSpeed = 2f;

        // Methods
        private void Update()
        {
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }
    }
}
