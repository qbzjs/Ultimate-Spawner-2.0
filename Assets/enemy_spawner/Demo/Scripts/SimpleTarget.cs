using UnityEngine;
using UnityEngine.AI;

namespace UltimateSpawner.Demo
{
    /// <summary>
    /// A basic AI script used to move monsters towards the player in demo scenes.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public class SimpleTarget : MonoBehaviour
    {
        // Private
        private NavMeshAgent agent = null;
        private Transform targetTransform = null;

        // Public
        /// <summary>
        /// The name of the game object to target.
        /// </summary>
        public string targetObjectName = "FPSPlayer(Clone)";

        // Properties
        /// <summary>
        /// The tranform of the target game object.
        /// </summary>
        public Transform TargetTransform
        {
            get { return targetTransform; }
        }

        // Methods
        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Start()
        {
            // Try to find the ai component
            this.agent = GetComponent<NavMeshAgent>();
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Update()
        {
            FindTarget();

            // Check for missing components
            if (agent == null || targetTransform == null)
                return;

            // Set target
            agent.SetDestination(targetTransform.position);
        }

        /// <summary>
        /// Try to find the target object in the scene if it exists.
        /// </summary>
        public void FindTarget()
        {
            if(targetTransform == null)
            {
                // Try to find object in scene
                GameObject target = GameObject.Find(targetObjectName);

                // Store the target transform
                if (target != null)
                    this.targetTransform = target.transform;
            }
        }
    }
}