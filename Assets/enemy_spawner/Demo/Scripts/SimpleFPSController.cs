using UnityEngine;

namespace UltimateSpawner.Demo
{
    /// <summary>
    /// A simple FPS player controller.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class SimpleFPSController : MonoBehaviour
    {
        // Private
        private CharacterController controller = null;
        private Vector3 moveDirection = Vector3.zero;
        private Vector3 contactPoint = Vector3.zero;
        private bool isGrounded = false;
        private bool isFalling = false;
        private float rayDistance = 0;
        private int jumpTimer = 0;

        // Public
        /// <summary>
        /// The walk speed for the controller.
        /// </summary>
        public float walkSpeed = 6;
        /// <summary>
        /// The jump height of the controller.
        /// </summary>
        public float jumpHeight = 8;
        /// <summary>
        /// The amount of gravity to apply to the controller.
        /// </summary>
        public float gravity = 20;

        // Methods
        private void Start()
        {
            // Get the controller
            controller = GetComponent<CharacterController>();

            // Calcualte the ray distance
            rayDistance = controller.height * 0.5f + controller.radius;
        }

        private void FixedUpdate()
        {
            // Get the input
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");

            // Limit diagonal speed
            float modify = (x != 0 && y != 0) ? 0.707f : 1;

            // Check for grounded
            if (isGrounded == true)
            {
                bool isSliding = false;

                RaycastHit hit;

                // Check for sliding surface
                if (Physics.Raycast(transform.position, Vector3.down, out hit, rayDistance) == true)
                {
                    // Check if we can slide
                    if (Vector3.Angle(hit.normal, Vector3.up) > controller.slopeLimit - 0.1f)
                    {
                        // Set the sliding flag
                        isSliding = true;
                    }
                }
                else
                {
                    // Raycast at contact point to catch steep inclines
                    Physics.Raycast(contactPoint + Vector3.up, Vector3.down, out hit);

                    // Check if we can slide
                    if (Vector3.Angle(hit.normal, Vector3.up) > controller.slopeLimit - 0.1f)
                    {
                        // Set the sliding flag
                        isSliding = true;
                    }
                }

                // Check if we are currently falling
                if (isFalling == true)
                {
                    // Disbale the flag since we are grounded
                    isFalling = false;
                }

                // Check if we are sliding
                if (isSliding == true)
                {
                    // Get the slide normal
                    Vector3 normal = hit.normal;

                    // Update the move direction
                    moveDirection = new Vector3(normal.x, -normal.y, normal.z);

                    // Normalize the direction
                    Vector3.OrthoNormalize(ref normal, ref moveDirection);

                    // Apply the speed
                    moveDirection *= walkSpeed;
                }
                else
                {
                    // Apply movement based on input
                    moveDirection = new Vector3(x * modify, -0.75f, y * modify);
                    moveDirection = transform.TransformDirection(moveDirection) * walkSpeed;
                }

                if (Input.GetButton("Jump") == true)
                {
                    // Incrmement the jump counter
                    jumpTimer++;
                }
                else if (jumpTimer >= 1)
                {
                    // Trigger a jump
                    Jump(jumpHeight);
                }
            }
            else
            {
                // Check if we are not falling
                if (isFalling == false)
                {
                    // We should be falling
                    isFalling = true;
                }
            }

            // Apply gravity
            moveDirection.y -= gravity * Time.deltaTime;

            // Apply the movement to the controller
            CollisionFlags flags = controller.Move(moveDirection * Time.deltaTime);

            // Check for grounded
            isGrounded = (flags & CollisionFlags.Below) != 0;
        }

        /// <summary>
        /// Jump to the specified height.
        /// </summary>
        /// <param name="height">The value to determine how high the jump will be</param>
        public void Jump(float height)
        {
            // Make sure we are on the ground
            if (isGrounded == true)
            {
                // Apply the jump
                moveDirection.y = height;
                jumpTimer = 0;
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // Store the collider point
            contactPoint = hit.point;
        }
    }
}
