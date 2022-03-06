using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chestproperties : MonoBehaviour
{
    public GameObject Camera;

    // Start is called before the first frame update
    void Start()
    {// backs the chest look at player
        transform.LookAt(new Vector3(Camera.transform.position.x, transform.position.y, Camera.transform.position.z));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
