using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//a class that will hold time info and update with a tick
//we can keep track of any time variable. from nano seconds to centuries...as long as the lowest tick amt is correct
public class TimeKeeper 
{
	public struct TimeDigit
	{
		public int maxDigit;
		public int digit;
		public TimeDigit(int max)
		{
			maxDigit = max;
			digit = 0;
		}
		//if we return true, thats a signal to update the other digits
		public bool increment(int amt = 1)
		{
			digit+=amt;
			if(digit >= maxDigit)
			{
				digit -= maxDigit;
				return true;
			}
			return false;
		}
		public bool decrement(int amt = 1)
		{
			digit-=amt;
			
			if(digit <= 0)
			{
				digit += maxDigit;
				return true;
			}
			return false;
		}
	};

	int numDigits = 0;
	TimeDigit[] digits;
	bool countUp = true;
	
	public  TimeKeeper (int numDigits, int[] maxValues) 
	{
		this.numDigits = numDigits;
		digits = new TimeDigit[numDigits];
		for(int i = 0; i < numDigits; i++)
		{
			digits[i] = new TimeDigit(maxValues[i]);
		}
	}
	
	public string getTimeString()
	{
		string returnString = "";
		for(int i = 0; i < numDigits; i++)
		{
			returnString += (digits[i].digit + " ");
		}
		return returnString;
	}
	
	public void tick(int amt = 1)
	{
		if(countUp)
		{
			if(digits[numDigits-1].increment(amt))//if we are not resetting the digit, dont inc the other digits!
				for(int i = numDigits-2; i > -1; i--)
				{
					if(!digits[i].increment())
						break;
				}
		}
		else
		{
			if(digits[numDigits-1].decrement(amt))//if we are not resetting the digit, dont inc the other digits!
				for(int i = numDigits-2; i > -1; i--)
				{
					if(!digits[i].decrement())
					break;
				}
		}
	}
	
}
