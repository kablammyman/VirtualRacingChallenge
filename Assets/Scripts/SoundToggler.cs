using UnityEngine;
using System.Collections;

public class SoundToggler : MonoBehaviour
{
	public float fadeTime = 1.0f; 
	private SoundController soundScript; 

	void  Start ()
	{ 
	   soundScript = FindObjectOfType(typeof(SoundController)) as SoundController; 
	} 

	void  OnTriggerEnter ()
	{ 
		if(soundScript)
			soundScript.ControlSound(true, fadeTime); 
	} 

	void  OnTriggerExit ()
	{ 
		if(soundScript)
			soundScript.ControlSound(false, fadeTime); 
	}
}