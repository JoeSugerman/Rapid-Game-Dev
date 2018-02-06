using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower : MonoBehaviour {

    public List<Transform> path = new List<Transform>();
    public float speed = 5.0f;
    public float reachDistance = 1.0f;
    public int currentPoint = 0;
    public float rotationSpeed = 0.10f;

    // Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (currentPoint < path.Count)
        {

            //Rotate player to keep player looking foward - using next point in path
            Quaternion targetRotation = Quaternion.LookRotation(path[currentPoint].transform.position - transform.position);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);


            //move player along path - using points created on path
            //points should be created on 
            if (transform.rotation == targetRotation)
            {

                float dist = Vector3.Distance(path[currentPoint].position, transform.position);

                transform.position = Vector3.MoveTowards(transform.position, path[currentPoint].position, Time.deltaTime * speed);

                if (dist <= reachDistance)
                {
                    if (currentPoint >= path.Count-1)
                    {
                        currentPoint = 0;
                    }
                    else
                    {
                        currentPoint++;
                    }
                }

            }
        }
        
            
   }

  

}
