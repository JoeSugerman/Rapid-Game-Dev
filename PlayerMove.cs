using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//handles player movement, utilising the CharacterMotor class
[RequireComponent(typeof(CharacterMotor))]
[RequireComponent(typeof(DealDamage))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    //setup
    public bool sidescroller;                   //if true, won't apply vertical input
    public Transform mainCam, floorChecks;      //main camera, and floorChecks object. FloorChecks are raycasted down from to check the player is grounded.
    public Animator animator;                   //object with animation controller on, which you want to animate
    public AudioClip jumpSound;                 //play when jumping
    public AudioClip landSound;                 //play when landing on ground
    public AudioClip concreteSound;             //play when walking on concrete
    public AudioClip metalSound;                //play when wlaking on metal
    public GameObject pressE;
    public List<NoiseSize> noisesMade = new List<NoiseSize>();
    public GameObject noiseSphere;              //object to display noise size on screen

    //movement
    public float accel = 70f;                   //acceleration/deceleration in air or on the ground
    public float airAccel = 18f;
    public float decel = 7.6f;
    public float airDecel = 1.1f;
    [Range(0f, 5f)]
    public float rotateSpeed = 0.7f, airRotateSpeed = 0.4f; //how fast to rotate on the ground, how fast to rotate in the air
    public float maxSpeed = 9;                              //maximum speed of movement in X/Z axis
    public float slopeLimit = 40, slideAmount = 35;         //maximum angle of slopes you can walk on, how fast to slide down slopes you can't
    public float movingPlatformFriction = 7.7f;             //you'll need to tweak this to get the player to stay on moving platforms properly

    //jumping
    public Vector3 jumpForce = new Vector3(0, 13, 0);       //normal jump force
    public Vector3 secondJumpForce = new Vector3(0, 13, 0); //the force of a 2nd consecutive jump
    public Vector3 thirdJumpForce = new Vector3(0, 13, 0);  //the force of a 3rd consecutive jump
    public float jumpDelay = 0.1f;                          //how fast you need to jump after hitting the ground, to do the next type of jump
    public float jumpLeniancy = 0.17f;                      //how early before hitting the ground you can press jump, and still have it work
    [HideInInspector]
    public int onEnemyBounce;

    //radius Control
    private bool jumped;
    public bool running;
    public bool sneaking;
    private int noiseCount = 0;
    private float noiseTimer = 0.1f;
    private const int MAX_NOISES = 5;
    private float jumpCount;
    private float volumeDelay;

    //sound variables
    public float sneakSoundLevel;
    public float normalSoundLevel;
    public float runSoundLevel;


    private int onJump;
    public bool grounded;
    private Transform[] floorCheckers;
    private Quaternion screenMovementSpace;
    private float airPressTime, groundedCount, curAccel, curDecel, curRotateSpeed, slope;
    private Vector3 direction, moveDirection, screenMovementForward, screenMovementRight, movingObjSpeed;

    private CharacterMotor characterMotor;
    private EnemyAI enemyAI;
    private DealDamage dealDamage;
    private Rigidbody rigid;
    public AudioSource aSource;
   

    public float h;
    public float v;

    private bool moving;

    //ladder climb variables
    public bool onLadder;
    public float climbSpeed;
    private float climbVelocity;
   

    //setup
    void Awake()
    {
        //create single floorcheck in centre of object, if none are assigned
        if (!floorChecks)
        {
            floorChecks = new GameObject().transform;
            floorChecks.name = "FloorChecks";
            floorChecks.parent = transform;
            floorChecks.position = transform.position;
            GameObject check = new GameObject();
            check.name = "Check1";
            check.transform.parent = floorChecks;
            check.transform.position = transform.position;
            Debug.LogWarning("No 'floorChecks' assigned to PlayerMove script, so a single floorcheck has been created", floorChecks);
        }
        //assign player tag if not already
        if (tag != "Player")
        {
            tag = "Player";
            Debug.LogWarning("PlayerMove script assigned to object without the tag 'Player', tag has been assigned automatically", transform);
        }
        //usual setup
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").transform;
        dealDamage = GetComponent<DealDamage>();
        characterMotor = GetComponent<CharacterMotor>();
        rigid = GetComponent<Rigidbody>();
        aSource = GetComponent<AudioSource>();
       
        pressE.SetActive(false);
        //gets child objects of floorcheckers, and puts them in an array
        //later these are used to raycast downward and see if we are on the ground
        floorCheckers = new Transform[floorChecks.childCount];
        for (int i = 0; i < floorCheckers.Length; i++)
            floorCheckers[i] = floorChecks.GetChild(i);

        volumeDelay = 0.2f;
        
        
    }

    //get state of player, values and input
    void Update()
    {
        //stops rigidbody "sleeping" if we don't move, which would stop collision detection
        rigid.WakeUp();
        //handle jumping
        JumpCalculations();
        //adjust movement values if we're in the air or on the ground
        curAccel = (grounded) ? accel : airAccel;
        curDecel = (grounded) ? decel : airDecel;
        curRotateSpeed = (grounded) ? rotateSpeed : airRotateSpeed;

        //get movement axis relative to camera
        screenMovementSpace = Quaternion.Euler(0, mainCam.eulerAngles.y, 0);
        screenMovementForward = screenMovementSpace * Vector3.forward;
        screenMovementRight = screenMovementSpace * Vector3.right;

        //get movement input, set direction to move in
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");


        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        //noise update for player when walking normally - 
        if (h != 0 && !running && !jumped && !sneaking)
        {
            if (noiseTimer < 0)
            {               
                GameObject tn = Instantiate(noiseSphere, transform.position, transform.rotation);
                NoiseSize tempNosie = tn.AddComponent(typeof(NoiseSize)) as NoiseSize;
                tn.tag = "Noise";
                tn.SetActive(true);
                tempNosie.lifeTime = 0.25f;
                noisesMade.Add(tempNosie);         
                noiseTimer = 0.5f;
                noiseCount++;
            }
            else
            {
                noiseTimer -= Time.deltaTime;
            }
           
        }
        else if (!running && !jumped && !sneaking)
        {           
            noiseTimer = 0.1f;
        }

        //use coroutine to check life of noise spheres
        StartCoroutine(CheckLife());
      
        //only apply vertical input to movemement, if player is not sidescroller
        if (!sidescroller)
            direction = (screenMovementForward * v) + (screenMovementRight * h);
        else
            direction = Vector3.right * h;
        moveDirection = transform.position + direction;


        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        //climb ladder
        if (onLadder) //will be true if player is in front of a ladder (determined by LadderClimb script
        {            
            if (v == 1) //if player is moving up the ladder
            {
                rigid.useGravity = false;                               //turn off gravity so player will move up
                climbVelocity = climbSpeed * v;                         //adjust climbing speed - can be updated in the inspector
                rigid.velocity = new Vector3(0, climbVelocity, 0);      //update velocity of ridigbody
            }
            else  
            {
                rigid.useGravity = true;                                //if player isn't moving turn gravity back on so player falls
            }
        }
        if (!onLadder) //check if player isn't on ladder and set gravity to true to ensure gravity is reset after coming off ladder
        {
            rigid.useGravity = true;                                    
        }       
    }

    private void LateUpdate()
    {
        UpdateSphere(); //using late update to check if player is trying to sneak/run and output the appropriate sound
    }

    //apply correct player movement (fixedUpdate for physics calculations)
    void FixedUpdate()
    {
        //are we grounded
        grounded = IsGrounded();
        //move, rotate, manage speed
        characterMotor.MoveTo(moveDirection, curAccel, 0.7f, true);
     
        if (rotateSpeed != 0 && direction.magnitude != 0)
            characterMotor.RotateToDirection(moveDirection, curRotateSpeed * 5, true);
        characterMotor.ManageSpeed(curDecel, maxSpeed + movingObjSpeed.magnitude, true);
        //set animation values
        if (animator)
        {
            animator.SetFloat("DistanceToTarget", characterMotor.DistanceToTarget);
            animator.SetBool("Grounded", grounded);
            animator.SetFloat("YVelocity", GetComponent<Rigidbody>().velocity.y);
        }

   
    }

    //prevents rigidbody from sliding down slight slopes (read notes in characterMotor class for more info on friction)
    void OnCollisionStay(Collision other)
    {
        //only stop movement on slight slopes if we aren't being touched by anything else
        if (other.collider.tag != "Untagged" || grounded == false)
            return;
        //if no movement should be happening, stop player moving in Z/X axis
        if (direction.magnitude == 0 && slope < slopeLimit && rigid.velocity.magnitude < 2)
        {
            //it's usually not a good idea to alter a rigidbodies velocity every frame
            //but this is the cleanest way i could think of, and we have a lot of checks beforehand, so it should be ok
            rigid.velocity = Vector3.zero;
        }
    }

    //returns whether we are on the ground or not
    //also: bouncing on enemies, keeping player on moving platforms and slope checking
    private bool IsGrounded()
    {
        //get distance to ground, from centre of collider (where floorcheckers should be)
        float dist = GetComponent<Collider>().bounds.extents.y;
        //check whats at players feet, at each floorcheckers position
        foreach (Transform check in floorCheckers)
        {
            RaycastHit hit;
            if (Physics.Raycast(check.position, Vector3.down, out hit, dist + 0.05f))
            {
                if (!hit.transform.GetComponent<Collider>().isTrigger)
                {
                    //slope control
                    slope = Vector3.Angle(hit.normal, Vector3.up);
                    //slide down slopes
                    if (slope > slopeLimit && hit.transform.tag != "Pushable")
                    {
                        Vector3 slide = new Vector3(0f, -slideAmount, 0f);
                        rigid.AddForce(slide, ForceMode.Force);
                    }
                 
                    if (hit.transform.tag == "MovingPlatform" || hit.transform.tag == "Pushable")
                    {
                        movingObjSpeed = hit.transform.GetComponent<Rigidbody>().velocity;
                        movingObjSpeed.y = 0f;
                        //9.5f is a magic number, if youre not moving properly on platforms, experiment with this number
                        rigid.AddForce(movingObjSpeed * movingPlatformFriction * Time.fixedDeltaTime, ForceMode.VelocityChange);
                    }
                    else
                    {
                        movingObjSpeed = Vector3.zero;
                    }
                    //yes our feet are on something
                    return true;
                }
            }
        }
        movingObjSpeed = Vector3.zero;
        //no none of the floorchecks hit anything, we must be in the air (or water)
        return false;
    }

    //jumping
    private void JumpCalculations()
    {
        //keep how long we have been on the ground
        groundedCount = (grounded) ? groundedCount += Time.deltaTime : 0f;

        //play landing sound
        if (groundedCount < 0.25 && groundedCount != 0 && !GetComponent<AudioSource>().isPlaying && landSound && GetComponent<Rigidbody>().velocity.y < 1)
        {
            //aSource.volume = Mathf.Abs(GetComponent<Rigidbody>().velocity.y) / 40;            
            aSource.volume = 1;
            aSource.Play();
        }

        //check if we hit the ground to change radius of noise
        if (groundedCount == 0)
        {
            jumped = true;

        }
        if (jumped)
        {
            if (groundedCount < 0.10 && groundedCount != 0) 
            {
                if (jumpCount == 0) // check if player has landed (not jumped)
                {
                    GameObject tn = Instantiate(noiseSphere, transform.position, transform.rotation);
                    NoiseSize tempNosie = tn.AddComponent(typeof(NoiseSize)) as NoiseSize;
                    tn.tag = "Noise";
                    tn.SetActive(true);
                    tempNosie.lifeTime = 0.5f;
                    noisesMade.Add(tempNosie);
                    noiseTimer = 0.5f;
                    jumpCount++;
                }
                
            }
            else if (groundedCount > 0.10)
            {                                
                jumped = false;
                jumpCount = 0;
            }
        }


        //if we press jump in the air, save the time
        if (Input.GetButtonDown("Jump") && !grounded)
            airPressTime = Time.time;

        //if were on ground within slope limit
        if (grounded && slope < slopeLimit)
        {
            //and we press jump, or we pressed jump justt before hitting the ground
            if (Input.GetButtonDown("Jump") || airPressTime + jumpLeniancy > Time.time)
            {
                //increment our jump type if we haven't been on the ground for long
                onJump = (groundedCount < jumpDelay) ? Mathf.Min(2, onJump + 1) : 0;
                //execute the correct jump (like in mario64, jumping 3 times quickly will do higher jumps)
                if (onJump == 0)
                    Jump(jumpForce);
                else if (onJump == 1)
                    Jump(secondJumpForce);
                else if (onJump == 2)
                {
                    Jump(thirdJumpForce);
                    onJump--;
                }
            }
        }
    }

    //push player at jump force
    public void Jump(Vector3 jumpVelocity)
    {
        if (jumpSound)
        {
            aSource.volume = 1;
            aSource.clip = jumpSound;
            aSource.Play();
        }
        rigid.velocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
        rigid.AddRelativeForce(jumpVelocity, ForceMode.Impulse);
        airPressTime = 0f;
    }


    //====================================================================================================================================================================================================0
    //Function that controls - speed if running/sneaking, creates noise spheres based on buttons input, updates noise clip and sound level
    public void UpdateSphere()
    {      
        if (Input.GetKey(KeyCode.LeftShift)&& h != 0 && !jumped) //if Left Shift is being pressed and user is moving and not jumping - user is running
        {
            if (noiseTimer < 0) //check noise timer has expried - noise timer used to ensure noise spheres are created appropriately spaced apart
            {
                accel = 100.0f; //update player acceleration
                maxSpeed = 100.0f; //update player max speed
           
                GameObject tn = Instantiate(noiseSphere, transform.position, transform.rotation); // create sphere game object at player's position
                NoiseSize tempNosie = tn.AddComponent(typeof(NoiseSize)) as NoiseSize;            // add nosie size script to the object to hold life of object - used for checking in checklife funciton
                tn.tag = "Noise";                                                                 // set sphere's tag to "nosie" for enemy detection
                tn.SetActive(true);                                                               // set shere is active
                tempNosie.lifeTime = 0.5f;                                                        // set lifetime - larger number will allow the sphere to grow larger
                noisesMade.Add(tempNosie);                                                        // add sphere to nosie made list for checking during check life function
                noiseTimer = 0.5f;                                                                // set noise timer - larger number will create larger delay between spheres being made
                noiseCount++;                                                                     // increase noise counter - for a reason i can't remember
            }
            else
            {
                noiseTimer -= Time.deltaTime;                                                     // if a noise shouldn't be made - reduce noise counter
            }

            if (volumeDelay < 0)                            // check if volume delay has expired - used to ensure sounds are played appropriately spaced apart
            {
                aSource.Play();                             // play audio source
                volumeDelay = 0.2f;                         // set volume delay so sound doesn't play continuously - larger number will create larger delay between sounds being played
            }
            else
            {
                volumeDelay -= Time.deltaTime;              // if a noise shouldn't be made - reduce volume delay
            }
            running = true;                                 
            aSource.volume = runSoundLevel;                 // set volume of audio source - can be set in the inspector - should be the loudest level as the player is running and making the most noise
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))  // check if left shift has been released - to reset variables back to walking speed
        {
            accel = 70.0f;
            maxSpeed = 20.0f;            
            running = false;
            aSource.volume = normalSoundLevel;
        }

        if (Input.GetKey(KeyCode.LeftControl) && h != 0 && !jumped) // if left control is being pressed and user is moving and not jumping - user is sneaking
        {
            if (noiseTimer < 0)  //if noise timer has expired
            {
                accel = 25.0f;
                maxSpeed = 25.0f;
    
                GameObject tn = Instantiate(noiseSphere, transform.position, transform.rotation);    //same as left shift
                NoiseSize tempNosie = tn.AddComponent(typeof(NoiseSize)) as NoiseSize;
                tn.tag = "Noise";
                tn.SetActive(true);
                tempNosie.lifeTime = 0.10f;
                noisesMade.Add(tempNosie);
                noiseTimer = 0.5f;
                noiseCount++;

            }
            else
            {
                noiseTimer -= Time.deltaTime;
            }
            if (volumeDelay < 0)
            {
                aSource.Play();
                volumeDelay = 0.5f;
            }
            else
            {
                volumeDelay -= Time.deltaTime;
            }
            sneaking = true;            
            aSource.volume = sneakSoundLevel;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {         
            accel = 70.0f;
            maxSpeed = 20.0f;
            sneaking = false;
            aSource.volume = normalSoundLevel;
        }
    }


    //=================================================================================================================
    //on trigger update clip to metal or concrete sound based on the tag of the object that triggers it
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "MetalGround")
        {
            aSource.clip = metalSound;
            Debug.Log("MetalSoundCollided");
        }
        if (other.gameObject.tag == "ConcreteGround")
        {
            aSource.clip = concreteSound;
            Debug.Log("Concerte Sound Collided");
        }
    }

    //=================================================================================================================
    //check life of noises made to see if they should be destoryed or continue to grow
    IEnumerator CheckLife()
    {
        foreach (NoiseSize noise in noisesMade)
        {
            if (noise.IsNoiseOver())
            {
                noisesMade.Remove(noise);
                Destroy(noise.gameObject);
                break;
            }
        }
        yield return new WaitForEndOfFrame();
    }
}
