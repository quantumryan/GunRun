using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QuitMenu : MonoBehaviour 
{
	public Canvas quitMenu;

	// Use this for initialization
	void Start () 
	{
		quitMenu.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			quitMenu.gameObject.SetActive(!quitMenu.gameObject.activeSelf);
		}
	}

	public void ShowMenu(bool show)
	{
		quitMenu.gameObject.SetActive(show);
	}


	public void Quit()
	{
		Application.Quit();
	}
}
