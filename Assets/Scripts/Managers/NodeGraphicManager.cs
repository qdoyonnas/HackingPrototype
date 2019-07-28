using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NodeGraphicManager
{
	public static float defaultSpacingRadius = 10;
	
	public static float memoryRadius = 1.5f;
	public static float memoryTextRadius = 1.75f;
	public static float memoryTextHeight = 0.25f;

	static GameObject gameObject;
	static Object nodePrefab;

	public static Dictionary<double, NodeGraphicScript> nodes = new Dictionary<double, NodeGraphicScript>();

	public static void Init()
	{
		gameObject = GameObject.Find("NodeManager");
		if( !gameObject ) {
			Debug.LogError("NodeManager did not find its object");
		}
		/*nodePrefab = Resources.Load("Prefabs/Node");
		if( !nodePrefab ) {
			Debug.LogError("NodeManager did not find Node Prefab!");
		}*/

		CreateMemoryGrid();
	}

	static void CreateMemoryGrid()
	{
		// Load Resources
		Object baseNodePrefab = Resources.Load("Prefabs/Node");
		Object memoryCube = Resources.Load("Prefabs/MemoryCube");
		Object dataText = Resources.Load("Prefabs/DataText");

		// Create runtime 'prefab'
		nodePrefab = GameObject.Instantiate(baseNodePrefab);
		GameObject g_node = nodePrefab as GameObject;
		Transform t_memory = g_node.transform.Find("Memory/MemoryData");
		Transform t_memoryRead = g_node.transform.Find("Memory/MemoryRead");

		// Calc values
		int rows = GameManager.gameOptions.readMemoryCount;
		int columns = GameManager.gameOptions.nodeMemoryLength / GameManager.gameOptions.readMemoryCount;

		float zRange = (7*Mathf.PI/9);
		float zAngleBetween = zRange / rows;
		float yAngleBetween = (Mathf.PI*2) / columns;

		int i = 0;
		float startZAngle = (zRange/2) - (zAngleBetween/2);
		float zAngle = startZAngle;
		float yAngle = 0;
		for( int y=0; y < columns; y++ ) {
			for( int x=0; x < rows; x++ ) {
				// Calc pos
				float xPos = (Mathf.Cos(yAngle) * Mathf.Cos(zAngle)) * memoryRadius;
				float zPos = (Mathf.Sin(yAngle) * Mathf.Cos(zAngle)) * memoryRadius;
				float yPos = Mathf.Sin(zAngle) * memoryRadius;
				Vector3 pos = new Vector3(xPos, yPos, zPos);

				Quaternion angle = Quaternion.Euler( 0, -(yAngle * 180/Mathf.PI), (zAngle * 180/Mathf.PI) );

				// Instantiate and configure (name)
				GameObject mem = GameObject.Instantiate(memoryCube, pos, angle, t_memory) as GameObject;
				mem.name = "mem" + i;

				i++;
				zAngle -= zAngleBetween;
			}
			zAngle = startZAngle;
			yAngle += yAngleBetween;
		}
		
		zAngle = startZAngle;
		for( int x=0; x < rows; x++ ) {
			// Calc pos
			float xPos = memoryTextRadius; //Mathf.Cos(zAngle) * memoryTextRadius;
			float zPos = 0;
			float yPos = (rows/2 * memoryTextHeight) - (x * memoryTextHeight); //Mathf.Sin(zAngle) * memoryTextRadius;
			Vector3 pos = new Vector3(xPos, yPos, zPos);
			
			// Instantiate and configure (name)
			GameObject mem = GameObject.Instantiate(dataText, pos, Quaternion.identity, t_memoryRead) as GameObject;
			
			zAngle -= zAngleBetween;
		}

		// Configure and hide node 'prefab'
		//t_memory.localScale = new Vector3(0.35f, 0.35f, 0.35f);
		g_node.SetActive(false);
	}

	public static NodeGraphicScript DisplayNode( Vector3 position, double id )
	{
		if( nodes.ContainsKey(id) )
		{
			NodeGraphicScript node = nodes[id];
			node.MoveTo( position );
			return node;
		}
		
		NodeScript script = NodeManager.GetNode(id);
		if( script == null ) {
			return null;
		}

		NodeGraphicScript graphic = CreateGraphic( script, position, defaultSpacingRadius );
		return graphic;
	}
	public static NodeGraphicScript DisplayNode( Vector3 position, NodeScript node )
	{
		return DisplayNode( position, node.GetAddress() );
	}

	public static void HideNode( double id )
	{
		if( !nodes.ContainsKey(id) )
		{
			return;
		}

		NodeGraphicScript node = nodes[id];
		nodes.Remove(id);
		GameObject.Destroy(node.gameObject);
	}
	public static void HideAll()
	{
		foreach( double id in nodes.Keys ) {
			NodeGraphicScript node = nodes[id];
			nodes.Remove(id);
			GameObject.Destroy(node.gameObject);
		}
	}

	public static bool NodeIsVisible( double id )
	{
		return nodes.ContainsKey(id);
	}
	public static bool NodeIsVisible( NodeScript node )
	{
		return NodeIsVisible( node.GetAddress() );
	}

	public static NodeGraphicScript GetGraphic( double id ) {
		return nodes[id];
	}
	public static NodeGraphicScript GetGraphic( NodeScript node ) {
		return nodes[node.GetAddress()];
	}

	public static NodeGraphicScript CreateGraphic(NodeScript node, Vector3 position, float radius)
	{
		// XXX: Collision checking for finding position
		Vector3 pos = position + UnityEngine.Random.onUnitSphere * radius;

		// XXX: Spawn far away (in param) and MoveTo position
		GameObject newNode;
		newNode = GameObject.Instantiate( nodePrefab, pos, Quaternion.identity, gameObject.transform ) as GameObject;
		newNode.SetActive(true);
		NodeGraphicScript script = newNode.GetComponent<NodeGraphicScript>();
		script.Init(node);
		nodes.Add( node.GetAddress(), script );

		return script;
	}
}