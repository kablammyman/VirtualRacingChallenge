using UnityEngine;
using XInputDotNetPure; // Required in C#

public class SteeringWheelController : MonoBehaviour
{
    // Reference to WheelInput class
    public WheelInput dinput;

	//Drivetrain dt;
	
	bool gearUpButton = false,gearDownButton = false;
	int curGear = 0;

 /////////force feedback//////////////////////////////	
    Wheel wheelFrontLeft;
    Wheel wheelFrontRight;

    // Force Feedback Scale
    public float ffbScale = 1f;

    // Downforce Factor
    public float FzFactor = 0.001f;


    float[] c = { 2.2f, -3.9f, -3.9f, -1.26f, -8.2f, 0.025f, 0f, 0.044f, -0.58f, 0.18f, 0.043f, 0.048f, -0.0035f, -0.18f, 0.14f, -1.029f, 0.27f, -1.1f }; // aligning values
 //////////////////////////////////////////////////////	
 
	public float getThrottle()
	{
		return dinput.GetThrottle();
	}
	public float getBrake()
	{
		return dinput.GetBrake();
	}
	public float getSteeringWheel()
	{
		// Sensitivity is a logistics growth curve (S shaped curve) trends between 0.95f to 0. the 0.05f is a constant to ensure we always have some steering 
        //float sensitivity = (.95f / (1 + 0.02f * Mathf.Exp(0.025f * veloKmh))) + 0.05f;
		//float sensitivity = (.95f / (1 + 0.3f * Mathf.Exp(0.06f * Mathf.Abs(transform.InverseTransformDirection(rigidbody.velocity).z)))) + 0.05f;
        //return dinput.GetSteer(sensitivity);
		
		
		return dinput.GetSteer(1);
	}
	public bool getHandBrake()
	{
		if(dinput.GetHandbrake()> 0)
			return true;
		return false;
	}
	public bool getGearUp()
	{
		if(gearUpButton && !dinput.getButton(4))//if(dinput.getInputStatus(GameOptions.InputType.ShiftUp) > 0)
		{
			gearUpButton = false;
			return true;
		}
		if(dinput.getButton(4))
		{
			gearUpButton = true;
		}
		
		return false;
	}
	public bool getGearDown()
	{
		if(gearDownButton && !dinput.getButton(5))//if( dinput.getInputStatus(GameOptions.InputType.ShiftDown) > 0)
		{
			gearDownButton = false;
			return true;
		}
		if(dinput.getButton(5))
		{
			gearDownButton = true;
		}
		
		return false;
	}
	
	public int getGear()
	{
		if(dinput.getButton(8))
			curGear = 1;
		else if(dinput.getButton(9))
			curGear = 2;
		else if(dinput.getButton(10))
			curGear = 3;
		else if(dinput.getButton(11))
			curGear = 4;
		else if(dinput.getButton(12))
			curGear = 5;
		else if(dinput.getButton(13))
			curGear = 6;
		else if(dinput.getButton(14))
			curGear = -1;
		else
			curGear = 0;
			
		return curGear;
	}

    void Awake()
    {
        try
        {
            //inputScan = gameObject.AddComponent<InputScan>();
            dinput = WheelInput.Instance;
			dinput.SetGain(100);
			
        }
        catch { print("Error instantiating WheelInput"); }
		
		//dt = gameObject.GetComponent(typeof(Drivetrain)) as Drivetrain; 
		wheelFrontLeft = GameObject.Find("WheelFL").GetComponent(typeof(Wheel)) as Wheel;
		if(wheelFrontLeft == null)
			Debug.LogWarning("couldnt find front left wheel");
		wheelFrontRight = GameObject.Find("WheelFR").GetComponent(typeof(Wheel)) as Wheel;
		if(wheelFrontRight == null)
			Debug.LogWarning("couldnt find front right wheel");	
    }


    void OnDestroy()
    {
        if (dinput != null)
        {
            dinput.SelfDestruct();
            dinput = null;
        }
    }

   

    void OnApplicationFocus(bool focus)
    {
        // If constant force was running when we lost focus, we need to stop it now. Otherwise it will stop working
        if (focus)
        {
            if (WheelInput.Instance != null)
                WheelInput.Instance.StopConstantForce();
        }
    }
	
	void Update()
	{
		dinput.GetInput();
		calcForeFeedback();
        //wheelMenu.dinput.SetLeds(drivetrain.rpm, drivetrain.minRPM + 1500, drivetrain.maxRPM + 1500);
	}
	
	void  OnCollisionEnter ( Collision collInfo  )
	{
		if(enabled && collInfo.contacts.Length > 0)
		{
			
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
		
			print("crash at: "+angleOfCrash + " degrees, force = " + collInfo.relativeVelocity.magnitude);
			if(angleOfCrash >=315 || angleOfCrash < 45 )
				dinput.PlayFrontalCollisionForce((int)collInfo.relativeVelocity.magnitude*4); 
			else if(angleOfCrash >= 185 && angleOfCrash <=314 )
				dinput.PlaySideCollisionForce(-(int)collInfo.relativeVelocity.magnitude*4);//neg num = left side
			else //if(angleOfCrash >= 195 && angleOfCrash <=329 )
				dinput.PlaySideCollisionForce((int)collInfo.relativeVelocity.magnitude*4);
		}
	}
	
	void calcForeFeedback()
    {
        // If controller has forcefeedback. Calculate force and send to controller
        if (dinput.HasForceFeedback(dinput.currentDeviceID))
        {
            float Mz = 0f;
            Mz += CalcMz(wheelFrontLeft);
            Mz += CalcMz(wheelFrontRight);
			//print(Mz);
            dinput.SetForceCoordinate(dinput.currentDeviceID, Mz * ffbScale);
        }
    }
//taken from the forcefeedback class, then modifed since the wheel code doesnt use wheel colliders, and does many of these calcs already
    // Calculate Aligning force using Pacejka formula.    
	 private float CalcMz(Wheel w)
    {
        if (!w.onGround) 
			return 0f;
		
		float Mz;
        float FzSquared;
        float camber = 0.02f;
        float grip = w.grip;

        // Get Wheel Velocity in real world
        float sideSlip = w.slipVelo;

        // Get Downward force
        float Fz = Mathf.Clamp(w.getForceOfGroundWheelCollision(), 0, rigidbody.mass * 9.8f) * FzFactor;


        // Calculate derived coefficients
        FzSquared = Fz * Fz;
        float C = c[0];
        float D = (c[1] * FzSquared + c[2] * Fz) * grip;
        float E = (c[7] * FzSquared + c[8] * Fz + c[9]) * (1.0f - c[10] * Mathf.Abs(camber));
        float B = ((c[3] * FzSquared + c[4] * Fz) * (1 - c[6] * Mathf.Abs(camber)) * Mathf.Exp(-c[5] * Fz)) / (C * D);
        float Sh = c[11] * camber + c[12] * Fz + c[13];
        float Sv = (c[14] * FzSquared + c[15] * Fz) * camber + c[16] * Fz + c[17];

        // Calculate aligning force
        Mz = D * Mathf.Sin(C * fastAtan(B * (1.0f - E) * (sideSlip + Sh) + E * fastAtan(B * (sideSlip + Sh)))) + Sv;
        return Mz;
    }

    private float fastAtan(float x)
    {
        float absx = Mathf.Abs(x);
        return x * (1f + 1.1f * absx) / (1f + 2 * (1.6f * absx + 1.1f * x * x) / Mathf.PI);
    }

}
////force feedback calc stuff
    //#----------	Aligning moment
    //c0=2.2			#	Shape factor   C0
    //c1=-4.3			#	Load influence of peak value   (Nm/kN**2)   C1
    //c2=-4.4			#	Load influence of peak value   (Nm/kN)   C2
    //c3=-1.9			#	Curvature factor of stiffness   (Nm/deg/kN**2) C3
    //c4=-9.6			#	Change of stiffness with load at load = 0 (Nm/deg/kN)   C4
    //c5=0.0225			#	Change of progressivity of stiffness/load (1/kN)   C5
    //c6=0				#	Camber influence on stiffness   (%/deg/100)   C6
    //c7=0.044			#	Curvature change with load   C7
    //c8=-0.58			#	Curvature change with load   C8
    //c9=0.18			#	Curvature at load = 0   C9
    //c10=0.043			#	Camber influence of stiffness   C10
    //c11=0.048			#	Camber influence on horizontal shift (deg/deg)  C11
    //c12=-0.0035		#	Load influence on horizontal shift (deg/kN)  C12
    //c13=-0.18			#	Horizontal shift at load = 0 (deg)  C13
    //c14=0.14			#	Camber influence on vertical shift (Nm/deg/kN**2) C14
    //c15=-1.029		#	Camber influence on vertical shift (Nm/deg/kN)  C15
    //c16=0.27			#	Load influence on vertical shift (Nm/kN)  C16
    //c17=-1.1			#	Vertical shift at load = 0 (Nm)  C17c0=2.2



 // GUI Display for Force Feedback testing
    /*void FFTest()
    {
        int p = 0;

        bumpyRoad = GUI.Toggle(ffRect[p], bumpyRoad, "Bumpy Road");
        if (bumpyRoad != oldBumpyRoad)
        {
            if (bumpyRoad) dinput.PlayBumpyRoadEffect(50);
            else dinput.StopBumpyRoadEffect();
            oldBumpyRoad = bumpyRoad;
        }

        p++;
        slipperyRoad = GUI.Toggle(ffRect[p], slipperyRoad, "Slippery Road");
        if (slipperyRoad != oldSlipperyRoad)
        {
            if (slipperyRoad) dinput.PlaySlipperyRoadEffect(50);
            else dinput.StopSlipperyRoadEffect();
            oldSlipperyRoad = slipperyRoad;
        }

        p++;
        damper = GUI.Toggle(ffRect[p], damper, "Damper Force");
        if (damper != oldDamper)
        {
            if (damper) dinput.PlayDamperForce(90);
            else dinput.StopDamperForce();
            oldDamper = damper;
        }

        p++;
        dirtRoad = GUI.Toggle(ffRect[p], dirtRoad, "Dirt Road");
        if (dirtRoad != oldDirtRoad)
        {
            if (dirtRoad) dinput.PlayDirtRoadEffect(50);
            else dinput.StopDirtRoadEffect();
            oldDirtRoad = dirtRoad;
        }


        p++;
        springForce = GUI.Toggle(ffRect[p], springForce, "Spring Force");
        if (springForce != oldSpringForce)
        {
            if (springForce) dinput.PlaySpringForce(30, 20, 80);
            else dinput.StopSpringForce();
            oldSpringForce = springForce;
        }

        p++;
        airbourne = GUI.Toggle(ffRect[p], airbourne, "Airbourne");
        if (airbourne != oldAirbourne)
        {
            if (airbourne) dinput.PlayAirbourne();
            else dinput.StopAirbourne();
            oldAirbourne = airbourne;
        }

        p++;
        constForce = GUI.Toggle(ffRect[p], constForce, "Constant Force");
        p++;
        force = GUI.HorizontalSlider(ffRect[p], force, -100f, 100f);
        if (force != oldForce || constForce != oldConstantForce)
        {
            if (constForce)
            {
                dinput.PlayConstantForce((int)force);
            }
            else
            {
                dinput.StopConstantForce();
            }
            oldConstantForce = constForce;
            oldForce = force;
        }

        p = 10;
        if (GUI.Button(ffRect[p], "Front Collision")) dinput.PlayFrontalCollisionForce(80);

        p++;
        if (GUI.Button(ffRect[p], "Side Collision")) dinput.PlaySideCollisionForce(-80);

        p++;
        GUI.Label(ffRect[p], "LED: ");
        p++;
        led = GUI.HorizontalSlider(ffRect[p], led, 0f, 5000f);
        if (led != oldLed)
        {
            dinput.SetLeds(led, 0, 5000);
            oldLed = led;
        }

    }*/
