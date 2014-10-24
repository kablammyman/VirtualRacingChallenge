using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MyTimer : MonoBehaviour 
{
	public class Timer
	{
		public float endTime;
		public float curTime;
		public string name;
		public bool timerOn;
	}
	List<Timer> timers = new List<Timer>();
	
	// Update is called once per frame
	void Update () 
	{
		if(timers.Count <= 0)
			return;
		for(int i = 0; i < timers.Count; i++)
		{
			if(!timers[i].timerOn)
				continue;
			timers[i].curTime = (timers[i].endTime - Time.time);
			if(timers[i].curTime <= 0)
				timers[i].curTime = 0;
		}
	}
//------------------------------------------------------------------------------------		
	public void startTimer(string name)
	{
		//do something about timers with the same name at same time
		for(int i = 0; i < timers.Count; i++)
		{
			if(timers[i].name == name)
			{
				timers[i].timerOn = true;
				timers[i].endTime = Time.time + timers[i].curTime;
			}
		}
	}
//------------------------------------------------------------------------------------		
	public void pauseTimer(string name)
	{
		//do something about timers with the same name at same time
		for(int i = 0; i < timers.Count; i++)
		{
			if(timers[i].name == name)
				timers[i].timerOn = false;
		}
	}	
//------------------------------------------------------------------------------------		
	public void addTimer(float timeAmt, string name)
	{
		//do something about timers with the same name at same time
		Timer newTimer = new Timer();
		newTimer.name = name;
		newTimer.endTime = Time.time + timeAmt;
		newTimer.curTime = timeAmt;
		newTimer.timerOn = false;
		timers.Add(newTimer);
	}
//------------------------------------------------------------------------------------		
	public void resetTimer(float timeAmt, string name)
	{
		for(int i = 0; i < timers.Count; i++)		
			if(timers[i].name == name)
			{
				timers[i].name = name;
				timers[i].endTime = Time.time + timeAmt;
				timers[i].curTime = timeAmt;
				timers[i].timerOn = false;
			}
		
	}
//------------------------------------------------------------------------------------		
	public void incTimer(float timeAmt, string name)
	{
		for(int i = 0; i < timers.Count; i++)		
		if(timers[i].name == name)
		{
			//timers[i].endTime = Time.time + timeAmt;
			timers[i].endTime += timeAmt;
		}
		
	}

//------------------------------------------------------------------------------------		
	public float getTimer(string name)
	{
		for(int i = 0; i < timers.Count; i++)
		{
			if(timers[i].name == name)
				return timers[i].curTime;
		}
		return -1;
	}
//------------------------------------------------------------------------------------		
	public void removeTimer(string name)
	{
		for(int i = 0; i < timers.Count; i++)
		{
			if(timers[i].name == name)
				timers.RemoveAt(i);
		}
	}
//------------------------------------------------------------------------------------		
	public bool isTimeOver(string name)
	{
		bool timeOver = false;
		for(int i = 0; i < timers.Count; i++)
		{
			if(timers[i].name == name && timers[i].curTime == 0)
				timeOver = true;
		}
		return timeOver;
	}
}
