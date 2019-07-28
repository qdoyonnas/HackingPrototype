using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketGraphicScript : MonoBehaviour
{
	// Move tween values
	Vector3Tweener positionTween = new Vector3Tweener(false);

	Vector3 startScale;
	float lifeTime;
	public float graphic_pulseFrequency = 0.2f;
	public float graphic_pulseMagnitude = 0.05f;

	Vector3 spin;

	void Start()
	{
		lifeTime = Time.time;
		startScale = transform.localScale;
		spin = Random.onUnitSphere;
	}
	
	void Update ()
	{
		transform.position = positionTween.Update(transform.position);
		if( !positionTween.active ) {
			PacketGraphicManager.RemovePacket(this);
		}

		float scaleOffset = Mathf.Sin((Time.time - lifeTime)/graphic_pulseFrequency) * graphic_pulseMagnitude;
		transform.localScale = startScale + (startScale * scaleOffset);

		transform.Rotate(spin);
	}

	public void MoveTo(NodeScript target)
	{
		Vector3 v_target;
		if( target == null || !NodeGraphicManager.NodeIsVisible(target) ) {
			v_target = Random.onUnitSphere * 40;
		} else {
			v_target = NodeGraphicManager.GetGraphic(target).transform.position;
		}
		positionTween.StartTween(transform.position, v_target, GameManager.gameOptions.packetTravelTime);
	}
}