using System;
using UnityEngine;

namespace UltimateSpawner.Despawning
{
    public class DespawnOnCollision : Despawner
    {
        // Public
        public bool is2DDespawner = false;

        public LayerMask collisionLayer = 0;

        [TagCollection]
        public string[] collisionTags = null;

        public DespawnTarget despawnTarget = DespawnTarget.ThisObject;

        // Methods
        public void Reset()
        {
            is2DDespawner = UltimateSpawning.IsEditorIn2DMode();
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (is2DDespawner == false)
                OnCollisionEvent(collision.gameObject);
        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
            if (is2DDespawner == true)
                OnCollisionEvent(collision.gameObject);
        }

        public void OnCollisionEvent(GameObject other)
        {
            // Check if despawn is allowed
            if (ShouldAllowDespawn == false)
                return;

            if (other.layer == collisionLayer)
            {
                if (UltimateSpawning.IsTagged(other, collisionTags) == true)
                {
                    // Set despawn condition
                    MarkDespawnConditionAsMet();

                    if (despawnTarget == DespawnTarget.ThisObject)
                    {
                        Despawn();
                    }
                    else if (despawnTarget == DespawnTarget.OtherObject)
                    {
                        UltimateSpawning.Despawn(other);
                    }
                }
            }
        }

        public override void CloneFrom(Despawner cloneFrom)
        {
            DespawnOnCollision despawner = cloneFrom as DespawnOnCollision;

            if(despawner != null)
            {
                collisionLayer = despawner.collisionLayer;

                if (despawner.collisionTags != null)
                    collisionTags = (string[])despawner.collisionTags.Clone();

                despawnTarget = despawner.despawnTarget;
            }
        }
    }
}
