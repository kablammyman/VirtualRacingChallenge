using UnityEngine;
using System.Collections;
using System.IO;
using System;


public class CGameData : MonoBehaviour
{

	//public CSaveManager saveFile;
	private static CGameData instance;
	protected char slash;
	
	public InputManager input;
	//PlayerPrefs.DeleteAll
	//public static int TEMP_SAVE_SLOT = 10;

	public bool startedGame = false;
	public bool gameOver = false;
	public int points = 0;
	
	public bool isAuto = true;
	public float percentageLevelLoaded = 0;
   //a singleton to keep all the game data between scenes
    public static CGameData Instance
    {
        get
        {
            if (instance == null)
            {
				//print("new CGameData obj");
				new GameObject("CGameData", typeof(CGameData));
				instance.init();
            }

            return instance;
        }
    }
//-----------------------------------------------------------------------------------------------------		
	void Start()
	{
		//print("client start");
		DontDestroyOnLoad(gameObject);
	}
	
	void Awake ()
	{
		// Only allow one instance of the API bridge
		//print("client awake");
		if (instance)
		{
			Debug.LogError ("Cannot have two instances of singleton. Self destruction in 3...");
            return;
			//Destroy(gameObject);
		}
		//else print("awake");
		instance = this;
	}
//----------------------------------------------------------------------------------------------- 	
	//public CGameData ()
	public void init ()
    {	
		startedGame = true;
		input = gameObject.GetComponent(typeof(InputManager)) as InputManager;
		if(input == null)//if we start the game from anywhere other than the title (for testing)
		{
			GameObject Singleton = new GameObject("Singleton");
			transform.parent = Singleton.transform;
			input = Singleton.AddComponent<InputManager>();
		}
		input.inputType = InputManager.InputType.JOYSTICK;
		input.ignoreControl = false; 
    }
//----------------------------------------------------------------------------------------------- 
    public void OnApplicationQuit ()
    {	
		instance = null;
    }
//----------------------------------------------------------------------------------------------- 
	public static Transform getChildObject(GameObject go, string name)
	{
		Transform[] allChildren = go.GetComponentsInChildren<Transform>();
		foreach (Transform child in allChildren) 
		{
			if(child.name == name)
			return child;
		}
		return null;
	}
}

