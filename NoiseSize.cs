﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseSize : MonoBehaviour {

    public GameObject playerLocation;
    public float radius = 6.0f;
    public Collider[] collidersDetected;
    public LayerMask mask;
   

    // Use this for initialization
	void Start () {

		
	}

  	// Update is called once per frame
    void Update()
    {
        collidersDetected = Physics.OverlapSphere(playerLocation.transform.position, radius, mask);

        foreach (Collider col in collidersDetected)
        {
            col.gameObject.GetComponent<Renderer>().material.color = Color.blue;
        }
	}

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(playerLocation.transform.position, radius);
    }
}
