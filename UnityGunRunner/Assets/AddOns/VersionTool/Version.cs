using UnityEngine;
using System.Collections;

public class Version
{
	//Google versions their apps like this: major10000 + minor100 + build number
	//format 'major'.'minor'.'build'.'status'
	protected int versionMajor = 0; //'0', but is not limited.  should probably be 0 or 1.
	protected int versionMinor = 0; //'00', 0 to 99
	int versionBuild = 0; //'00', 0 to 99
	string versionStatus = "a"; //'a', expect alpha 'a', beta 'b', release candidate 'rc' or release 'r'

	public Version(string versionInfo = "0.00.01.a")
	{
		//get just the first line whatever the text is, such as a version.txt file
		string versionText = versionInfo.Split('\n')[0].Trim(); //get version from text asset, first line only
		//Debug.Log("Version Text: '" + versionText + "'");

		string[] lines = versionText.Split('.');
		int.TryParse(lines[0], out versionMajor);
		int.TryParse(lines[1], out versionMinor);
		int.TryParse(lines[2], out versionBuild);
		if (lines.Length > 3)
			versionStatus = lines[3];
	}


	public static string GetVersionFromResourceFile(string resourceFile = "version")
	{
		string versionText = "Unknown Version";
		TextAsset versionTextAsset = Resources.Load(resourceFile) as TextAsset;
		if (versionTextAsset != null)
		{
			versionText = versionTextAsset.text.Split('\n')[0].Trim(); //get version from text asset, first line only
			Debug.Log("Version::GetVersionFromResourceFile() -- Version Text: '" + versionText + "'");
		}
		return versionText;
	}


	public static string GetBuildDateTimeFromResourceFile(string resourceFile = "version")
	{
		string buildDateText = "Unknown Build Date";
		TextAsset versionTextAsset = Resources.Load(resourceFile) as TextAsset;
		if (versionTextAsset != null)
		{
			buildDateText = versionTextAsset.text.Split('\n')[1].Trim(); //get build date time line from text asset, second line only
			Debug.Log("Version::GetBuildDateTimeFromResourceFile() -- Version Text: '" + buildDateText + "'");
		}
		return buildDateText;
	}



	public void IncrementVersion(int maj, int min, int build)
	{
		versionMajor += maj;
		versionMinor += min;
		versionBuild += build;

		while (versionBuild >= 100)
		{
			versionMinor += 1;
			versionBuild -= 100;
		}
		while (versionMinor >= 100)
		{
			versionMajor += 1;
			versionMinor -= 100;
		}


		while (versionBuild < 0)
		{
			versionMinor -= 1;
			versionBuild += 100;
		}
		while (versionMinor < 0)
		{
			versionMajor -= 1;
			versionMinor += 100;
		}
	}

	//TODO, add operators
	//public static bool operator >(Version v1, Version v2)
	//{
	//	if(v1.versionMajor > v2.
	//}



	// override for string
	public override string ToString()
	{
		return	versionMajor.ToString("0") + "." +
					versionMinor.ToString("00") + "." +
					versionBuild.ToString("00") + "." +
					versionStatus;
	}

	public int ToInt()
	{
		//Google versions their apps like this: major10000 + minor100 + build number
		//format 'major'.'minor'.'build'.'status'
		return versionMajor * 10000 + versionMinor * 100 + versionBuild;
	}

	/// <summary>
	/// Given a version number string separated by a character (default is a period, '.')
	/// and get back the integer equivalent value. Assumes 100 scale. For example:
	///  version 1.23.98 will return 12398 
	/// Any other characters will be stripped. For example:
	///  version 0.03.26.alpha will return 326, the .alpha is ignored
	/// </summary>
	/// <param name="version">integer of supplied string</param>
	/// <returns></returns>
	public static int GetBundleNumber(string version, char splitchar = '.' )
	{
		string[] lines = version.Split(splitchar);
		int countperiod = lines.Length - 1;
		int[] v = new int[countperiod];
		int validNums = 0;
		for(int i = 0; i < countperiod; i++)
		{
			if (int.TryParse(lines[i], out v[validNums]))
			{
				//Debug.Log("Got number v[" + validNums + "] =" + v[validNums]);
				validNums++;
			}
		}

		int verint = 0;
		for(int i = 0; i < validNums; i++)
		{
			//Debug.Log("Calc Number v[" + i + "] = " + v[i] + " which is X by " + Mathf.Pow(100, (validNums - 1 - i)));
			verint +=  v[i] * (int)Mathf.Pow(100, (validNums - 1 - i));
		}

		return verint;
	}
}
