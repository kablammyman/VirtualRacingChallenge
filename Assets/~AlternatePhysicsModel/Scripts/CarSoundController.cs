using UnityEngine;
using System.Collections;

// Simple class to controll sounds of the car, based on engine throttle and RPM, and skid velocity.
[RequireComponent (typeof (Drivetrain))]
[RequireComponent (typeof (CarController))]
public class CarSoundController : MonoBehaviour {

	public AudioClip engine;
	public AudioClip skid;
	public AudioClip crash1;
	public AudioClip crash2;
	public AudioClip crashFeedback1;//sends a low bass sound to bass shaker
	public AudioClip crashFeedback2;//sends a low bass sound to bass shaker
	
	AudioSource engineSource;
	AudioSource skidSource;
	AudioSource crash1Source;
	AudioSource crash2Source;
	AudioSource crashFBSource;
	AudioSource crashFB2Source;
	
	float crashLowVolume = .5f;
	float crashHighVolume = 1.0f;
	
	CarController car;
	Drivetrain drivetrain;
	
	AudioSource CreateAudioSource (AudioClip clip, bool loop = true) 
	{
		GameObject go = new GameObject("audio");
		go.transform.parent = transform;
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.identity;
		go.AddComponent(typeof(AudioSource));
		go.audio.clip = clip;
		go.audio.loop = loop;
		go.audio.volume = 1;
		if(loop)
			go.audio.Play();
		return go.audio;
	}
	
	void Start () {
		engineSource = CreateAudioSource(engine);
		skidSource = CreateAudioSource(skid);
		
		crash1Source = CreateAudioSource(crash1,false);
		crash2Source = CreateAudioSource(crash2,false);
		crashFBSource = CreateAudioSource(crashFeedback1,false);
		crashFBSource = CreateAudioSource(crashFeedback2,false);
		
		
		car = GetComponent (typeof (CarController)) as CarController;
		drivetrain = GetComponent (typeof (Drivetrain)) as Drivetrain;
		CrashController crash = gameObject.AddComponent(typeof(CrashController)) as CrashController;
	}
	
	void Update () {
		engineSource.pitch = 0.5f + 1.3f * drivetrain.rpm / drivetrain.maxRPM;
		engineSource.volume = 0.4f + 0.6f * drivetrain.throttle;
		skidSource.volume = Mathf.Clamp01( Mathf.Abs(car.slipVelo) * 0.2f - 0.5f );
	}
	
	public void crashSound(float volumeFactor)
	{
		if(volumeFactor > 0.7f)
		{
			crash1Source.PlayOneShot(crash1, Mathf.Clamp01((crashLowVolume + volumeFactor * crashLowVolume) * crashHighVolume));
			crashFBSource.PlayOneShot(crashFeedback1, crashHighVolume);
		}
		else
		{
			crash2Source.PlayOneShot(crash2, volumeFactor);
			crashFBSource.PlayOneShot(crashFeedback2, crashHighVolume);
		}
	}
}
