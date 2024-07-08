using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    public GameObject rotationTarget;

    public float targetDoorRotation;
    // Start is called before the first frame update
    void Start()
    {
        rotationTarget.transform.Rotate(Vector3.up,targetDoorRotation);
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }
}
