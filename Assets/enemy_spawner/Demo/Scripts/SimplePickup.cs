using System.Collections;
using UnityEngine;

namespace UltimateSpawner.Demo
{
    /// <summary>
    /// Used by the demo scene to represent a pickup item that can be collected by a player.
    /// </summary>
    public class SimplePickup : MonoBehaviour
    {
        // Types
        /// <summary>
        /// The type of pickup.
        /// </summary>
        public enum PickupType
        {
            /// <summary>
            /// The pickup will cause all alive enemies to be killed.
            /// </summary>
            Death,
            /// <summary>
            /// The pickup will cause the player to receive a maximum ammo award.
            /// </summary>
            Ammo,
        }

        // Private
        private Camera activeCam = null;
        private bool hasTriggered = false;

        // Public
        /// <summary>
        /// The tranform object that should pivot to face the active camera.
        /// </summary>
        public Transform faceCameraRoot;
        /// <summary>
        /// The light effect for the pickup.
        /// </summary>
        public Light lightEffect;
        /// <summary>
        /// The sound effect for the pickup.
        /// </summary>
        public AudioClip pickupSound;
        /// <summary>
        /// The type of pickup.
        /// </summary>
        public PickupType type = PickupType.Death;
        /// <summary>
        /// Require this tag to be assigned to the colliding object in order for the pickup to be activated.
        /// </summary>
        [TagCollection]
        public string triggerTag = "Player";
        /// <summary>
        /// The amount of time in seconds that the pickup will exist in the scene.
        /// </summary>
        public float lifeTime = 15;

        // Methods
        private void Start()
        {
            activeCam = Component.FindObjectOfType<Camera>();

            // Destroy after time
            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            Vector3 camPos = new Vector3(activeCam.transform.position.x, transform.position.y, activeCam.transform.position.z);

            Vector3 direction = (transform.position - camPos).normalized;

            // Look at camera
            transform.rotation = Quaternion.LookRotation(direction);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered == false && other.gameObject.CompareTag(triggerTag) == true)
            {
                if (type == PickupType.Death)
                {
                    // Play the pickup sound
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);

                    // Kill all items
                    foreach(SimpleDamage damage in Component.FindObjectsOfType<SimpleDamage>())
                        damage.TakeDamage(100f);
                }
                else if(type == PickupType.Ammo)
                {
                    // Play the pickup sound
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);

                    // Full reload
                    if (other.GetComponent<SimpleShoot>() != null)
                        other.GetComponent<SimpleShoot>().AwardMaxAmmo();
                }
                hasTriggered = true;

                // Start the destroy routine
                StartCoroutine(PickupDestroyRoutine());
            }
        }

        private IEnumerator PickupDestroyRoutine()
        {
            while(lightEffect.intensity < 10)
            {
                lightEffect.intensity += 0.3f;
                yield return null;
            }

            // Destroy the object
            Destroy(gameObject);
        }
    }
}
