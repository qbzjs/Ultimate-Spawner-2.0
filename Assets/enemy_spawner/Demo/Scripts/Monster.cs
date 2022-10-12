using UnityEngine;

namespace UltimateSpawner.Demo
{
    /// <summary>
    /// Represents a monster in the game.
    /// </summary>
    public class Monster : MonoBehaviour
    {
        // Private
        private float lastHealth = 0f;
        private float lastGrowlTime = -float.MaxValue;

        // Public
        /// <summary>
        /// The AI targeting component for the monster.
        /// </summary>
        public SimpleTarget target;
        /// <summary>
        /// The health damage component for the monster.
        /// </summary>
        public SimpleDamage damage;
        /// <summary>
        /// The audio source for all monster sound effects.
        /// </summary>
        public AudioSource audioSource;
        /// <summary>
        /// The monster hit sound effect. Played when the monster takes damage.
        /// </summary>
        public AudioClip monsterHit;
        /// <summary>
        /// The monster growl sound effect. Played when the monster is near its target.
        /// </summary>
        public AudioClip monsterGrowl;
        /// <summary>
        /// The distance from the target when the monster will play the grow sound.
        /// </summary>
        public float growlDistance = 3f;
        /// <summary>
        /// The minimum amount of time before a monster can play a growl sound again.
        /// </summary>
        public float growlDelay = 4.5f;

        // Methods
        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Start()
        {
            lastHealth = damage.health;
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Update()
        {
            // Check for hit
            if(damage.health != lastHealth)
            {
                audioSource.pitch = Random.Range(1.3f, 1.8f);
                audioSource.PlayOneShot(monsterHit);
                lastHealth = damage.health;
            }

            // Check for near
            if(target.TargetTransform != null)
            {
                if (Time.time > (lastGrowlTime + growlDelay))
                {
                    if (Vector3.Distance(transform.position, target.TargetTransform.position) < growlDistance)
                    {
                        audioSource.pitch = Random.Range(1.8f, 2.2f);
                        audioSource.PlayOneShot(monsterGrowl);
                        lastGrowlTime = Time.time;
                    }
                }
            }
        }
    }
}
