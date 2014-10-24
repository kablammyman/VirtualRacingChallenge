using UnityEngine;
using System.Collections;

public class CarCamera : MonoBehaviour
{
	public Transform target = null;
	public float height = 1f;
	public float positionDamping = 3f;
	public float velocityDamping = 3f;
	public float distance = 4f;
	public LayerMask ignoreLayers = -1;

	public enum CamType
	{
		IN_CAR = 0,
		CHASE,
		SPECTATE
	};
	public CamType camType = CamType.IN_CAR;
	
	public bool inCarCam = true;
	public bool mouseLook = true;
	
	private RaycastHit hit = new RaycastHit();

	private Vector3 prevVelocity = Vector3.zero;
	private LayerMask raycastLayers = -1;
	
	private Vector3 currentVelocity = Vector3.zero;
	private Vector3 camPos = Vector3.zero;
	private GameObject camPosTransform;//i added a child obj to the car, this is to place cam in editor
	
	
	////mouse look stuff, basis of oculus code
	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;
 
    public float minimumX = -360F;
    public float maximumX = 360F;
 
    public float minimumY = -60F;
    public float maximumY = 60F;
 	
	Vector3 spectatePos;
	float spectateHeight = 5;
	bool passedCamera = true;//sued for spectate mode
    //Quaternion originalRotation;
	////
	RaceLogic gameScript;//need this for spectate camera
	MouseLook headCam;
	
	float inputX,inputY;
	void Start()
	{
		CGameData.Instance.init();
		raycastLayers = ~ignoreLayers;
		if(target == null)
			print("target is null");
		
		camPosTransform = target.parent.Find("camPos").gameObject;
		gameScript = target.parent.GetComponent(typeof(RaceLogic)) as RaceLogic;
		
		headCam = gameObject.GetComponent<MouseLook>();
		
		spectatePos = gameScript.checkPointPos;
		spectatePos.y += 15;
					
		if(camPosTransform == null)
			camPosTransform.transform.position = target.position;
			
			
		//oculus code
        //headCam.originalRotation = camPosTransform.transform.parent.rotation;
	}

	void FixedUpdate()
	{
		currentVelocity = Vector3.Lerp(prevVelocity, target.root.rigidbody.velocity, velocityDamping * Time.deltaTime);
		currentVelocity.y = 0;
		prevVelocity = currentVelocity;
	}
	
	public void setCamPos(GameObject newCamPos)
	{
		target = newCamPos.transform.parent.Find("CenterOfMass"); 
		gameScript = target.parent.GetComponent(typeof(RaceLogic)) as RaceLogic;
		camPosTransform = newCamPos;
		//headCam.originalRotation = camPosTransform.transform.parent.rotation;
	}
 
    /*static float ClampAngle (float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp (angle, min, max);
    }*/
	
	void LateUpdate()
	{
		
		float speedFactor = 1f;
		switch (camType)
		{
			case CamType.IN_CAR:
				camPos.x =-1f;
				camPos.y = -1f;
				camera.fieldOfView = Mathf.Lerp(55, 72, speedFactor);
				transform.position = camPosTransform.transform.position;
				transform.rotation = target.transform.rotation;
				
				if(mouseLook)
				{
					headCam.doMouseLook(transform.rotation,CGameData.Instance.input.getLookX(),CGameData.Instance.input.getLookY());
				}
			break;
			
			case CamType.CHASE:
				speedFactor = Mathf.Clamp01(target.root.rigidbody.velocity.magnitude / 70.0f);
				camPos.x = 7.5f;
				camPos.y = 6.5f;
				
				camPos.z = speedFactor;
			
				camera.fieldOfView = Mathf.Lerp(55, 72, speedFactor);
				float currentDistance = Mathf.Lerp(camPos.x,camPos.y,camPos.z);
				
				currentVelocity = currentVelocity.normalized;
				
				Vector3 newTargetPosition = target.position + Vector3.up * height;
				Vector3 newPosition = newTargetPosition - (currentVelocity * currentDistance);
				newPosition.y = newTargetPosition.y;
				
				Vector3 targetDirection = newPosition - newTargetPosition;
				if(Physics.Raycast(newTargetPosition, targetDirection, out hit, currentDistance, raycastLayers))
					newPosition = hit.point;
				
				transform.position = newPosition;
				transform.LookAt(newTargetPosition);
			break;
			case CamType.SPECTATE:
				
				float dist = Vector3.Distance(spectatePos, target.position);
				if(dist <= spectateHeight)
					passedCamera = true;
					
				if(dist > (spectateHeight+40) && passedCamera)
				{
					spectatePos = gameScript.checkPointPos;
					spectateHeight = Random.Range(gameScript.checkPointPos.y, gameScript.checkPointPos.y+25);
					spectatePos.y = spectateHeight;
					spectatePos.x = Random.Range(gameScript.checkPointPos.x-10, gameScript.checkPointPos.x+10);
					spectatePos.z = Random.Range(gameScript.checkPointPos.z-10, gameScript.checkPointPos.z+10);
					passedCamera = false;
				}
				transform.position = spectatePos; 
				transform.LookAt(target.position);
			break;
		}
		
		
	}
}



 
    
