using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Object References")]
    public Camera cam;
    private Rigidbody rBody;
    //[SerializeField] private Animator playerAnimator;
    
    [Header("Move Parameters")]
    public float moveSpeed;
    private float speed;
    
    private Vector3 movement;
    
    //Mouse look
    private Vector3 lookDirection;
    private Quaternion lookRotation;
    private Vector3 mousePos;
    [HideInInspector] public Vector3 movementDirection;

    private bool canDash = true;

    
    public float dashSpeed;
    public float dashTime;
    public float dashCooldown;
    private float dashTimer;
    public bool isDashing;

    //public GameObject dashVfx;
    

    public Vector3 MousePos
    {
        get
        {
            return mousePos;
        }
    }
    
 

    private void OnEnable()
    {
        //dashVfx.SetActive(false);
        speed = moveSpeed;
        canDash= true;
    }

    private void OnFootstep()
    {
    }

    private void Awake()
    {
        rBody = GetComponent<Rigidbody>();
        //playerAnimator = GetComponentInChildren<Animator>();
        
    }

private void FixedUpdate()
{
    if (isDashing)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, movement.normalized, out hit, movement.magnitude * speed * Time.fixedDeltaTime,Physics.AllLayers,QueryTriggerInteraction.Ignore))
        {
            if (!hit.collider.CompareTag("Enemy"))
            {
                // If there is a collision, stop the player's movement
                movement = Vector3.zero;
            }
            return;
        }
    }


    // If there is no collision, move the player
    rBody.MovePosition(rBody.position + movement.normalized * speed * Time.fixedDeltaTime);
    rBody.MoveRotation(lookRotation.normalized);
    
    UpdateSound();
}

    // Update is called once per frame
    void Update()
    {
        //playerAnimator.SetFloat("speed", movementDirection.magnitude);
        
        PointPlayerTowardsMouse();

      
    }

    public void GetMovementInput(Vector3 movement)
    {
        this.movement = movement;
        movementDirection = movement.normalized;
        
        float forwardBackwardsMagnitude = 0;
        float rightLeftMagnitude = 0;
        if (this.movement.magnitude > 0) {
            //Vector3 normalizedLookingAt = direction - transform.position;
            //normalizedLookingAt.Normalize ();
            forwardBackwardsMagnitude = Mathf.Clamp (
                Vector3.Dot (this.movement, lookDirection), -1, 1
            );
 
            Vector3 perpendicularLookingAt = new Vector3 (
                lookDirection.z, 0, -lookDirection.x
            );
            rightLeftMagnitude = Mathf.Clamp (
                Vector3.Dot (this.movement, perpendicularLookingAt), -1, 1
            );
        }
 
        // update the animator parameters
        //playerAnimator.SetFloat ("yDirection", forwardBackwardsMagnitude);
        //playerAnimator.SetFloat ("xDirection", rightLeftMagnitude);
        
        
       
    }


    public void PointPlayerTowardsMouse()
    {
        if (Input.mousePosition.y < Screen.height && Input.mousePosition.y > 0 && Input.mousePosition.x < Screen.width && Input.mousePosition.x > 0)
        { 
            Plane plane = new Plane(Vector3.up, transform.position);
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            float distance;
            if (plane.Raycast(ray, out distance))
            {
                mousePos = ray.GetPoint(distance);
            }
            lookDirection = (mousePos - transform.position).normalized;
            lookRotation = Quaternion.LookRotation(lookDirection).normalized;
            
        }
       

    }
    //Audio

    public void Start()
    {
        //Note: 2 olika tutorials clashar..
        //playerFootsteps = AudioManager.instance.CreateInstance(playerAudio.footsteps);
       
    }

    private void UpdateSound()
    //Start footsteps event if the player has an x velocity and is on ground
    {
        if (rBody.velocity.x !=0)
        {
            //get the playback state
          
        } 
        //otherwise, stop the footsteps event
        else
        {
           
        }
    }
}
