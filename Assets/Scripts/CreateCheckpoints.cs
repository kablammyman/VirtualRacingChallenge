using UnityEngine;
using System.Collections;
using System.IO;


public class CreateCheckpoints : MonoBehaviour {

    public int checkPointNum = 0;
	public GameObject parent;
	FileStream fileStream;
	string filePath = @"k:\checkPoints.txt";
	void Start()
	{
		parent = new GameObject();
		fileStream = new FileStream(filePath, FileMode.Open);
		fileStream.Close();
	}
	
    void Update() 
	{
        if (Input.GetKeyUp("space"))
		{
            checkPointNum++;
			GameObject checkpoint = new GameObject();
			checkpoint.transform.parent = parent.transform;
			checkpoint.transform.position = transform.position;
			File.AppendAllText(filePath,checkPointNum + ":" +checkpoint.transform.position+"\n");
			print("check point " + checkPointNum + " at " +checkpoint.transform.position);
		}
        
    }
	
}