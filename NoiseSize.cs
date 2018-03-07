using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseSize : MonoBehaviour {

    public float lifeTime;
       

    public bool IsNoiseOver()
    {
        if (lifeTime < 0) // check if life is over - if so return true so object will be destoryed
        {
            return true;
        }
        else
        {
            this.transform.localScale += new Vector3(1.0f, 1.0f, 1.0f); //if object is still alive, increase scale of nosie sphere
            lifeTime-=Time.deltaTime;                                   //decrease life
            return false;                                               //return false so object isn't destroyed
        }
    }
}
