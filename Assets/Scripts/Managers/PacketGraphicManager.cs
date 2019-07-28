using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PacketGraphicManager
{
	public static GameObject gameObject;
	static UnityEngine.Object packetPrefab;

	public static List<PacketGraphicScript> packets = new List<PacketGraphicScript>();

	public static void Init()
	{
		gameObject = GameObject.Find("PacketManager");
		if( !gameObject ) {
			Debug.LogError("NodeManager did not find its object");
		}
		packetPrefab = Resources.Load("Prefabs/Packet");
		if( !packetPrefab ) {
			Debug.LogError("PacketManager did not find Packet Prefab!");
		}
	}

	public static void DisplayPacket( NodeScript source, NodeScript target )
	{
		// Determine if sourceNode needs graphic
		if( !NodeGraphicManager.NodeIsVisible(source)
				&& target != null
				&& NodeGraphicManager.NodeIsVisible(target) ) { 
			foreach(PlayerScript console in GameManager.GetConsoles().Values ) {
				if( target == console.connectedNode ) {
					NodeGraphicScript graphic = NodeGraphicManager.GetGraphic(target);
					NodeGraphicManager.DisplayNode(graphic.transform.position, source);
					break;
				}
			}
		}

		// Show packet graphics
		if( NodeGraphicManager.NodeIsVisible(source) ) {
			PacketGraphicManager.CreatePacketGraphic(source, target);
		}
	}

	public static void CreatePacketGraphic( NodeScript source, NodeScript target )
	{
		Vector3 vrot = Random.onUnitSphere;
		Quaternion rot = Quaternion.Euler(vrot);
		GameObject packetGraphics = GameObject.Instantiate(packetPrefab, NodeGraphicManager.GetGraphic(source).transform.position,
			rot, gameObject.transform) as GameObject;
			
		PacketGraphicScript graphicsScript = packetGraphics.GetComponent<PacketGraphicScript>();
		graphicsScript.MoveTo(target);
		packets.Add(graphicsScript);
	}

	public static void RemovePacket( PacketGraphicScript packet )
	{
		packets.Remove(packet);
		GameObject.Destroy(packet.gameObject);
	}
}