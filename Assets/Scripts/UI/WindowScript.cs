using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowScript : MonoBehaviour
{
	public RectTransform activePosition;
	public RectTransform hidePosition;
	public Graphic[] elementsToHide;
	public float transitionTime = 0.5f;

	//[NonSerialized]
	public Text text;

	bool isHiding = true;
	public bool GetIsHiding()
	{
		return isHiding;
	}
	Vector3 startPos = Vector3.zero;
	Vector3 changeVector = Vector3.zero;
	float panStartTime = -1;
	float panMoveDuration = -1;
	
	void Awake()
	{
		text = transform.Find("TextScroll/TextField").GetComponent<Text>();
		if( text == null ) {
			Debug.LogError("Window did not find its TextField");
		}
	}

	void Update ()
	{
		if( Time.time <= panStartTime + panMoveDuration ) {
			transform.position = TweenMovement(Time.time - panStartTime);
		}
	}

	public void ToggleHide()
	{
		SetHide(!isHiding);
	}

	public void SetHide(bool hide)
	{
		if( isHiding == hide ) {
			return;
		}

		isHiding = hide;

		startPos = transform.position;
		panMoveDuration = transitionTime;
		panStartTime = Time.time;

		if( isHiding ) {			
			changeVector = hidePosition.position - startPos;
		} else {
			changeVector = activePosition.position - startPos;
		}

		foreach( Graphic graphic in elementsToHide ) {
			graphic.gameObject.SetActive(!isHiding);
		}
	}

	Vector3 TweenMovement(float time)
	{
		Vector3 newPos = new Vector3();

		newPos = -changeVector/2 * (Mathf.Cos(Mathf.PI * time/panMoveDuration) - 1) + startPos;

		return newPos;
	}
}