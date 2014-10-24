using UnityEngine;
using System.Collections;

public class Tachometer : MonoBehaviour {

public Transform needle;//The needle object of the meter
	
	
public float inputValue=0f;// The value provided by the player
public float inputMin=0f;// The minimum expected value for the meter's lowest reading
public float inputMax=0f;// The maximum expected value for the meter's highest reading
float inputTemp=0f;	//Temporary number
	
float currentReading=0f; // The angle of rotation for the needle object
public float minReading=0f;// The minimum angle of rotation for the needle object
public float maxReading=350f;// The maximum angle of rotation for the needle object
	
/*float testNum=0;// The number used to test the 
bool testTurn=true;// Used to cycle the test number
public bool testing=false;	//Are we testing?
*/	
	
	void Awake()
	{
		/*if(testing)
		{
			testNum=inputMin;
		}else*/{
			inputValue=inputMin;
		}
	}
	
	/*void Update()
	{
		if(testing)
		{
			if(testTurn)
			{
				testNum=testNum+0.1f;
				if(testNum>=inputMax)
				{
					testTurn=false;
				}
			}else{
				testNum=testNum-0.1f;
				if(testNum<=inputMin)
				{
					testTurn=true;
					
				}
			}
		inputValue=testNum;
		}
	}	*/
	
	void FixedUpdate()
	{
		inputTemp = (inputValue - inputMin)/(inputMax - inputMin);  
	    currentReading = inputTemp*(maxReading - minReading) + minReading;    
	    needle.localRotation = Quaternion.Euler(new Vector3(0f,0f,currentReading));  		
	}


}
