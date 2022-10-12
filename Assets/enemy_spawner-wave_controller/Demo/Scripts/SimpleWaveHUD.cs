using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateSpawner.Demo
{
    public class SimpleWaveHUD : MonoBehaviour
    {
        // Private
        private WaveSpawnController waveController = null;

        // Public
        public Text waveText;
        public Text nextWaveText;

        // Methods
        public void Start()
        {
            waveController = Component.FindObjectOfType<WaveSpawnController>();

            if (waveController != null)
            {
                waveController.OnWaveStarted.AddListener(OnWaveStarted);
            }
        }

        public void Update()
        {
            if (waveController != null)
            {
                if (waveController.CurrentWave < 1)
                {
                    waveText.enabled = false;
                }
                else
                {
                    waveText.enabled = true;
                    waveText.text = string.Format("Wave {0}", waveController.CurrentWave);
                }
            }
        }

        private void OnWaveStarted()
        {
            if (waveController.CurrentWave > 1)
                StartCoroutine(ShowNextWaveHint());
        }

        private IEnumerator ShowNextWaveHint()
        {
            nextWaveText.color = Color.white;
            nextWaveText.enabled = true;

            yield return new WaitForSeconds(2f);

            WaitForSeconds wait = new WaitForSeconds(0.1f);

            Color temp = nextWaveText.color;

            while(temp.a > 0)
            {
                temp.a -= 0.05f;
                nextWaveText.color = temp;

                yield return wait;
            }
        }

    }
}
