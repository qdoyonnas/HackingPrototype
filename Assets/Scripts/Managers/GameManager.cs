using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameManager
{
	public static GameOptions gameOptions;
	static string _addressFormat;
	public static string GetAddressFormat()
	{
		return _addressFormat;
	}
	
	public static double playerConsole;
	public static double adminConsole;
	static double _activeConsole;
	public static double activeConsole {
		get {
			return _activeConsole;
		}
		set {
			consoles[activeConsole].ShowRender(false);
			_activeConsole = value;
			consoles[activeConsole].ShowRender(true);
		}
	}
	static Dictionary<double, PlayerScript> consoles = new Dictionary<double, PlayerScript>();
    public static Dictionary<double, PlayerScript> GetConsoles()
    {
        return consoles;
    }

	static float lastFrameTime;

	public static void Init(GameOptions options)
	{
		gameOptions = options;
		_addressFormat = GenerateAddressFormat();

        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
		if( playerObjects.Length <= 0 ) {
            Debug.LogError("GameManager did not find any players!");
        }
        for( int i=0; i < playerObjects.Length; i++ ) {
			double id = PlayerScript.GenerateId();
            consoles[id] = playerObjects[i].GetComponent<PlayerScript>() as PlayerScript;
			_activeConsole = id;
        }
        
		lastFrameTime = Time.time;
	}

	static string GenerateAddressFormat()
	{
		string format = new string('*', gameOptions.dataLength);

		int[] indxs = new int[gameOptions.setAddressChars];
		for( int i=0; i < indxs.Length; i++ ) {
			int rand;
			do {
				rand = UnityEngine.Random.Range(0, format.Length);
			} while( Array.IndexOf(indxs, rand) != -1 );

			indxs[i] = rand;
			rand = UnityEngine.Random.Range(0, CharSets.ALPHA_UPPER.Length-1);
			format = format.Remove(indxs[i], 1);
			format = format.Insert(indxs[i], CharSets.ALPHA_UPPER[rand].ToString());
		}

		return format;
	}
	
	public static void InitPlayers()
	{
		foreach( KeyValuePair<double, PlayerScript> player in consoles ) {
			player.Value.Init(player.Key);
		}
	}

	public static void CodeUpdate()
	{
		// Calc when to run a 'CodeUpdate'
		if( Time.time >= lastFrameTime + gameOptions.codeFrameDelay ) {
			PacketManager.CodeUpdate();
			NodeManager.CodeUpdate();
            foreach( PlayerScript console in consoles.Values ) {
                console.CodeUpdate();
            }

			lastFrameTime = Time.time;
		}

		// This happens every frame
		consoles[activeConsole].KeepFocus();
	}

    public static PlayerScript FindPlayer(double playerId)
    {
		if( !consoles.ContainsKey(playerId) ) {
			return null;
		}
        return consoles[playerId];
    }

    public static PlayerScript FindPlayerAtNode(double playerId, NodeScript node)
    {
        PlayerScript player = FindPlayer(playerId);
        if( player == null || player.connectedNode != node ) {
            return null;
        }
        return player;
    }
}
