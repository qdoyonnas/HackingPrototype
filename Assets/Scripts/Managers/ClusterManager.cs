using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public static class ClusterManager
{
	public static string dirPath = "Assets/Resources/Clusters/";
	public static string ext = ".kscript";

	public static string SaveProgram( string file, string[] data )
	{
		try{
			File.WriteAllLines(dirPath + file + ext, data);
		}
		catch ( Exception e ) {
			Debug.LogError("StreamWrite Exception: " + e);
			return "Program write failed";
		}
		
		return null;
	}

	public static string LoadProgram( string file, out string[] instructions )
	{
		instructions = null;
		string[] buffer = new string[GameManager.gameOptions.nodeMemoryLength];

		try {
			instructions = File.ReadAllLines(dirPath + file + ext);
		}
		catch ( FileNotFoundException e ) {
			return "'" + file + "' program not found";
		}
		catch ( Exception e ) {
			Debug.LogError("StreamRead Exception: " + e);
			return "Program read failed";
		}

		return null;
	}
}