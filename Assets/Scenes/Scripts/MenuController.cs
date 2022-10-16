using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
	public string mainMenuScene;
	public GameObject pauseMenu;
	public bool isPaused;
	public bool unlockCursor;
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
		{
			if(isPaused)
			{
				ResumeGame();
			}else
			{
				isPaused = true;
				pauseMenu.SetActive(true);
				Time.timeScale = 0f;
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}
    }
	
	public void ResumeGame()
	{
		isPaused = false;
		pauseMenu.SetActive(false);
		Time.timeScale = 1f;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
	
	public void ReturnToMain()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(mainMenuScene);
	}
}
