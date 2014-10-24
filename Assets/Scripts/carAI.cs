using UnityEngine;
using System.Collections;

public class carAI : MonoBehaviour 
{
	public enum ThrottleLogicState
	{
		IDLE = 0,
		ACCELERATE,
		CRUISE,
		SLOW_DOWN,
		STOP,
		BACK_UP
	};
	public ThrottleLogicState throttleLogic;
	
	public enum StuckState  //we trying to move, but we cant
	{
		NOT_STUCK = 0, //we are good to go
		VERIFY, //we may be stuck, since speed is so low
		IS_STUCK, //yup, we are stuck
		CORRECTING //we are fixing the issue
	};

	public StuckState stuckState;
	public ThrottleLogicState actionToGetUnstuck;
	
	const float PI	= 3.14159265359f;
	const float RadToDeg = 180 / Mathf.PI;
	
	float throttle;
	float brake;
	float steeringWheel;
	bool handBrake;
	int curGear = 1;
	
	float aiTurnSpeed = 2;//the faster this is, the weirder things get! 15 did some wierd shit
	float angleToTarget = 0;
	
	float prevSpeed = 0;
	float angleDiff = 0;
	float[] deadZone = {10, 30,50,90};
	float anglePercent;
	float stuckTimer = 0;

	public bool inUse = false;
	Vector3 stuckPos;
	
	RaceLogic raceScript; 
	Drivetrain dt;
		
	void Start () 
	{		
		raceScript = gameObject.GetComponent(typeof(RaceLogic)) as RaceLogic;
		dt = gameObject.GetComponent(typeof(Drivetrain)) as Drivetrain;
		raceScript.minDistFromChecpoint = 15;		
		throttleLogic = ThrottleLogicState.ACCELERATE;
		stuckState = StuckState.NOT_STUCK;
	}
//---------------------------------------------------------------------------------------------------------------	
	void applyThrottle()
	{
		handBrake = false;
		switch (throttleLogic)
		{
			case ThrottleLogicState.IDLE:
				curGear = 0;
				throttle = 0;
				brake = 0;
			break;
			case ThrottleLogicState.ACCELERATE:
				curGear = 1;
				
				angleDiff = Mathf.Abs(angleToTarget - transform.rotation.eulerAngles.y);
				if(angleDiff < 20)
					throttle = 1f;
				else if(angleDiff >= 20 && angleDiff < 40)
						throttle = .7f;
				else
					throttle = .3f;
				brake = 0;
			break;
			case ThrottleLogicState.CRUISE: //is this the same as idle while moving?
				throttle = .3f;
				brake = 0;
			break;
			case ThrottleLogicState.SLOW_DOWN:
				if(dt.speed > 30)
				{
					brake = .7f;
					throttle = 0;
				}
				else
					throttleLogic = ThrottleLogicState.ACCELERATE;
			break;	
			case ThrottleLogicState.STOP:
				if(dt.speed != 0)
				{
					brake = 1;
					throttle = 0;
					handBrake = true;
				}
				 else throttleLogic = ThrottleLogicState.IDLE;
			break;
			case ThrottleLogicState.BACK_UP:
				curGear = -1;
				brake = 0;
				throttle = .7f;
				//print("trying to back up");
			break;
		}
	}
//---------------------------------------------------------------------------------------------------------------	
	void checkIfStuck()
	{
		switch(stuckState)
		{
			case StuckState.NOT_STUCK:
				if(dt.speed < 1 && throttleLogic != ThrottleLogicState.STOP)
				{
					stuckState = StuckState.VERIFY;
					stuckTimer = Time.time;
				}
			break;
			case StuckState.VERIFY:
				if((Time.time - stuckTimer >= 2) && dt.speed <= 1)
				{
					stuckState = StuckState.IS_STUCK;
				}
				else if(dt.speed > 3 && prevSpeed > 3)//if we speed up again, we aint stuck, reset the state
				{
					stuckState = StuckState.NOT_STUCK;
					throttleLogic = ThrottleLogicState.ACCELERATE;
				}
			break;
			case StuckState.IS_STUCK:
				//print ("we stuck");
								
				if(throttleLogic == ThrottleLogicState.ACCELERATE || throttleLogic == ThrottleLogicState.CRUISE || throttleLogic == ThrottleLogicState.SLOW_DOWN)
					throttleLogic = ThrottleLogicState.BACK_UP;
				else if(throttleLogic == ThrottleLogicState.BACK_UP )
					throttleLogic = ThrottleLogicState.CRUISE;
				
				actionToGetUnstuck = throttleLogic;
				stuckPos = transform.position;
				raceScript.findNeastCheckpoint();
				//since we may have new check point, refresh data
				angleToTarget = Mathf.Atan2(raceScript.checkPointPos.x - transform.position.x, raceScript.checkPointPos.z - transform.position.z)* RadToDeg;

				stuckState = StuckState.CORRECTING;
				
			break;
			case StuckState.CORRECTING:
				//print("we stuck. distToTarget = " + distToTarget + " angleToTarget = " + angleToTarget + " throttle = " + throttleLogic);
				//when we are correcting, it may be best to reasses what check point to go after, then try again.
				/*if(speed < 1 && throttleLogic != ThrottleLogicState.STOP)
				{
					stuckState = StuckState.VERIFY;
					stuckTimer = Time.time;
				}*/
				
				if(Vector3.Distance(stuckPos, transform.position) >5)
				{
					stuckState = StuckState.NOT_STUCK;
					throttleLogic = ThrottleLogicState.ACCELERATE;
				}
				else 
					throttleLogic = actionToGetUnstuck;
			break;
		}
		
	}
//---------------------------------------------------------------------------------------------------------------
/*mayeb we look ahead at the next 3 check points, and derive whats going on, based on thier posistion to each other
ex: look at curcheck point, then the one after, see if they are really far left or right (get angle between them), same with 3rd
if they are far, we will turn soon. the 
*/
	void followTrackPath()
	{
		angleToTarget = Mathf.Atan2(raceScript.checkPointPos.x - transform.position.x, raceScript.checkPointPos.z - transform.position.z)* RadToDeg;
		
		if(inUse)
			checkIfStuck();
	
		getAngleWithin360(ref angleToTarget);
		
		Vector3 steerVector = transform.InverseTransformPoint(new Vector3(raceScript.checkPointPos.x,transform.position.y,raceScript.checkPointPos.z));  
		float newSteer =  aiTurnSpeed*(steerVector.x / steerVector.magnitude);  
		steeringWheel = newSteer;
			
			
		if(stuckState < StuckState.IS_STUCK)
		{
			angleDiff = Mathf.Abs(angleToTarget - transform.rotation.eulerAngles.y);
			
			if(angleDiff < deadZone[0])
				throttleLogic = ThrottleLogicState.ACCELERATE;
			else if(angleDiff >= deadZone[0]/2 && angleDiff < (deadZone[1]))
				throttleLogic = ThrottleLogicState.CRUISE;	
			if(dt.speed > 30)
			{
				if(angleDiff >= (deadZone[1]/2) && angleDiff < (deadZone[2]))
					throttleLogic = ThrottleLogicState.SLOW_DOWN;	
				/*else if(angleDiff >= (deadZone[2]))
					throttleLogic = ThrottleLogicState.STOP;	*/
			}	
		}
		else //if we are stuck steer this way to get back on track
		{
			if(throttleLogic == ThrottleLogicState.BACK_UP)
				steeringWheel *=-1;
		}
		applyThrottle();
		prevSpeed = dt.speed;
	}

//---------------------------------------------------------------------------------------------------
	public float getThrottle()
	{
		return throttle;
	}
	public float getBrake()
	{
		return brake;
	}
	public float getSteeringWheel()
	{
		return steeringWheel;
	}
	public bool getHandBrake()
	{
		return handBrake;
	}
	public int getGear()
	{
		return curGear;
	}
	
	void getAngleWithin360(ref float ang)
	{
		if(ang > 360)
			ang %= 360;
		else if (ang < 0)
		{
			while( ang < 0)
				ang+=360;
		}	
	}
//---------------------------------------------------------------------------------------------------------------
	// Update is called once per frame
	void Update () 
	{
		//if(inUse)
			followTrackPath();
	}
}

/*old code to steer the car. it "worked" but wasnt smooth
//print("angleToTarget: " + angleToTarget + " my angle: " + transform.rotation.eulerAngles.y);
	if(angleDiff < 180)
	{
		if(angleToTarget > transform.rotation.eulerAngles.y)
		{
			//print("turn right");
			steeringWheel = newSteer; //anglePercent;
		}
		else if(angleToTarget < transform.rotation.eulerAngles.y)
		{
			//print("turn left");
			steeringWheel = -1; //-anglePercent;
		}
		else
		{
			//print("go straight");
			steeringWheel = 0;
		}
	}
	else //OTHERWISE, DONT TURN AROUND THE LONG WAY!!
	{
		if(angleToTarget > transform.rotation.eulerAngles.y)
		{
			//print("turn LEFT");
			steeringWheel = -1; //anglePercent;
		}
		else if(angleToTarget < transform.rotation.eulerAngles.y)
		{
			//print("turn RIGHT");
			steeringWheel = 1; //-anglePercent;
		}
		else
		{
			//print("go STRAIGHT");
			steeringWheel = 0;
		}
		
		//Debug.Break();
	}
*/
