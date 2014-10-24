using UnityEngine;
using System.Collections;

/// <summary>
/// This component is attached to a vehicle with wheel colliders and will calculate the aligning forces
/// on the front wheels which are then sent to the force feedback controller
/// </summary>
public class FFBGenerator : MonoBehaviour
{

    // Mapped to front left wheel, must have wheel collider
    public Transform wheelFrontLeft;

    // Mapped to front right wheel, must have wheel collider
    public Transform wheelFrontRight;

    // Force Feedback Scale
    public float ffbScale = 1f;

    // Downforce Factor
    public float FzFactor = 0.001f;

    // Controller linked to car
    public int controllerID = 0;

    WheelMenu wheelMenu;

    float[] c = { 2.2f, -3.9f, -3.9f, -1.26f, -8.2f, 0.025f, 0f, 0.044f, -0.58f, 0.18f, 0.043f, 0.048f, -0.0035f, -0.18f, 0.14f, -1.029f, 0.27f, -1.1f }; // aligning values
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


    // Save reference to WheelMenu
    void Start()
    {
        wheelMenu = Camera.main.GetComponent<WheelMenu>();
        wheelMenu.dinput.GetInput();
    }

    // Set the controller index for this vehicle
    public void SetControllerID(int index)
    {
        controllerID = index;
    }

    void Update()
    {
        // If controller has forcefeedback. Calculate force and send to controller
        if (wheelMenu.dinput.HasForceFeedback(controllerID))
        {
            float Mz = 0f;
            Mz += CalcMz(wheelFrontLeft);
            Mz += CalcMz(wheelFrontRight);
            wheelMenu.dinput.SetForceCoordinate(controllerID, Mz * ffbScale);
        }
    }

    // Calculate Aligning force using Pacejka formula.          
    private float CalcMz(Transform w)
    {
        float Mz;
        float FzSquared;
        WheelHit hit;

        // Default values are used for the camber and grip on the wheel.
        // For a better force feedback experience, accurate values should be set in 
        // these variables
        float camber = 0.02f;
        float grip = 1.9f;

        // Get Wheel Velocity in real world
        Rigidbody rigidBody = w.GetComponent<WheelCollider>().attachedRigidbody;
        Vector3 wheelV = rigidBody.GetPointVelocity(w.position);
        Vector3 localV = w.InverseTransformDirection(wheelV);
        float sideSlip = localV.x;

        // Get Downward force
        bool grounded = w.GetComponent<WheelCollider>().GetGroundHit(out hit);
        if (!grounded) return 0f;
        float hitForce = Mathf.Clamp(hit.force, 0, rigidbody.mass * 9.8f);
        float Fz = hitForce * FzFactor;


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
