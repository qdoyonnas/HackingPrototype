using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeGraphicScript : MonoBehaviour
{
	NodeScript node;

	float lifeTime;
	public float graphic_pulseFrequency = 0.2f;
	public float graphic_pulseMagnitude = 0.05f;
	public float defaultMoveDuration = 1;
	Vector3 originalScale;
	Vector3 memoryCubeOriginalScale;

	public Color memoryBaseColor;
	public Color memoryActiveColor;

	// Parts
	Transform sphere;
	Transform memory;
	Transform memoryData;
	Transform memoryRead;
	Transform logBox;
	Text logText;
	LogDetails logDetail = LogDetails.RESPONSE;

	// Tweens
	Vector3Tweener positionTween = new Vector3Tweener(true);
	Vector3Tweener scaleTween = new Vector3Tweener(false);
	FloatTweener rotateTween = new FloatTweener(false);

	// Memory pulse values
	public float memoryRotationSpeed = 0.2f;
	float rotationSpeed;
	public void SetRotationSpeed(float speed)
	{
		freeRotate = true;
		rotationSpeed = speed;
	}
	public Vector3 memoryHiddenScale = new Vector3(0.35f, 0.35f, 0.35f);
	public float memory_pulseFrequency = 0.8f;
	public float memory_pulseMagnitude = 0.2f;

	// Memory Rotate tween values
	bool freeRotate = true;
	int memoryColumns;
	int memoryAngleBetweenColumns;

	// Memory Scale tween values
	public bool memoryIsHidden = true;

	public void Init(NodeScript in_node)
	{
		node = in_node;
	}

	void Start ()
	{
		sphere = transform.Find("Sphere");
		originalScale = sphere.localScale;

		memory = transform.Find("Memory");
		memory.gameObject.SetActive(memoryIsHidden);
		memory.localScale = memoryHiddenScale;

		memoryData = transform.Find("Memory/MemoryData");
		memoryCubeOriginalScale = memoryData.GetChild(0).localScale;
		memoryRead = transform.Find("Memory/MemoryRead");

		logBox = transform.Find("Sphere/Canvas/Log");
		logText = transform.Find("Sphere/Canvas/Log/TextScroll/TextField").GetComponent<Text>();
		logBox.gameObject.SetActive(false);

		memoryColumns = GameManager.gameOptions.nodeMemoryLength / GameManager.gameOptions.readMemoryCount;
		memoryAngleBetweenColumns = 360 / memoryColumns;
		
		lifeTime = Time.time;
		rotationSpeed = memoryRotationSpeed;
	}
	
	void Update ()
	{
		// Update Position
		transform.position = positionTween.Update(transform.position);

		// Scale Pulse
		float scaleOffset = Mathf.Sin((Time.time - lifeTime)/graphic_pulseFrequency) * graphic_pulseMagnitude;
		sphere.localScale = originalScale + (originalScale * scaleOffset);

		// Lock rotation to camera
		transform.rotation = GameManager.FindPlayer(GameManager.activeConsole).cameraFocus.transform.localRotation;

		// Update Log Text
		if( logBox.gameObject.activeSelf ) {
			logText.text = GetLogText();
		}

		UpdateMemory();
	}
	void UpdateMemory()
	{
		if( !memory.gameObject.activeSelf ) {
			return;
		}

		// Hide/unhide memory
		memory.localScale = scaleTween.Update(memory.localScale);
		
		if( !scaleTween.active ) {
			if ( memoryIsHidden ) {
				memory.gameObject.SetActive(false);
				memoryData.localRotation = Quaternion.identity;
			}
				
			memoryData.localRotation = Quaternion.Euler(memoryData.localEulerAngles.x, 
													rotateTween.Update(memoryData.localEulerAngles.y),
													memoryData.localEulerAngles.z);
			if( !rotateTween.active && freeRotate ) {
				// Spin
				memoryData.Rotate(transform.up, rotationSpeed, Space.World);
			}
		}

		// Scale pulse memory cubes
		for( int i=0; i < memoryData.childCount; i++ ) {
			float cubeAge = (Time.time - lifeTime) + (i * 100);
			float scaleOffset = Mathf.Sin(cubeAge/memory_pulseFrequency) * memory_pulseMagnitude;
			memoryData.GetChild(i).localScale = memoryCubeOriginalScale + (memoryCubeOriginalScale * scaleOffset);
			memoryData.GetChild(i).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", memoryBaseColor);
		}

		// Set MemoryRead text
		GetMemoryText();
	}

	string GetLogText()
	{
		string output =
			"\nNode " + node.GetAddressString() + " log start\n" +
			"------------------------------------";
		output += node.ReadLog(logDetail);

		return output;
	}
	public void DisplayLog(LogDetails detail)
	{
		logBox.gameObject.SetActive(true);
		logDetail = detail;
	}
	public void HideDisplay()
	{
		logBox.gameObject.SetActive(false);
	}

	public void MoveTo( Vector3 target, float duration )
	{
		positionTween.StartTween(transform.position, target, duration);
	}
	public void MoveTo(Vector3 target)
	{
		MoveTo(target, defaultMoveDuration);
	}
	public void JumpTo( Vector3 target )
	{
		transform.position = target;
	}

	public void ShowMemory( bool show )
	{
		if( memoryIsHidden != show ) {
			return;
		}

		if( show ) {
			memoryIsHidden = false;
			memory.gameObject.SetActive(true);
			logBox.gameObject.SetActive(false);
			ScaleMemory(Vector3.one);
		} else {
			memoryIsHidden = true;
			ScaleMemory(memoryHiddenScale);
		}
	}
	void GetMemoryText()
	{
		// Get block number
		int block = (int)(memoryData.localEulerAngles.y / memoryAngleBetweenColumns);
		int block_index = block * GameManager.gameOptions.readMemoryCount; // This will lose trailing decimals

		// Write block to output
		for( int i = 0; i < GameManager.gameOptions.readMemoryCount; i++ ) {
			int memory_index = block_index + i;
			if( memory_index >= GameManager.gameOptions.nodeMemoryLength ) {
				memoryRead.GetChild(i).GetComponent<TextMesh>().text = "";
				continue;
			}
			string s_index = memory_index.ToString();
			while( s_index.Length < GameManager.gameOptions.nodeMemoryLength.ToString().Length ) {
				s_index = "0" + s_index;
			}
			memoryRead.GetChild(i).GetComponent<TextMesh>().text = s_index + ": " 
																	+ "[" + node.GetLabel(memory_index) + "] "
																	+ node.GetMemory(memory_index).ToString(); 

			memoryData.GetChild(memory_index).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", memoryActiveColor);
		}
	}

	public void RotateMemory( int index, float duration )
	{
		// Get block number
		int block = index / GameManager.gameOptions.readMemoryCount;
		float angle = (block * memoryAngleBetweenColumns) + 1;

		freeRotate = false;
		rotateTween.StartTween(memoryData.localEulerAngles.y, angle, duration);
	}
	public void RotateMemory( int index )
	{
		RotateMemory(index, defaultMoveDuration);
	}

	public void ScaleMemory( Vector3 target, float duration )
	{
		scaleTween.StartTween(memory.localScale, target, duration);
	}
	public void ScaleMemory( Vector3 target )
	{
		ScaleMemory(target, defaultMoveDuration);
	}
}