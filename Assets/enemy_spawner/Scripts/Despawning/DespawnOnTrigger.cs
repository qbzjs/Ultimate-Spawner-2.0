using System;
using UltimateSpawner.Spawning;
using UnityEngine;

namespace UltimateSpawner.Despawning
{
    public class DespawnOnTrigger : Despawner
    {
        public enum TriggerEvent
        {
            Enter,
            Exit,
        }

        // Public
        public bool is2DSpawner = false;

        public TriggerEvent triggerEvent = TriggerEvent.Enter;

        public LayerMask triggerLayer = 0;

        [TagCollection]
        public string[] triggerTags = null;

        public DespawnTarget despawnTarget = DespawnTarget.ThisObject;

        // Methods
        public void OnTriggerEnter(Collider other)
        {
            if (is2DSpawner == false && triggerEvent == TriggerEvent.Enter)
                OnTriggerEvent(other.gameObject);
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (is2DSpawner == true && triggerEvent == TriggerEvent.Enter)
                OnTriggerEvent(other.gameObject);
        }

        public void OnTriggerExit(Collider other)
        {
            if(is2DSpawner == false && triggerEvent == TriggerEvent.Exit)
                OnTriggerEvent(other.gameObject);
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            if (is2DSpawner == true && triggerEvent == TriggerEvent.Exit)
                OnTriggerEvent(other.gameObject);
        }

        public void OnTriggerEvent(GameObject other)
        {
            // Check if despawn is allowed
            if (ShouldAllowDespawn == false)
                return;

            if (other.layer ==triggerLayer) //(other.gameObject.layer & triggerLayer.value) != 0)
            {
                if (UltimateSpawning.IsTagged(other, triggerTags) == true)
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
            DespawnOnTrigger despawner = cloneFrom as DespawnOnTrigger;

            if(despawner != null)
            {
                triggerEvent = despawner.triggerEvent;
                triggerLayer = despawner.triggerLayer;

                if (despawner.triggerTags != null)
                    triggerTags = (string[])despawner.triggerTags.Clone();

                despawnTarget = despawner.despawnTarget;
            }
        }
    }
}
