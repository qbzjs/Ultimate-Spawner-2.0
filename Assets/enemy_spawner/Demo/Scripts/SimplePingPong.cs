using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateSpawner.Demo
{
    public class SimplePingPong : MonoBehaviour
    {
        // Private
        private Vector3 initialPosition = Vector3.zero;
        private float updatePosition = 0;
        private bool flip = false;

        // Public
        public float speed = 2;
        public float distance = 1f;

        // Methods
        public void Start()
        {
            initialPosition = transform.position;
        }

        public void Update()
        {
            if(flip == false)
            {
                updatePosition += speed * Time.deltaTime;

                if(updatePosition > distance)
                {
                    updatePosition = distance;
                    flip = true;
                }
            }
            else
            {
                updatePosition -= speed * Time.deltaTime;

                if(updatePosition < -distance)
                {
                    updatePosition = -distance;
                    flip = false;
                }
            }

            transform.position = initialPosition + new Vector3(updatePosition, 0, 0);
        }
    }
}
