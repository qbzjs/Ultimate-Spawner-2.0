using UnityEngine;
using UnityEditor;

namespace UltimateSpawner.Editor
{ 
    public class EditorMenu_Waves : EditorMenu
    {
        // Methods
        [MenuItem(gameObjectMenuPath + "Wave Controller", false, 60)]
        public static WaveSpawnController CreateWaveSpawnController()
        {
            // Create the wave spawn controller
            return CreateObjectWithMainComponent<WaveSpawnController>("Wave Spawn Controller");
        }
    }
}
