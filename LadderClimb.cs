using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderClimb : MonoBehaviour {

    public PlayerMove player;

    // Use this for initialization
    void Start()
    {
      

    }

    void OnTriggerEnter(Collider other) //placed on object you want to climb, will update player to be on ladder allowing vertical movmenet
    {
        if (other.gameObject.tag == "Player")
        {
            player.onLadder = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            player.onLadder = false;
        }
    }
}
