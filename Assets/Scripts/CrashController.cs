using UnityEngine;
using System.Collections;

public class CrashController : MonoBehaviour 
{
	private CarSoundController sound;
	
	void Start()
	{
		sound = transform.root.GetComponent<CarSoundController>();
		//car = transform.GetComponent<Car>();
	}
	
	void  OnCollisionEnter ( Collision collInfo  )
	{
		if(enabled && collInfo.contacts.Length > 0)
		{
			float volumeFactor = Mathf.Clamp01(collInfo.relativeVelocity.magnitude * 0.08f);	
			ContactPoint contact = collInfo.contacts[0];
			Vector3 pos = contact.point;
			
			float angleOfCrash = Mathf.Atan2(pos.x - transform.position.x, pos.z - transform.position.z)*(180 / Mathf.PI);
			angleOfCrash -= transform.rotation.eulerAngles.y;
			
			if(angleOfCrash > 360)
			angleOfCrash %= 360;
			else if (angleOfCrash < 0)
			{
				while( angleOfCrash < 0)
					angleOfCrash+=360;
			}	
		
			//print("crash at: "+angleOfCrash + " degrees, force = " + collInfo.relativeVelocity.magnitude);
			volumeFactor *= Mathf.Clamp01(0.3f + Mathf.Abs(Vector3.Dot(collInfo.relativeVelocity.normalized, collInfo.contacts[0].normal)));
			volumeFactor = volumeFactor * 0.5f + 0.5f;
			sound.crashSound(volumeFactor);
		}
	}
}