using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class mainMenu : MonoBehaviour {

	MouseLook headCam;

	Text curTitle;
	Text curMsg;
	Text Automatic;
	Text Manual;
	Image selector;
	int selecotrX = 6;//459
	bool inputTriggered = false;
	Vector2 selectorPos;

	public enum MainMenu
	{
		TITLE_SCREEN = 0,
		CHOOSE_CAR,
		CHOOSE_TRANS,
		START_GAME,
		CHOOSE_CONTROLLER,
		CHOOSE_SCREEN, //normal screen or oculus
		RESULTS
	};
	MainMenu menu;
	void Awake () 
	{
		CGameData.Instance.init();
		headCam = Camera.main.gameObject.GetComponent<MouseLook>();
		GameObject c = GameObject.Find("Canvas");
		
	
		Transform[] allChildren = c.GetComponentsInChildren<Transform>();
		foreach (Transform child in allChildren) 
		{
			//print(child.name + " " +child.GetType());
			if(child.name == "msg")
				curMsg = child.GetComponent<Text>();

			else if(child.name == "title")
				curTitle = child.GetComponent<Text>();
				
			else if(child.name == "ManualText")
				Manual = child.GetComponent<Text>();

			else if(child.name == "AutomaticText")
				Automatic = child.GetComponent<Text>();
				
			else if(child.name == "selector")
			{
				selector = child.GetComponent<Image>();
				selectorPos = selector.rectTransform.localPosition;
			}
				
		}
		transform.rotation = Camera.main.transform.rotation;
		
		if(CGameData.Instance.gameOver)
			menu = MainMenu.RESULTS;
		else
			menu = MainMenu.TITLE_SCREEN;
	}
	
	// Update is called once per frame
	void Update () 
	{
		//headCam.doMouseLook(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"));
		
		//print(CGameData.Instance.input.getLookX());
		if(CGameData.Instance.input == null)
			return;
		headCam.doMouseLook(transform.rotation,CGameData.Instance.input.getLookX(),CGameData.Instance.input.getLookY());
		
		doMenu();
		
	}
	bool accpet()
	{
		if(CGameData.Instance.input.getThrottle() >= .5f)
			inputTriggered = true;
		else if(CGameData.Instance.input.getThrottle() <.5f && inputTriggered)
		{
			inputTriggered = false;
			return true;
		}

		return false;
	}
	bool reject()
	{
		if(CGameData.Instance.input.getBrake() > .5f)
			return true;
		return false;
	}
	int decision()
	{
		if(CGameData.Instance.input.getSteeringWheel() >= .3f)	
			return 1;
		else if(CGameData.Instance.input.getSteeringWheel() <= -.3f)	
			return -1;
		return 0;
	}
	
	void doMenu()
	{
		switch(menu)
		{
			case MainMenu.TITLE_SCREEN:
			curTitle.text = "Virtual Racing Challenge";
			curMsg.text = "Hit the gas to begin.";
			Automatic.enabled = false;
			Manual.enabled = false;
			selector.enabled = false;
			CGameData.Instance.gameOver = false;
				if(accpet())
					menu = MainMenu.CHOOSE_TRANS;
			break;
			case MainMenu.CHOOSE_CAR:
				
			break;
			case MainMenu.CHOOSE_TRANS:
			
				curTitle.text = "Choose Transmission Type";
				curMsg.text = "Hit the gas to accept.";
				
				Automatic.enabled = true;
				Manual.enabled = true;
				selector.enabled = true;
				selecotrX = decision();
				if(selecotrX == 1)
				{
					selectorPos.x = Automatic.rectTransform.localPosition.x;
					CGameData.Instance.isAuto = true;
				}
				else if(selecotrX == -1)
				{
					selectorPos.x = Manual.rectTransform.localPosition.x;
					CGameData.Instance.isAuto = false;
				}
					
				selector.rectTransform.localPosition = selectorPos;
				if(accpet())	
					menu = MainMenu.START_GAME;
				
			break;
			case MainMenu.START_GAME:
				curTitle.text = "Are you ready?";
				curMsg.text = "Hit the gas to start the race!";
				Automatic.enabled = false;
				Manual.enabled = false;
				selector.enabled = false;
				
				if(CGameData.Instance.input.getThrottle() > .5f)
					Application.LoadLevel("CompleteScene");
			break;
			case MainMenu.CHOOSE_CONTROLLER:
			break;
			case MainMenu.CHOOSE_SCREEN:
			break;
			case MainMenu.RESULTS:
				Automatic.enabled = false;
				Manual.enabled = false;
				selector.enabled = false;
				curTitle.text = "Race Results";
				curMsg.text = "score: " + CGameData.Instance.points;
				
				if(accpet())
					menu = MainMenu.TITLE_SCREEN;
			break;
		}
	}
}
