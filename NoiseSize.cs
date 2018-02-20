using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseSize : MonoBehaviour {

    public float lifeTime;
    public GameObject noise; 
    


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

   

    public void IsNoiseOver()
    {
        if (lifeTime < 0)
        {
            Destroy(noise);
        }
        else
        {
            noise.transform.localScale += new Vector3(1.0f, 1.0f, 1.0f);
            lifeTime-=Time.deltaTime;
        }
    }
}
