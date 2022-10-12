using UnityEngine;

namespace UltimateSpawner.Demo
{
    /// <summary>
    /// A simple health script used in the demo scenes.
    /// </summary>
    public class SimpleDamage : MonoBehaviour
    {
        // Public
        /// <summary>
        /// The health value for the damageable item.
        /// </summary>
        public float health = 1f;

        // Methods
        /// <summary>
        /// Apply the specified amount fo damage.
        /// </summary>
        /// <param name="amount">The amount of damage to apply</param>
        public void TakeDamage(float amount)
        {
            health -= amount;

            if(health < 0)
            {
                health = 0;
                Destroy(gameObject);
            }
        }
    }
}
