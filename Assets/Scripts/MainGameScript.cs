using UnityEngine;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


//game checkl points 26, 54 (or 55) 
public class MainGameScript : MonoBehaviour {

	const string TIME_REMAINING = "timeRemaining";
	const string COUNT_DOWN = "countDown";
	const string TIME_EXTENDED = "timeExtended";
	const string TIME_TO_NEXT_SCENE = "nextScene";
	
	const float metersToMph = 2.23693629f;
	
	Text startText;
	Text bestTime;
	Text lapTime;
	Text lapNum;
	Text rank;
	
	Text curCheckpointObj;
	
	public int checkPointNum = 0;
	public GameObject parent;
	public string filePath = @"k:\checkPoints.txt";
	public List<GameObject> trackCheckpoints = new List<GameObject>();
//	public int[] gameCheckPoints = {26,54};
	
	//first item is the index of checkpoint, 2nd is the gameobject thats has the "checkpoint text"
	Dictionary<int, GameObject> gameCheckPoints;
	
	public GameObject[] cars;

	bool timeExtendedMessage = false;
	public bool  isCountDown = true;
	bool nextScene = false;
	
	MyTimer timers;
	
	public int gameOver = 0;
	
	string dash = "";
	TextMesh dashBoard;
	
	Camera mainCam;
	CarCamera camScript;
	char delimiter;
	int camIndex = 0;
	string gearString = "";
		
	Car curCarScript;//a pointer to a particular car we want to know about (mainly for the GUI)
	Drivetrain dt;
	RaceLogic[] carInfo;
	void Awake()
	{
		if(!CGameData.Instance.startedGame)
			CGameData.Instance.init();
			
		parent = new GameObject("checkPoints");
		readCheckpointFile(filePath, ':');
	
		timers = gameObject.AddComponent<MyTimer>();	
		
		timers.addTimer(4,COUNT_DOWN);
		timers.addTimer(3,TIME_TO_NEXT_SCENE);
		timers.addTimer(20,TIME_REMAINING);
		timers.startTimer(COUNT_DOWN);
		
		CGameData.Instance.points = 0;
		//textMesh = GameObject.Find ("Timer").GetComponent(TextMesh);
		//textMesh.text = "60";

	}
	void Start()
	{
		cars = GameObject.FindGameObjectsWithTag("car");
		carInfo = new RaceLogic[cars.Length];
		
		for(int i = 0; i < cars.Length; i++)
		{
			carInfo[i] = cars[i].GetComponent(typeof(RaceLogic)) as RaceLogic;
			if(cars[i].name == "Player1")
			{
				camIndex = i;
				carInfo[i].isPlayer = true;
				carInfo[i].getInput(CGameData.Instance.input, CGameData.Instance.isAuto);
				
				(cars[i].GetComponent(typeof(Drivetrain)) as Drivetrain).useTach = true;
			}
		}
		mainCam = Camera.main;
		camScript = mainCam.GetComponent(typeof(CarCamera)) as CarCamera;
		dashBoard = GameObject.Find("dashBoard").GetComponent(typeof(TextMesh)) as TextMesh;
		setCarCam(camIndex);
		
		getProperCarScript();
		
		GameObject temp = GameObject.Find("StartLine");
		startText = (CGameData.getChildObject(temp, "startText")).GetComponent<Text>();
		curCheckpointObj = startText;
		gameCheckPoints = new Dictionary<int, GameObject>{{0,temp},{26,GameObject.Find("cp1")},{54,GameObject.Find("cp2")}};
		
		temp = GameObject.Find("GUI");
		bestTime = (CGameData.getChildObject(temp, "bestTime")).GetComponent<Text>();
		lapTime = (CGameData.getChildObject(temp, "lapTime")).GetComponent<Text>();
		lapNum = (CGameData.getChildObject(temp, "lapNum")).GetComponent<Text>();
		rank = (CGameData.getChildObject(temp, "rank")).GetComponent<Text>();	
		
	}
//------------------------------------------------------------------------------------		
	void Update()
	{
		if (isCountDown)
		{
			if(timers.isTimeOver(COUNT_DOWN))
			{
				isCountDown = false;
				timers.removeTimer(COUNT_DOWN);
				timers.startTimer(TIME_REMAINING);
			}
		}
		
		if(timeExtendedMessage)
		{
			
			if(timers.isTimeOver(TIME_EXTENDED))
			{
				timeExtendedMessage = false;
				timers.removeTimer(TIME_EXTENDED);
			}
		}
				
		if(timers.getTimer(TIME_REMAINING) > 0)
			gameOver = 0;
		else if(timers.getTimer(TIME_REMAINING) <= 0 && gameOver== 0)
			gameOver = 1;
		if(gameOver == 2)
		{
			if(!nextScene)
			{
				timers.startTimer(TIME_TO_NEXT_SCENE);
				nextScene = true;
			}
			if(timers.isTimeOver(TIME_TO_NEXT_SCENE))
			{
				//print("end scene...");
			}
		}
			
		//textMesh.text = timeLeft.ToString();
	
		if (Input.GetKeyUp("space"))
		{
			if(camIndex < cars.Length-1)
				camIndex++;
			else
				camIndex = 0;
			
			setCarCam(camIndex);
			getProperCarScript();
		}
		figureOutRank();
		
		MyOnGUI();
	}
//------------------------------------------------------------------------------------		
	void MyOnGUI()
    {		
		if(isCountDown)
		{
			int c = (int)timers.getTimer(COUNT_DOWN);
			if(c > 0)
				startText.text = "ready: " +((int)timers.getTimer(COUNT_DOWN)).ToString();
			else
				startText.text = "GO!!";
		}
		else
			curCheckpointObj.text = "Checkpoint!\nTime: " + carInfo[camIndex].curLapTime.ToString();
		
		if(dt)
		{
			if(dt.gear == 0)
				gearString = "R";
			else if(dt.gear == 1)
				gearString = "N";
			else
				gearString = (dt.gear-1).ToString();
		}
		dash = "G: " + gearString;
		if (gameOver == 0)
		{
			dash += "\n"+((int)timers.getTimer(TIME_REMAINING)).ToString();
			lapTime.text = "Lap Time: " + carInfo[camIndex].curLapTime.ToString();
			bestTime.text = "Best Time: " + carInfo[camIndex].bestTime.ToString();
			lapNum.text = "laps: " + carInfo[camIndex].lapNum.ToString();
			rank.text = "Rank: " + carInfo[camIndex].rank.ToString();
			//GUI.Label(new Rect(Screen.width/2,Screen.height/8 ,50,50), "time: " + timers.getTimer("timeRemaining").ToString());
		}
		else if (gameOver == 1)
		{
			GUI.Label(new Rect(Screen.width/2,Screen.height/2 ,100,50), "out of gas...");	
		}
		else if (gameOver == 2)
		{
			timers.startTimer(TIME_TO_NEXT_SCENE);
			gameOver++;	
		}
		else if (gameOver >= 2)
			GUI.Label(new Rect(Screen.width/2,Screen.height/2 ,100,50), "Game Over");
			
		if(timers.isTimeOver(TIME_TO_NEXT_SCENE))
		{
			CGameData.Instance.gameOver = true;
			Application.LoadLevel("titleScreen");
		}
		dashBoard.text = dash;
		
		
		//if(timeExtendedMessage)
		//	GUI.Label(new Rect(Screen.width/2,Screen.height/2 ,100,50), "TIME EXTENDED");		
			
    }
//------------------------------------------------------------------------------------		
	public void checkForTimeExtension(int curCheckpoint)
	{
		if(gameCheckPoints.ContainsKey(curCheckpoint))
		{
			if(curCheckpoint == 26)
			{
				gameCheckPoints[54].SetActive(true);
				curCheckpointObj = (CGameData.getChildObject(gameCheckPoints[54], "cp2Text")).GetComponent<Text>();
			}
			else if(curCheckpoint == 54)
			{	
				gameCheckPoints[0].SetActive(true);
				curCheckpointObj = startText;
			}
			else if(curCheckpoint == 0)
			{
				gameCheckPoints[26].SetActive(true);
				curCheckpointObj = (CGameData.getChildObject(gameCheckPoints[26], "cp1Text")).GetComponent<Text>();
			}
			extendTime(30);
			CGameData.Instance.points += 500;
			return;
		}
		CGameData.Instance.points += 100;
	}	
//------------------------------------------------------------------------------------
	public void extendTime(float timeInc)
	{
		timers.incTimer(timeInc,TIME_REMAINING);
		print ("TIME EXTENDED");
		timers.addTimer(3,TIME_EXTENDED);
		timers.startTimer(TIME_EXTENDED);
		timeExtendedMessage = true;
	}
//------------------------------------------------------------------------------------		
	void getProperCarScript()
	{
		curCarScript = null;
		dt = null;
		curCarScript = cars[camIndex].GetComponent(typeof(Car)) as Car;
		if(curCarScript == null)
			dt = cars[camIndex].GetComponent(typeof(Drivetrain)) as Drivetrain; 
	}
//------------------------------------------------------------------------------------		
	void setCarCam(int index)
	{
		//print("start set car cam: " + index);
		Transform[] allChildren = cars[index].GetComponentsInChildren<Transform>();
		foreach (Transform child in allChildren) {
			if(child.name == "camPos")
			{
				camScript.setCamPos(child.gameObject);
				dashBoard.transform.parent = child.transform.parent;
			}
		}
		
	}
//------------------------------------------------------------------------------------	
	void figureOutRank()
	{
		for(int i = 0; i < carInfo.Length; i++)
		for(int j = 0; j < carInfo.Length; j++)
		{
			if(carInfo[i].lapNum == carInfo[j].lapNum)
			{
				if(carInfo[i].checkpointIndex > carInfo[j].checkpointIndex)
				{
					if(carInfo[i].rank >= carInfo[j].rank)
					{
						carInfo[i].rank--;
						carInfo[j].rank++;
					}
				}
				else if(carInfo[i].checkpointIndex == carInfo[j].checkpointIndex)
				{
					if(carInfo[i].distToCheckpoint < carInfo[j].distToCheckpoint)
					{
						if(carInfo[i].rank >= carInfo[j].rank)
						{
							if(carInfo[j].rank < cars.Length)
								carInfo[j].rank++;
							if(carInfo[i].rank > 1) 
								carInfo[i].rank--;
						}
					}
				}//end if check points are the same
			} //end if on same lap
			else if(carInfo[i].lapNum > carInfo[j].lapNum)
			{
				if(carInfo[i].rank >= carInfo[j].rank)
				{
					if(carInfo[j].rank < cars.Length)
						carInfo[j].rank++;
					if(carInfo[i].rank > 1) 
						carInfo[i].rank = carInfo[j].rank - 1; 
				} 
			}  
		}				 
	}
//------------------------------------------------------------------------------------	
//should we give checkpoints attributes, like speed up, in turn, slow down etc?	
	public void readCheckpointFile(string path, char delim)
	{
		delimiter = delim;
		string line = "";
		int lineNum = 1;

		if(!File.Exists(path))
		{
			Debug.LogError("file "+ path + " does not exist!");
			return;
		}
		try
		{
			using (StreamReader stream = new StreamReader(path))//reading normal text files...
			//using(System.IO.StringReader stream = new System.IO.StringReader(file))//read a giant string
            {
                
				while ((line = stream.ReadLine()) != null)
                {
					lineNum++;
					line = line.Trim(); //remove all white space before and after
					//print(line);
					//look for comments
					if(line.Length > 2)
					{
						if(line[0] == '/' && line[1] == '/')//dont proccess comments
							continue;
						else if(line.Contains("//")) //we have a comment somewhere else
						{
							int index = line.IndexOf('/'); //this shoudl awlays work since we found it exists alrady with Contains
							if(line[index+1] == '/') //if the next char after the first one is als a '/', ignore the rest
								line = line.Substring(0,index-1); //return everything before the comment
						}
					}
					
					string[] token = line.Split(delimiter);					
					GameObject checkpoint = new GameObject("checkPoint"+token[0]);
					checkpoint.transform.parent = parent.transform;
					
					string tempString = token[1].Trim();
					//remove parens
					int paren = tempString.IndexOf('(');
					tempString = tempString.Remove(paren, 1);
	
					paren = tempString.IndexOf(')');
					tempString = tempString.Remove(paren, 1);
					
					Vector3 pos;
					//print("token 1:" + token[0] + "\ntoken 2:"+tempString);
					string[] cords = tempString.Split(',');
					pos.x = float.Parse(cords[0]);
					pos.y = float.Parse(cords[1]);
					pos.z = float.Parse(cords[2]);
		
					checkpoint.transform.position = pos;
					trackCheckpoints.Add(checkpoint);
                }
            }
		}
		catch (Exception e)
		{
			Debug.LogError("error loading file: "+ path + "\nline number" + lineNum+ "\nError: "+e);
		}
	}

}
