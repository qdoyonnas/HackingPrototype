using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFocusScript : MonoBehaviour
{
	[NonSerialized]
	public Transform offset;
	public Camera childCamera;

	public Vector3 spin;
	public float defaultMoveDuration = 1;
	public float minZoomDistance = 6;
	
	Vector3Tweener positionTween = new Vector3Tweener(true);
	Vector3Tweener offsetTween = new Vector3Tweener(true);
	FloatTweener zoomTween = new FloatTweener(false);

	// Rotate tween (disables spin for duration of tween)
	Quaternion startRotation = Quaternion.identity;
	Quaternion targetRotation = Quaternion.identity;
	float rotateStartTime = -1;
	float rotateMoveDuration = -1;

	void Start()
	{
		offset = transform.Find("Offset");
		if( !offset ) {
			Debug.LogError("CameraFocus did not find Offset");
		}
		childCamera = transform.Find("Offset/Camera").GetComponent<Camera>();
		if( !childCamera ) {
			Debug.LogError("CameraFocus did not find Camera!");
		}

		spin = UnityEngine.Random.onUnitSphere * 0.2f;
	}

	void Update()
	{
		// Tweens
		transform.position = positionTween.Update(transform.position);
		childCamera.transform.localPosition = new Vector3( 0, 0, zoomTween.Update(childCamera.transform.localPosition.z) );
		offset.localPosition = offsetTween.Update(offset.localPosition);

		if( Time.time <= rotateStartTime + rotateMoveDuration ) {
			transform.localRotation = TweenRotate(Time.time - rotateStartTime);
		} else {
			transform.Rotate(spin);
		}
	}

	public void MoveTo( Vector3 target, float duration )
	{
		positionTween.StartTween(transform.position, target, duration);

		Offset(Vector3.zero, duration);
	}
	public void MoveTo( Vector3 target )
	{
		MoveTo(target, defaultMoveDuration);
	}
	public void MoveTo( NodeGraphicScript graphic, float duration )
	{
		MoveTo( graphic.transform.position, duration );
	}
	public void MoveTo( NodeGraphicScript graphic )
	{
		MoveTo( graphic.transform.position, defaultMoveDuration );
	}
	public void JumpTo( NodeGraphicScript graphic )
	{
		transform.position = graphic.transform.position;
	}

	public void SetDistance( float distance, float duration )
	{
		distance = (distance < minZoomDistance ? minZoomDistance : distance);
		
		zoomTween.StartTween(childCamera.transform.localPosition.z, -distance, duration);
	}
	public void SetDistance( float distance )
	{
		SetDistance(distance, defaultMoveDuration);
	}

	public void SetRotation( Vector3 change, float duration )
	{
		startRotation = transform.localRotation;
		targetRotation = startRotation * Quaternion.Euler(change);
		rotateMoveDuration = duration;
		rotateStartTime = Time.time;
	}
	public void SetRotation( Vector3 change )
	{
		SetRotation(change, defaultMoveDuration);
	}
	Quaternion TweenRotate (float time )
	{
		Quaternion newRot = Quaternion.Slerp( startRotation, targetRotation, time/rotateMoveDuration );

		return newRot;
	}

	public void Offset( Vector3 target, float duration )
	{
		offsetTween.StartTween(offset.localPosition, target, duration);
	}
	public void Offset( Vector3 target )
	{
		Offset(target, defaultMoveDuration);
	}
}
