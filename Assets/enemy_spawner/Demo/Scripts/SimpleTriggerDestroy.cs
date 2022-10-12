using UnityEngine;

namespace UltimateSpawner.Demo
{
    /// <summary>
    /// A demo script used in the trigger exmple scene.
    /// </summary>
    public class SimpleTriggerDestroy : MonoBehaviour
    {
        // Methods
        private void OnTriggerEnter(Collider other)
        {
            // Destroy colliding object
            Destroy(other.gameObject);
        }
    }
}
