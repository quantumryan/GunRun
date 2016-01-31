using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

public class FilePath
{
	/// <summary>
	/// Converts an absolute path to project relative path.
	/// TODO: this needs to be checked.
	/// </summary>
	/// <returns>The to project relative.</returns>
	/// <param name="path">Path.</param>
	public static string AbsoluteToProjectRelative(string path)
	{
		return "Assets" + path.Replace(GetProjectAbsolutePath(), "");
	}

	/// <summary>
	/// With respect to Unity's Assets folder, this will return the project absolute path.
	/// This is an example of Unity's path format 'C:/svn/trunk/Vis/Unity/Project' on a PC.
	/// </summary>
	/// <returns>The project absolute path. This should be the full path to the executable.</returns>
	public static string GetProjectAbsolutePath()
	{
		string path = Application.dataPath;
		//TODO: see what Application.persistentDataPath is for Android builds
		return Application.dataPath.Remove(path.LastIndexOf('/')); //remove the '/Assets'
	}
	
	/// <summary>
	/// Strips the name of the file. Not sure what this does or why you would want it.
	/// TODO: this needs to be checked.
	/// </summary>
	/// <returns>The file name.</returns>
	/// <param name="path">Path.</param>
	public static string StripFileName(string path)
	{
		Match match = Regex.Match(path, "(^.*/)(.*)[.](.*$)");

		if (!match.Success)
		{
			return path;
		}

		return match.Groups[1].Value;
	}

	/// <summary>
	/// Gets the folder path, discarding the top folder or filename.
	/// i.e. 'folder/subfolder/dir' will return the path for the directory 'dir', 'folder/subfolder'
	/// i.e. 'folder/subfolder/file.txt' will return the path for the file 'file.txt', 'folder/subfolder'
	/// </summary>
	/// <returns>The folder path.</returns>
	/// <param name="path">Path.</param>
	public static string GetFolderPath(string path)
	{
		string tmppath = NormalizeSlashes(path); //convert to common format line
		int lastIndexSlash = tmppath.LastIndexOf('/');
		int startIndex = tmppath.StartsWith("/") == true ? 1 : 0;

		if (lastIndexSlash == -1)
		{
			return tmppath;
		}

		string returnpath = tmppath.Substring(startIndex, lastIndexSlash - startIndex + 1);
		return returnpath;
	}

	/// <summary>
	/// Gets the name of the file from a pathfilename formated string.
	/// i.e. 'folder/subfolder/file.txt' will return 'file.txt'
	/// </summary>
	/// <returns>The file name.</returns>
	/// <param name="path">Path.</param>
	public static string GetFileName(string path)
	{
		string tmppath = NormalizeSlashes(path); //convert to common format line
		int lastIndexSlash = tmppath.LastIndexOf('/');

		if (lastIndexSlash == -1)
		{
			return tmppath;
		}

		return tmppath.Substring(lastIndexSlash + 1);
	}

	/// <summary>
	/// Normalizes the slashes is a supplied path or filename or path/filename string to forward slashes, '/'.
	/// </summary>
	/// <returns>A modification of the original string with '/' slashes.</returns>
	/// <param name="filestring">Filestring.</param>
	public static string NormalizeSlashes(string filestring)
	{
		string normal = filestring.Replace('\\', '/'); //in case of "wrong" slashes, try to swap them first		
		normal = normal.Replace("//", "/"); //in case of // from combininng strings of paths and names
		normal = normal.Trim();

		return normal;
	}


	/// <summary>
	/// Normalizes the slashes is a supplied path or filename or path/filename string to forward backslashes, '\'.
	/// </summary>
	/// <returns>A modification of the original string with '\' backslashes.</returns>
	/// <param name="filestring">Filestring.</param>
	public static string NormalizeBackSlashes(string filestring)
	{
		string normal = filestring.Replace('/', '\\'); //in case of "wrong" slashes, try to swap them first		
		normal = normal.Replace("\\\\", "\\"); //in case of \\ from combininng strings of paths and names
		normal = normal.Trim();

		return normal;
	}


	/// <summary>
	/// Normalizes the path string into a commonly usuable form. Removes preceding  and ending
	///  slashes, replaces \ slashes with / slashes.
	/// i.e. '\temp\filefolder\directory1\' will return 'temp/filefolder/directory1', which
	/// will work with all other FilePath (and similar to System.IO) file naming conventions.
	/// Such that strings work together well, like pathfilename = path + filename;
	/// </summary>
	/// <returns>A modification of the original string, but in a common form.</returns>
	/// <param name="path">Path.</param>
	public static string NormalizePathString(string path)
	{
		string normal = NormalizeSlashes(path);

		if (normal.StartsWith("/"))
		{
			normal.Remove(0, 1); //remove leading /
		}

		if (normal.StartsWith("."))
		{
			Debug.Log("TODO: deal with . and ../ directories");
		}

		if (normal.EndsWith("/"))
		{
			normal = normal.Remove(normal.LastIndexOf("/"));
		}

		return normal;
	}
	
	/// <summary>
	/// Normalizes the path string into a commonly usuable form. Removes preceding  and ending
	///  slashes, replaces \ slashes with / slashes.
	/// </summary>
	/// <returns>A modification of the original string, but in a common form.</returns>
	/// <param name="path">Path.</param>
	public static string ValidatePathFilenameString(string path)
	{
		//atm, the NormalizePathString() function will do the same checks
		string normal = NormalizePathString(path);

		return normal;
	}

	/// <summary>
	/// Strips the file extension from the supplied string. Also normalizes the string.
	/// i.e. 'folder\file.txt' becomes 'folder/file'
	/// </summary>
	/// <returns>A modification of the original string, but without the filename extension.</returns>
	/// <param name="file">File.</param>
	public static string StripFileExtension(string file)
	{
		string tmpfile = NormalizePathString(file);
		int lastIndexPeriod = tmpfile.LastIndexOf('.');

		if (lastIndexPeriod == -1)
		{
			return tmpfile;
		}

		return tmpfile.Substring(0, lastIndexPeriod);
	}
}