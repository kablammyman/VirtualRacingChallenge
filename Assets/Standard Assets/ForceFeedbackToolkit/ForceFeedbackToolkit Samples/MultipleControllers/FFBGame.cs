using UnityEngine;
using System.Collections;

public class FFBGame : MonoBehaviour
{

    public Transform player;
    public Transform target;

    private Transform[] players;
    private Transform ball;

    private int[] score;
    private WheelMenu wheelMenu;
    private bool playGame = false;

    void Start()
    {

        // Get reference to WheelMenu component
        wheelMenu = Camera.main.GetComponent<WheelMenu>();

        // If we have game controllers attached, then create players
        if (wheelMenu.dinput.DeviceCount > 0)
        {

            // Initialise arrays for keeping track of players
            players = new Transform[wheelMenu.dinput.DeviceCount];
            score = new int[wheelMenu.dinput.DeviceCount];
            playGame = true;

            // Create a player object for each controller
            for (int a = 0; a < wheelMenu.dinput.DeviceCount; a++)
            {
                score[a] = 0;
                players[a] = Instantiate(player, new Vector3(-10f + a * 4f, 1f, 0f), new Quaternion()) as Transform;
                players[a].GetComponent<FFBCarController>().controllerID = a;
                players[a].GetComponent<FFBGenerator>().SetControllerID(a);
            }

            // Create a target for the players to chase
            MoveBall();

        }

    }


    void OnGUI()
    {

        Rect hdg = new Rect(20, 10, 600, 40);
        GUI.Label(hdg, "Press 'M' to configure input mapping. Hit ball to score points.");

        // Display players and their scores
        if (playGame)
        {
            for (int a = 0; a < wheelMenu.dinput.DeviceCount; a++)
            {
                Rect scoreBox = new Rect(20, 30 + (a * 30), 300, 40);
                GUI.Label(scoreBox, wheelMenu.dinput.GetControllerName(a) + " (" + a + ")  " + score[a].ToString());
            }
        }
    }

    // Create ball for players to chase
    void MoveBall()
    {
        // Destroy ball
        if (ball != null) Destroy(ball.gameObject);
        // Instantiate ball in random location
        ball = Instantiate(target, new Vector3(Random.Range(-18f, 18f), 3f, Random.Range(-9f, 9f)), new Quaternion()) as Transform;
    }

    // A player has hit the ball
    public void AddPoint(int playerId)
    {
        // Increase players score
        score[playerId]++;

        // create next target
        MoveBall();
    }

}
