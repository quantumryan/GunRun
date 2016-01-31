using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SimpleScreenShotByKey : MonoBehaviour
{
	public KeyCode captureKey = KeyCode.F10;


	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(captureKey))
		{
			if (InputUtils.GetKeyControl())
			{
				StartCoroutine (SaveScreenShot("", 4));
			}
			else
			{
				StartCoroutine (SaveScreenShot());
			}
		}
	}


	public static string SaveSingleScreenShot(string sFilename = "", int superSize = 1)
	{
		if (sFilename == "")
		{
			// use timestamp
			sFilename = string.Format("screenshot-{0:yyyy-MM-dd_hh-mm-ss-tt}.png", System.DateTime.Now);
		}

		string executableFullPath = FilePath.GetProjectAbsolutePath();
		string fullPathFileName = executableFullPath + "/" + FilePath.NormalizePathString(sFilename);

		//Debug.Log("Saving Screenshot: '" + fullPathFileName + "'.");
		Application.CaptureScreenshot(fullPathFileName, superSize); // Capture the screenshot
		return fullPathFileName;
	}
	


	public IEnumerator SaveScreenShot(string sFilename = "", int superSize = 1)
	{
		Canvas[] canvi = FindObjectsOfType<Canvas>();
		foreach(Canvas c in canvi)
		{
			c.enabled = false;
		}


		yield return new WaitForEndOfFrame();  //waits for all other stuff to happen

		SaveSingleScreenShot(sFilename, superSize);

		foreach (Canvas c in canvi)
		{
			c.enabled = true;
		}
	}


}

