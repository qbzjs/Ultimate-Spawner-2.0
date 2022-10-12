using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UltimateSpawner.Spawning
{
    /// <summary>
    /// The method used to determine whether or not an end point <see cref="Spawner"/> is occupied.
    /// The <see cref="Spawner"/> is occupied when another physics object is within the radius of the <see cref="Spawner"/>. 
    /// </summary>
    public enum OccupiedCheck
    {
        /// <summary>
        /// No occupied check is performed meaning that multiple objects can spawn at the same location.
        /// This is not recommended for physics objects using rigid body components. 
        /// </summary>
        None = 0,
        /// <summary>
        /// The physics system is used to perform an overlap sphere cast to determine whether any objects are withing range.
        /// </summary>
        PhysicsOverlap,
        /// <summary>
        /// The physics trigger callbacks are used to detect when a physics object enters and exits a spawn point.
        /// This method requires a collider to be attached to the spawn point.
        /// </summary>
        PhysicsTrigger,
    }

    /// <summary>
    /// Represent an end spawner that cannot have any child spawners.
    /// An end spawner is able to handle spawn requests directly and must be responsilbe for creating a spawnable item on request without delegating to child spawners.
    /// </summary>
    public abstract class EndPointSpawner : Spawner
    {
        // Internal
        internal static readonly Color availableColor = new Color(0, 1, 0, 0.2f);
        internal static readonly Color unavailableColor = new Color(1, 0, 0, 0.2f);
        internal static readonly Color highlightColor = new Color(0, 1, .6f, 0.4f);
        internal static readonly Color selectedColor = new Color(1, 0.4f, 0);

        // Private
#if UNITY_EDITOR
        private static Mesh cachedTriangleMesh = null;
        private static Material cachedTriangleMaterial = null;
#endif

        // Protected
        /// <summary>
        /// The size of the shared collider buffers used for all physics collision checks.
        /// </summary>
        protected const int sharedBufferSize = 256;
        /// <summary>
        /// The collider shared buffer used by all end spawners for physics checks.
        /// The shared buffer is used to avoid allocations for every physics cast.
        /// </summary>
        protected static Collider[] sharedColliderBuffer = new Collider[sharedBufferSize];
        /// <summary>
        /// The collider 2d shared buffer used bu all end spawners for physics checks.
        /// The shared buffer is used to avoid allocations for every physics cast.
        /// </summary>
        protected static Collider2D[] sharedCollider2DBuffer = new Collider2D[sharedBufferSize];

        /// <summary>
        /// Returns true if the Unity project is setup for 2D games or false if the project is setup for 3D games.
        /// This determines the behaviour or certain components.
        /// </summary>
        [SerializeField]
        [Tooltip("Is the spawner setup in 2D mode. When enabled, the spawner will use the appropriate 2D physics equivilent to perform necessary checks such as occupied checks")]
        protected bool is2DSpawner = false;

        // Properties
        /// <summary>
        /// Get a value idicating whether the spawner is setup as a 3D or 2D spawner.
        /// This value can be changed at runtime and will cause the spawner to rebuild its components if necessary.
        /// </summary>
        public virtual bool Is2DSpawner
        {
            get { return is2DSpawner; }
            set { is2DSpawner = value; }
        }

        // Methods
#if UNITY_EDITOR
        /// <summary>
        /// Called by Unity editor.
        /// </summary>
        public override void Reset()
        {
            // Call the base method
            base.Reset();

            // Spawners adopt the current 2d mode when created
            is2DSpawner = UltimateSpawning.IsEditorIn2DMode();
        }
#endif

        /// <summary>
        /// Enumerate all child spawners.
        /// An end point spawner cannot have any child spawners so this will return nothing.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Spawner> GetEnumerator()
        {
            // No children allowed
            yield break;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Draw a triangle gizmo in the scene.
        /// </summary>
        /// <param name="position">The position to draw the triangle at</param>
        /// <param name="rotation">The rotation to draw the triangle with</param>
        /// <param name="size">The scale to draw the triangle at</param>
        protected void DrawGizmoTriangle(Vector3 position, Quaternion rotation, Vector2 size)
        {
            // Check for cached mesh
            if (cachedTriangleMesh == null)
            {
                float offsetSize = 0.5f;

                // Create the mesh
                cachedTriangleMesh = new Mesh();

                cachedTriangleMesh.vertices = new Vector3[]
                {
                    new Vector3(-offsetSize, -offsetSize, 0),
                    new Vector3(offsetSize, -offsetSize, 0),
                    new Vector3(0, offsetSize, 0),
                };
                cachedTriangleMesh.triangles = new int[]
                {
                    0, 1, 2,
                };
                cachedTriangleMesh.normals = new Vector3[]
                {
                    Vector3.forward,
                    Vector3.forward,
                    Vector3.forward,
                };
            }
            

            // Check for cached material
            if(cachedTriangleMaterial == null)
            {
                // Create the material
                cachedTriangleMaterial = new Material(Shader.Find("Sprites/Default"));
            }

            // Update material color
            cachedTriangleMaterial.color = Gizmos.color;

            for (int i = 0; i < cachedTriangleMaterial.passCount; i++)
            {
                if (cachedTriangleMaterial.SetPass(i) == true)
                {
                    Graphics.DrawMeshNow(cachedTriangleMesh, Matrix4x4.TRS(position, rotation, new Vector3(size.x, size.y, 1f)));
                }
            }
        }
#endif
    }
}
