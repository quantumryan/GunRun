using UnityEngine;
using System.Collections;

public class TitleMenu : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void LoadLevel(string levelName)
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
	}
}
