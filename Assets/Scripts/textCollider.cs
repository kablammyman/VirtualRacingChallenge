using UnityEngine;
using System.Collections;

//since we are in oculus land, more nad more text will be in the world, and not as a hud
//we will use a trigger to know when to remove the text from the world
public class textCollider : MonoBehaviour 
{	
	void OnTriggerEnter(Collider other) 
	{
        //print(other.transform.parent.name );
		if(other.transform.parent.name == "Player1")
			gameObject.SetActive(false);
    }
}
