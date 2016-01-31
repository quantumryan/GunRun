using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class VersionGUI : MonoBehaviour
{
	public Text verText;
	
	void OnEnable()
	{
		verText.text = "Version: " + Application.version;
	}


	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}
}
