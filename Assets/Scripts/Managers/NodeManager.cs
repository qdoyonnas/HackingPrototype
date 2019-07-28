using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NodeManager
{
	static double[] nodeAddresses;

	static Dictionary<double, NodeScript> nodes = new Dictionary<double, NodeScript>();
	public static Dictionary<double, NodeScript> GetNodes()
	{
		return nodes;
	}

	public static Queue<NodeScript> callQueue;

	public static void Init()
	{
		// Generate Node address
		nodeAddresses = new double[GameManager.gameOptions.nodeCount];
		for( int i=0; i < nodeAddresses.Length; i++ ) {
			double adrs;
			do {
				adrs = GenerateAddress();
			} while ( Array.IndexOf(nodeAddresses, adrs) != -1 );

			nodeAddresses[i] = adrs;
		}

		callQueue = new Queue<NodeScript>();
	}

	public static void CodeUpdate()
	{
		// XXX: Add frame throttling control
		int count = callQueue.Count;
		for( int i=0; i < count; i++ ) {
			NodeScript node = callQueue.Dequeue();
			node.CodeUpdate();
		}
	}

	public static double GenerateAddress()
	{
		// Randomize address string based on AddressFormat
		string s_address = GameManager.GetAddressFormat();
		for( int i=0; i < s_address.Length; i++ )
		{
			if( s_address[i] == '*' ) {
				int rand = UnityEngine.Random.Range(0, CharSets.ALPHA_LOWER.Length);
				s_address = s_address.Remove(i, 1);
				s_address = s_address.Insert(i, CharSets.ALPHA_LOWER[rand].ToString());
			}
		}
		// Convert to number
		double address = 0;
		AlphaNumeral.StringToDbl(s_address, out address);

		return address;
	}
	
	public static double GetRandomAddress()
	{
		int idx = UnityEngine.Random.Range(0, nodeAddresses.Length);
		return nodeAddresses[idx];
	}

	public static double GetInactiveAddress()
	{
		double adrs;
		do {
			adrs = GetRandomAddress();
		} while ( nodes.ContainsKey(adrs) );

		return adrs;
	}

	public static bool CheckValidAddress( double adrs )
	{
		for( int i=0; i < nodeAddresses.Length; i++ ) {
			if( nodeAddresses[i] == adrs ) {
				return true;
			}
		}

		return false;
	}

	public static NodeScript CreateNode( double id )
	{
		// Script Only

		if( nodes.ContainsKey(id) ) {
			return nodes[id];
		}
		
		NodeScript script = new NodeScript(id);

		nodes.Add(id, script);
		callQueue.Enqueue(script);
		return script;
	}

	public static NodeScript CreateNode()
	{
		double id = GetInactiveAddress();
		return CreateNode(id);
	}

	public static NodeScript GetNode( double id, bool create = false )
	{
		if( !CheckValidAddress(id) ) {
			return null;
		}

		NodeScript node = null;
		if( !nodes.ContainsKey(id) ) {
			if( create ) {
				node = CreateNode(id);
			}
		} else {
			node = nodes[id];
		}

		return node; // will return null if not found
	}

	public static NodeScript GetNode( int i )
	{
		// XXX: Super hackish but it is mainly just to get any active Node
		double key = 0;
		int atI = 0;
		foreach( double k in nodes.Keys ) {
			key = k;
			atI++;
			if( atI >= i ) {
				break;
			}
		}

		return GetNode(key);
	}
}
