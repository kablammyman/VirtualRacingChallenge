using UnityEngine;
using System.Collections;

//throttle and brake go from 1 -> -1
public class InputManager : MonoBehaviour {

	float throttle;
	float brake;
	float steeringWheel;
	bool handBrake = false;
	int curGear = 0;
	bool gearUp = false;
	bool gearDown = false;
	float lookX,lookY;
	
	carAI cpu;
	XInputController x360;
	SteeringWheelController sw;
	
	public bool ignoreThrottle = false;
	public bool ignoreControl = true; 
	//if this is true, we cant change out of nutural, but we can rev, and turn the wheel
	public bool waitToStart = true;
	
	public enum InputType
	{
		KEYBOARD =0,
		JOYSTICK, //not just any joystick, but x360 since unity doesnt handle it well, it has its own code
		STEERINGWHEEL,
		CPU
	};
	public InputType inputType;
	
	public float getThrottle()
	{
		if(!ignoreThrottle)
			return throttle;
		return 0;
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
	public bool getGearUp()
	{
		return gearUp;
	}
	public bool getGearDown()
	{
		return gearDown;
	}
	public int getGear()
	{
		return curGear;
	}
	public float getLookX()
	{
		return lookX;
	}
	public float getLookY()
	{
		return lookY;
	}
	
	void Start()
	{
		if(inputType == InputType.CPU)
			cpu = gameObject.AddComponent<carAI>();
		else if(inputType == InputType.JOYSTICK)
			x360 = gameObject.AddComponent<XInputController>(); 
		else if(inputType == InputType.STEERINGWHEEL)
			sw = gameObject.AddComponent<SteeringWheelController>(); 
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(ignoreControl)
			return;
			
		//when we have ai enabled but not in use
		if(cpu != null)
		{
			if(waitToStart)
				cpu.inUse = false;
			else
				cpu.inUse = true;
		}	
		switch(inputType)
		{
			case InputType.KEYBOARD:
			{
				throttle = Input.GetAxis("Vertical");
				brake = 0;
				lookX = Input.GetAxis("Mouse X");
				lookY = Input.GetAxis("Mouse Y");
				if(throttle < 0)
				{
					brake = throttle *-1;//make this positive
					throttle = 0;
				}
				steeringWheel = Input.GetAxis("Horizontal");
				handBrake = Input.GetKey("space");
				if(!waitToStart)
				{
					gearUp = Input.GetKeyUp(KeyCode.A);
					gearDown = Input.GetKeyUp(KeyCode.Z);
				}
				else curGear = 0;
			}
			break;
			
			case InputType.JOYSTICK:
				throttle = x360.getThrottle();
				steeringWheel = x360.getSteeringWheel();
				handBrake = x360.getHandBrake();
				brake = x360.getBrake();
				lookX = x360.getLookX();
				lookY = x360.getLookY();
				if(!waitToStart)
				{
					gearUp = x360.getGearUp();
					gearDown = x360.getGearDown();
				}
				else curGear = 0;
			break;
			case InputType.STEERINGWHEEL:
				throttle = sw.getThrottle();
				steeringWheel = sw.getSteeringWheel();
				handBrake = sw.getHandBrake();
				brake = sw.getBrake();
				if(!waitToStart)
				{
					gearUp = sw.getGearUp();
					gearDown = sw.getGearDown();
					curGear = sw.getGear();
				}
				else curGear = 0;
					
			break;
			case InputType.CPU:
				throttle = cpu.getThrottle();
				steeringWheel = cpu.getSteeringWheel();
				handBrake = cpu.getHandBrake();
				brake = cpu.getBrake();
				lookX = Input.GetAxis("Mouse X");
				lookY = Input.GetAxis("Mouse Y");
				if(!waitToStart)
				{
					curGear = cpu.getGear();
				}
				else curGear = 0;
			break;
		}
	}
}
