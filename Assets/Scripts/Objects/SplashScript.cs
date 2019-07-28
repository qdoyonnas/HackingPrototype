using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void Callback();

public class SplashScript : MonoBehaviour
{
	public float fadeInTime = 1;
	public float fadeOutTime= 4;
	public float fadeDuration = 1;

	int state = 0;
	float startTime;
	Image image;
	FloatTweener fadeTween = new FloatTweener(false);
	
	void Start()
	{
		startTime = Time.time;
		image = GetComponent<Image>();
	}

	void Update()
	{
		Color newColor = image.color;
		newColor.a = fadeTween.Update(newColor.a);
		image.color = newColor;

		if( state == 0 && Time.time >= startTime + fadeInTime ) {
			fadeTween.StartTween(0, 1, fadeDuration);
			state = 1;
		} else if( state == 1 && Time.time >= startTime + fadeOutTime ) {
			fadeTween.StartTween(1, 0, fadeDuration);
			state = 2;
		}
	}
}