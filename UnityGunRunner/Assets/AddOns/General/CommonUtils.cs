using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class CommonUtils
{
	public static string editorWorkingDirectory = "";
	public static string ReadTextFile(string sFilePathAndName)
	{
		string filePathName = FilePath.NormalizeSlashes(sFilePathAndName);
#if UNITY_WEBPLAYER
				//Debug.LogWarning("Can not access files in Web Builds.");
			   //try loading resources file
				return ReadResourcesTextFile(sFilePathAndName);
#endif
//#if !UNITY_EDITOR
		//If running in the editor, allow ability to alter the assumed base directory.
		if (Application.isEditor && editorWorkingDirectory.Length > 1)
		{
			//simple check to ignore paths if a drive is specified, ie 'C:/'
			if (!filePathName.Contains(":"))
			{
				filePathName = FilePath.NormalizePathString(editorWorkingDirectory + "/" + filePathName);
				Debug.Log("Editor read text file: '" + sFilePathAndName + "' reading '" + filePathName + "'.");
			}
		}
//#endif

		//Check to see if the filename specified exists, if not try adding '.txt', otherwise fail
		string sFileNameFound = "";
		if (File.Exists(filePathName))
		{
			//Debug.Log("Reading '" + sFileName + "'.");
			sFileNameFound = filePathName; //file found
		}
		else if (File.Exists(filePathName + ".txt"))
		{
			sFileNameFound = filePathName + ".txt";
		}
		else
		{
			//Debug.LogWarning("Could not find file '" + sFilePathAndName + "'.");
			return ReadResourcesTextFile(sFilePathAndName); //use original string for resources file check, ignores editorWorkingDirectory
		}

		//read file as readonly
		try
		{
			using (StreamReader sr = new StreamReader(new FileStream(sFileNameFound, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
			{
				string fileContents = sr.ReadToEnd();
				sr.Close();
				return fileContents;
			}
		}
		catch (System.Exception e)
		{
			Debug.LogWarning("Something went wrong with read.  " + e.Message);
			return null;
		}
	}

	public static string ReadResourcesTextFile(string sFilePathAndName)
	{
		//try loading it from resources
		string filenameWithoutExtension = sFilePathAndName.Replace(".txt", "");
		TextAsset fileloaded = Resources.Load(filenameWithoutExtension) as TextAsset;
		string fileContents = null;
		if (fileloaded != null)
		{
			fileContents = fileloaded.text;
			//Debug.Log("not on disk, but found in resources");
		}
		return fileContents;
	}

	public static void WriteTextFile(string sFilePathAndName, string sTextContents)
	{
		StreamWriter sw = new StreamWriter(sFilePathAndName);
		sw.WriteLine(sTextContents);
		sw.Flush();
		sw.Close();
	}

	//simply take a list and save it out
	public static void SaveListToFile(List<string> sList, string sFileName)
	{
		float fStartTime = Time.realtimeSinceStartup;
		StreamWriter sw;
		try
		{
			sw = new StreamWriter(sFileName);
		}
		catch (System.Exception e)
		{
			Debug.LogWarning("Something went wrong with save.  " + e.Message);
			return;
		}

		foreach (string sMsg in sList)
			sw.WriteLine(sMsg);

		sw.Close();
		Debug.Log("Saved String List file to " + sFileName + " in " + (Time.realtimeSinceStartup - fStartTime) + " seconds.");
	}


	public static string ListStringsToText(List<string> listStrings)
	{
		string text = "";
		foreach (string line in listStrings)
		{
			text += line + "\n";
		}

		return text;
	}


	public static string GetProjectName()
	{
		string dp = Application.dataPath;
		string[] s;
		s = dp.Split("/"[0]);
		string projname = s[s.Length - 2].Trim();
		projname.Replace(' ', '_');
		//Debug.Log("project = " + projname);

		return projname;
	}
}