using UnityEngine;
using System.Collections;

// This class is repsonsible for controlling inputs to the car.
// Change this code to implement other input types, such as support for analogue input, or AI cars.
[RequireComponent (typeof (Drivetrain))]
public class CarController : MonoBehaviour {

	// Add all wheels of the car here, so brake and steering forces can be applied to them.
	public Wheel[] wheels;
	
	// A transform object which marks the car's center of gravity.
	// Cars with a higher CoG tend to tilt more in corners.
	// The further the CoG is towards the rear of the car, the more the car tends to oversteer. 
	// If this is not set, the center of mass is calculated from the colliders.
	public Transform centerOfMass;

	// A factor applied to the car's inertia tensor. 
	// Unity calculates the inertia tensor based on the car's collider shape.
	// This factor lets you scale the tensor, in order to make the car more or less dynamic.
	// A higher inertia makes the car change direction slower, which can make it easier to respond to.
	public float inertiaFactor = 1.5f;
	
	float throttleInput;
	//float lastShiftTime = -1;
	float throttle = 0;
	float brake = 0;
	
	// cached Drivetrain reference
	Drivetrain drivetrain;
	
	// How long the car takes to shift gears
	public float shiftSpeed = 0.8f;
	

	// These values determine how fast throttle value is changed when the accelerate keys are pressed or released.
	// Getting these right is important to make the car controllable, as keyboard input does not allow analogue input.
	// There are different values for when the wheels have full traction and when there are spinning, to implement 
	// traction control schemes.
		
	// How long it takes to fully engage the throttle
	
	// when the wheels are spinning (and traction control is disabled)
	public float throttleTimeTraction = 10.0f;
	// How long it takes to fully release the throttle

	// when the wheels are spinning.
	public float throttleReleaseTimeTraction = 0.1f;

	// Turn traction control on or off
	public bool tractionControl = true;
	
	
	// These values determine how fast steering value is changed when the steering keys are pressed or released.
	// Getting these right is important to make the car controllable, as keyboard input does not allow analogue input.
	
	// How long it takes to fully turn the steering wheel from center to full lock
	public float steerTime = 1.2f;
	// This is added to steerTime per m/s of velocity, so steering is slower when the car is moving faster.
	public float veloSteerTime = 0.1f;

	// How long it takes to fully turn the steering wheel from full lock to center
	public float steerReleaseTime = 0.6f;
	// This is added to steerReleaseTime per m/s of velocity, so steering is slower when the car is moving faster.
	public float veloSteerReleaseTime = 0f;
	// When detecting a situation where the player tries to counter steer to correct an oversteer situation,
	// steering speed will be multiplied by the difference between optimal and current steering times this 
	// factor, to make the correction easier.
	public float steerCorrectionFactor = 4.0f;

	private InputManager controls;
	
	// Used by SoundController to get average slip velo of all wheels for skid sounds.
	public float slipVelo {
		get {
			float val = 0.0f;
			foreach(Wheel w in wheels)
				val += w.slipVelo / wheels.Length;
			return val;
		}
	}

	// Initialize
	void Start () 
	{
		if (centerOfMass != null)
			rigidbody.centerOfMass = centerOfMass.localPosition;
		rigidbody.inertiaTensor *= inertiaFactor;
		drivetrain = GetComponent (typeof (Drivetrain)) as Drivetrain;
		controls = transform.GetComponent<InputManager>();
		if(controls.inputType == InputManager.InputType.STEERINGWHEEL)
			drivetrain.ChangeGear(controls.getGear());
	}
	
	void Update () 
	{
		// Gear shifting
		//float shiftThrottleFactor = Mathf.Clamp01((Time.time - lastShiftTime)/shiftSpeed);

		throttle = controls.getThrottle();
		brake = controls.getBrake();

		if(drivetrain.automatic)
		{
			if(controls.inputType != InputManager.InputType.CPU)
			{
				if(brake > 0 && throttle == 0 )
				{
					if(drivetrain.gear == 0)
					{
						throttle = brake;
						brake = 0;
					}
					//only switch to reverse if we are hitting the brake and we are no moving
					if(drivetrain.gear != 0 && (Mathf.Sqrt((rigidbody.velocity.x*rigidbody.velocity.x) + (rigidbody.velocity.z*rigidbody.velocity.z)) < 1))
					{
						drivetrain.ChangeGear(-1);
					}
				}
				else
				{
					if(brake <= 0 && throttle > 0 && drivetrain.gear == 0)
						drivetrain.ChangeGear(1);
				}
			}
			else
			{
				if(controls.getGear() == -1)//if the cpu says change gears, do it!
				{
					drivetrain.ChangeGear(-1);
				}
			}
		}	
		else
		{
			if(drivetrain.useShiftLever && controls.inputType == InputManager.InputType.STEERINGWHEEL )
			{
				drivetrain.ChangeGear(controls.getGear());
			}
			else
			{
				if(controls.getGearUp())
				{
					//lastShiftTime = Time.time;
					drivetrain.ShiftUp ();
				}
				if(controls.getGearDown())
				{
					//lastShiftTime = Time.time;
					drivetrain.ShiftDown ();
				}
			}
		}

		drivetrain.waitToStart = controls.waitToStart;
		
		drivetrain.throttle = throttle;
		drivetrain.throttleInput = throttle;

		// Apply inputs
		foreach(Wheel w in wheels)
		{
			w.brake = brake;
			if(controls.getHandBrake())
				w.handbrake = 1;
			else
				w.handbrake = 0;
			
			//even if all 4 wheels get steering input, they can only be turned by thier steering angle, and rear wheels have 0 steering angle
			w.steering = controls.getSteeringWheel();
		}
	}
	
}
