/*this is a script al cars will have, so that we can see:
1) lap timer
2) current rank
3) and no short cuts on track (using check points)
4) best time
and anything else that i can think of*/


using UnityEngine;
using System.Collections;

public class RaceLogic : MonoBehaviour {
	
	public int checkpointIndex = 0;
	public float distToCheckpoint = 0;
	public float minDistFromChecpoint = 25;
	GameObject curCheckPoint;
	public Vector3 checkPointPos;
	
	public int points = 0;
	
	public int lapNum = 1;
	public int rank = 1;
	
	GameObject mainGame;
	MainGameScript gameScript;
	Drivetrain dt;
	InputManager input;
	
	public float curLapTime = 0;
	float lapTime = 0;
	public float bestTime = 0;
	
	public bool isPlayer = false;
	//int[] timeDigits = {59,59,99};
	//TimeKeeper timer;
	
	void Awake()
	{
			//timer = new TimeKeeper(3, timeDigits);
		mainGame = GameObject.Find("game");
		gameScript = mainGame.GetComponent(typeof(MainGameScript)) as MainGameScript;
		input = gameObject.GetComponent(typeof(InputManager)) as InputManager;
		dt = gameObject.GetComponent(typeof(Drivetrain)) as Drivetrain;
		
	}
	void Start () 
	{		
		curCheckPoint = gameScript.trackCheckpoints[checkpointIndex];
		checkPointPos = curCheckPoint.transform.position;
		
		if(isPlayer)
		{
			minDistFromChecpoint = 35;
		}
		
	}
	
	void Update () 
	{
		
		if(gameScript.isCountDown)
			return;
	
	//like in rad racer, if we limp across a check point, lets award the driver!	
		if(isPlayer)
		{
			if(gameScript.gameOver == 1)
			{
				input.ignoreThrottle = true;
				if(dt.speed <= 1)
				{
					gameScript.gameOver = 2;
					input.ignoreControl = true;
				}
			}
			else if(gameScript.gameOver == 0)
				input.ignoreThrottle = false;
		}	
		input.waitToStart = false;
		//print(name + " checkpointIndex = " + (checkpointIndex + 1) + " out of " + gameScript.trackCheckpoints.Count);
		distToCheckpoint = Vector3.Distance(checkPointPos, transform.position);
		curLapTime += Time.deltaTime;
		//int temp =  (int)((Time.deltaTime - (int)Time.deltaTime) * 100); 
		//timer.tick(temp);

		
		
		if( distToCheckpoint <= minDistFromChecpoint )
		{			
			if(isPlayer)
			{
				gameScript.checkForTimeExtension(checkpointIndex);
			}
			
			if(checkpointIndex < gameScript.trackCheckpoints.Count-1)
				checkpointIndex++;
			else 
			{
				checkpointIndex = 0;
				lapNum++;
				curLapTime = 0;
				setBestTime();
			}
			curCheckPoint = gameScript.trackCheckpoints[checkpointIndex];
			checkPointPos = curCheckPoint.transform.position;
		}
	}
	public void getInput(InputManager i, bool isAuto)
	{
		input.inputType = i.inputType;
		dt.automatic = isAuto;
	}
	//used when we get stuck...slow method since we check all of them
	//we can take a short cut, and check the check points we were last dealing with, but i want to be through
	public void findNeastCheckpoint(bool useNearest = true)
	{
		float curClosestDist = 100000;
		int i = 0;
		
		if (useNearest)
		{
			if(checkpointIndex > 0)
				i = checkpointIndex;
		}
		for(i = 0; i < gameScript.trackCheckpoints.Count; i++)
		{
			float temp = Vector3.Distance(gameScript.trackCheckpoints[i].transform.position, transform.position);
			if(temp < curClosestDist)
			{
				curClosestDist = temp;
				checkpointIndex = i;
			}
		}
		curCheckPoint = gameScript.trackCheckpoints[checkpointIndex];
	}
	
	void setBestTime()
	{
		float curTime = Time.time - lapTime;
		if(bestTime < 0)
			bestTime = curTime;
		else
		{
			if(curTime < bestTime)
			{
				bestTime = curTime;
				print("new lap record!");
			}
		}
		print(name + " lap: " + curTime + " best: " + bestTime);
		//resetLap timer
		lapTime = Time.time;
	}
}
