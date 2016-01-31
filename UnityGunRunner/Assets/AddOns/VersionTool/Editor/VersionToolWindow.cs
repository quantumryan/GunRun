using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;

/// <summary>
/// Version Incermentation Tool Window. Provides automatic version numbering for your Unity Project while 
/// the window is open, and autoversioning is checked. Whenever Unity is compiling, the version will
/// increment. This version is saved as a Resource file, so that it may be easily accessed through other
/// methods during run time.
/// </summary>
[InitializeOnLoad]
public class VersionToolWindow : EditorWindow
{
	static bool doVersioning = false;//should the version number change automatically
	static string projectName = "TBD"; //for using editor prefs on multiple projects
	static string buildDateTime = "TBD";
	static TextAsset versionFile;
	static Version version;
	static bool versionFileConnected = false;

	bool triggerNotification = true;
	bool triggerVersioning = false;

	VersionToolWindow()
	{
		//EditorApplication.update += Update;
	}

	[MenuItem("Tools/Versioning Tool")]
	static void Init()
	{
		Debug.Log("Versioning Tool Init!");
		FillData();

		VersionToolWindow window = (VersionToolWindow)EditorWindow.GetWindowWithRect(typeof(VersionToolWindow), new Rect(0, 0, 175, 90), false, "Auto Version");
		window.Show();
	}

	[MenuItem("Tools/Versioning Tool Clear")]
	static void ClearEditorPrefKeys()
	{
		Debug.Log("Versioning Tool Clearing!");
		EditorPrefs.DeleteKey("versionAssetPathFileName" + projectName);
		EditorPrefs.DeleteKey("doVersioning" + projectName);
	}

	static void FillData()
	{
		projectName = CommonUtils.GetProjectName();
		doVersioning = EditorPrefs.GetBool("doVersioning" + projectName, false);

		string versionAssetPathFileName = EditorPrefs.GetString("versionAssetPathFileName" + projectName, "Assets/Resources/version.txt");

		LoadVersionAssetFile(versionAssetPathFileName);
	}

	static void LoadVersionAssetFile(string assetfilepathname)
	{
		versionFile = AssetDatabase.LoadAssetAtPath(assetfilepathname, typeof(TextAsset)) as TextAsset;
		AssetDatabase.SetLabels(versionFile, new string[] { "VersionFile" });

		if (versionFile != null)
		{
			versionFileConnected = true;
			if (versionFile.text.Length > 1)
			{
				version = new Version(versionFile.text.Split('\n')[0]);
			}
			else
			{
				version = new Version();
			}

			//Debug.Log("Asset file loaded: '" + assetfilepathname + "'");
			EditorPrefs.SetString("versionAssetPathFileName" + projectName, assetfilepathname);
		}
		AssetDatabase.Refresh();
	}

	void OnGUI()
	{
		int mult = 1;//for holding ctrl down to get x10
		if (Event.current.control)
		{
			//Event control key is down
			mult = 10;
			Repaint();
		}

		EditorGUILayout.BeginVertical();

		EditorGUILayout.BeginHorizontal();
		versionFile = EditorGUILayout.ObjectField(versionFile, typeof(TextAsset), true) as TextAsset;
		if (GUI.changed)
		{
			Debug.Log("GUI change detected, 'Version Text Asset File' change");
			if (versionFile != null)
			{
				string assetPath = AssetDatabase.GetAssetPath(versionFile);
				LoadVersionAssetFile(assetPath);
				SaveVersionFile(version);
			}
			else
			{
				//disconnected
				versionFileConnected = false;
				EditorPrefs.SetString("versionAssetPathFileName" + projectName, "");
			}
		}
		EditorGUILayout.EndHorizontal();
		if (versionFile != null)
		{
			EditorGUILayout.LabelField("Project: " + projectName);
			EditorGUILayout.LabelField("Last Compile: " + buildDateTime);
			//EditorGUILayout.LabelField("Unity Pro: " + Application.HasProLicense().ToString());

			doVersioning = EditorGUILayout.Toggle("Auto Versioning", doVersioning);
			if (GUI.changed)
			{
				//Debug.Log("GUI change detected, 'Do Versioning Flag' change");
				EditorPrefs.SetBool("doVersioning" + projectName, doVersioning);
			}

			if (versionFileConnected)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Increment:", GUILayout.Width(80));
				if (GUILayout.Button("Major+" + (1 * mult)))
				{
					IncrementVersion(1 * mult, 0, 0);
				}
				if (GUILayout.Button("Minor+" + (1 * mult)))
				{
					IncrementVersion(0, 1 * mult, 0);
				}
				if (GUILayout.Button("Build+" + (1 * mult)))
				{
					IncrementVersion(0, 0, 1 * mult);
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Decrement:", GUILayout.Width(80));
				if (GUILayout.Button("Major-" + (1 * mult)))
				{
					IncrementVersion(-1 * mult, 0, 0);
				}
				if (GUILayout.Button("Minor-" + (1 * mult)))
				{
					IncrementVersion(0, -1 * mult, 0);
				}
				if (GUILayout.Button("Build-" + (1 * mult)))
				{
					IncrementVersion(0, 0, -1 * mult);
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Label("Version: " + version.ToString(), EditorStyles.whiteLargeLabel);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
			}

			if (triggerNotification && version != null)
			{
				triggerNotification = false;
				ShowNotification(new GUIContent("Version\n" + version.ToString()));
			}
		}
		else
		{
			EditorGUILayout.HelpBox("Drag and Drop the text asset to use for versioning or use the options below to create a new version file or use an existing file.", MessageType.Info);

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Create A Version File"))
			{
				if (!Directory.Exists("Assets/Resources"))
				{
					Directory.CreateDirectory("Assets/Resources");
				}
				File.WriteAllText("Assets/Resources/version.txt", Application.version);
				AssetDatabase.Refresh();
				LoadVersionAssetFile("Assets/Resources/version.txt");
			}
			EditorGUILayout.EndHorizontal();


			string[] existingVersionFilesGuid = AssetDatabase.FindAssets("t:textasset l:VersionFile");
			if (existingVersionFilesGuid.Length > 0)
			{
				foreach (string guid in existingVersionFilesGuid)
				{
					string name = AssetDatabase.GUIDToAssetPath(guid);
					GUILayout.Label("Possible Version Files Found");
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("Use"))
					{
						LoadVersionAssetFile(name);
					}
					EditorGUILayout.LabelField(name.Remove(0, 7)); //remove "Assets/"
					EditorGUILayout.EndHorizontal();
				}
			}


		}

		EditorGUILayout.EndVertical();
	}

	void OnInspectorUpdate()
	{
		if (version == null || versionFile == null)
		{
			versionFileConnected = false;
		}

		if (versionFileConnected)
		{
			//Debug.Log("Updating");
			if (EditorApplication.isCompiling)
			{
				//Debug.Log("Compiling");
				triggerVersioning = true;
			}
			else if (triggerVersioning)
			{
				//the main person in charge of the project should check this flag
				buildDateTime = System.DateTime.Now.ToString(@"M/d/yyyy hh:mm:ss tt");
				if (doVersioning)
				{
					IncrementVersion(0, 0, 1);
				}
				triggerVersioning = false;
			}
		}
	}


	void IncrementVersion(int maj, int min, int build)
	{
		if (version != null)
		{
			version.IncrementVersion(maj, min, build);

			SaveVersionFile(version);

			//update the window
			Repaint();
			triggerNotification = true; //popup notification

			//store version in some of the PlayerSettings
			PlayerSettings.bundleVersion = version.ToString();
			PlayerSettings.Android.bundleVersionCode = version.ToInt();

			Debug.Log("VERSION Incremented " + Application.version);
		}
		else
		{
			Debug.LogWarning("WARNING: Version::IncrementVersion() experienced a problem when trying to increment. Null version variable.");
		}
	}


	static void SaveVersionFile(Version ver)
	{
		//add additional compile info to the version file.
		string versionInfo = version.ToString() + "\n";
		versionInfo += buildDateTime;
		versionInfo += "\n";
		versionInfo += "Project name: " + projectName + "\n";

		string assetPath = AssetDatabase.GetAssetPath(versionFile);
		//Debug.Log("Saving version file to: " + assetPath + "\n" + versionInfo);
		CommonUtils.WriteTextFile(assetPath, versionInfo);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh(); //tell unity the file changed
	}

	//When the user click in the Version Tool Window.
	void OnFocus()
	{
		//Debug.Log("VersionToolWindow::OnFocus()");
		//FillData();
	}

	void OnEnable()
	{
		//Debug.Log("VersionToolWindow::OnEnable()");
		FillData();
	}

	//When the project changes, like files or folders are added.
	void OnProjectChange()
	{
		//Debug.Log("VersionToolWindow::OnProjectChange()");
		//FillData()	
	}

	void OnLostFocus()
	{
		//Debug.Log("Lost Focus");
		EditorPrefs.SetBool("doVersioning" + projectName, doVersioning);
	}
	void OnDestroy()
	{
		//Debug.Log("Destroy");
		EditorPrefs.SetBool("doVersioning" + projectName, doVersioning);
	}
}

//TODO: make it run without the Version Tool Window showing
// like this: http://answers.unity3d.com/questions/39313/how-do-i-get-a-callback-every-frame-in-edit-mode.html