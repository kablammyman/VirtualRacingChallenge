using UnityEngine;
using System.Collections;

public class FFBCarController : MonoBehaviour
{

    public int controllerID = 0;
    public float forwardSpeed = 5000f;
    public float turnspeed = 2f;
    private WheelMenu wheelMenu;

    void Start()
    {

        // Save reference to wheelMenu component
        wheelMenu = Camera.main.GetComponent<WheelMenu>();
    }

    void Update()
    {

        // Get current state of controller for this player
        wheelMenu.dinput.GetInput(controllerID);

        // Using controllers throttle and brake values, calculate force to apply to car
        float accellerator = (wheelMenu.dinput.GetThrottle(controllerID) - wheelMenu.dinput.GetBrake(controllerID)) * forwardSpeed;

        // Using controllers steering value, calculate amount to turn
        float steer = wheelMenu.dinput.GetSteer(controllerID, 1) * turnspeed;

        // Update car with calculated values
        transform.Rotate(0, steer, 0);
        rigidbody.AddRelativeForce(accellerator, 0, 0);
    }

    void OnCollisionEnter(Collision collision)
    {
        // If player hits ball, update their score
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.otherCollider.name.Length > 3)
                if (contact.otherCollider.name.Substring(0, 3) == "bal")
                    Camera.main.GetComponent<FFBGame>().AddPoint(controllerID);
        }

        // Play a vibration if player hits something hard
        //if (collision.relativeVelocity.magnitude > 2)
        //    print("Play vibration");

    }

}
