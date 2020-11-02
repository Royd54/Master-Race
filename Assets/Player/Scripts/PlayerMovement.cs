using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour {

    //Assingables
    [SerializeField] private Transform _playerCam;
    [SerializeField] private Transform _orientation;
    [SerializeField] private PhysicMaterial _physicMaterial;
    private Rigidbody _rb;
    
    //Rotation and look
    private float _xRotation;
    private float _sensitivity = 50f;
    private float _sensMultiplier = 1f;

    //Movement
    [SerializeField] private float _moveSpeed = 4500;
    [SerializeField] private float _maxSpeed = 20;
    [SerializeField] bool _maxSpeedCooldown = false;
    [SerializeField] private bool _grounded;
    [SerializeField] private LayerMask _whatIsGround;

    [SerializeField] public float _counterMovement = 0.175f;
    [SerializeField] private float _maxSlopeAngle = 35f;
    private float _threshold = 0.01f;
    private Vector3 _playerScale;

    //Jumping
    [SerializeField] private float _jumpForce = 550f;
    private Vector3 _normalVector = Vector3.up;
    private bool _readyToJump = true;
    private float _jumpCooldown = 0.25f;

    //Input
    float x, y;
    bool jumping, sprinting, crouching;
    
    //Particles
    [SerializeField] private ParticleSystem _ps;
    [SerializeField] private ParticleSystem.EmissionModule _em;
    private bool _highSpeedImpact = false;
    private ParticleSystem dustParticle;
    public GameObject dustPuff;

    //sounds
    [SerializeField] private AudioClip Walk;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    
    void Start()
    {
        _playerScale =  transform.localScale;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _physicMaterial.dynamicFriction = 0;
        _physicMaterial.staticFriction = 0;
    }
    
    private void FixedUpdate()
    {
        Movement();
    }

    private void Update()
    {
        Look();
        GetComponent<WallrunMovement>().CheckForWall();
        GetComponent<WallrunMovement>().WallRunInput();
        SoundCheck();
    }

    public void SoundCheck()
    {
        if (_rb.velocity.magnitude > 15 && _grounded == true && GetComponent<CrouchMovement>().isCrouching == false && AudioManager.Instance.WalkingSFXIsPlaying() == false ) AudioManager.Instance.PlayWalkingSFX(Walk);
    }

    public void CounterMovementSetter()
    {
        _counterMovement = 0;
        _maxSpeed = 50f;
        _maxSpeedCooldown = true;
        StartCoroutine(GetComponent<CrouchMovement>().Timer());
    }

    //Receives movement values from the PlayerInput class, and uses those values for the movement calculations
    public void MyInput(float receivedX, float receivedY, bool receivedJumping, bool receivedSprinting, bool receivedCrouching)
    {
        x = receivedX;
        y = receivedY;
        jumping = receivedJumping;
        crouching = receivedCrouching;

        //Crouching
        if (Input.GetKeyDown(KeyCode.LeftControl) && receivedCrouching)
            GetComponent<CrouchMovement>().StartCrouch();
        if (Input.GetKeyUp(KeyCode.LeftControl))
            GetComponent<CrouchMovement>().StopCrouch();
    }

    private void Movement()
    {
        //Extra gravity
        _rb.AddForce(Vector3.down * Time.deltaTime * 10);
        
        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(x, y, mag);
        
        //If holding jump && ready to jump, then jump
        if (_readyToJump && jumping) Jump();
        
        //If sliding down a ramp, add force down so player stays grounded and also builds speed
        if (crouching && _grounded && _readyToJump)
        {
            _rb.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (x > 0 && xMag > _maxSpeed) x = 0;
        if (x < 0 && xMag < -_maxSpeed) x = 0;
        if (y > 0 && yMag > _maxSpeed) y = 0;
        if (y < 0 && yMag < -_maxSpeed) y = 0;

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f;
        
        // Movement in air
        if (!_grounded)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }

        // Movement while sliding
        if (_grounded && crouching) multiplierV = 0f;

        //Apply forces to move player
        _rb.AddForce(_orientation.transform.forward * y * _moveSpeed * Time.deltaTime * multiplier * multiplierV);
        _rb.AddForce(_orientation.transform.right * x * _moveSpeed * Time.deltaTime * multiplier);

        //Makes particles that give player speed feedback
        _em = _ps.emission;

        if (_rb.velocity.magnitude > 30)
        {
            _em.rateOverTime = _rb.velocity.magnitude * 2.5f;
            if (_ps.emissionRate > 1000f)
            {
                _em.rateOverTime = 1000f;
            }
        }
        else
        {
            _em.rateOverTime = 0f;
        }
    }


    private void Jump()
    {
        if (_grounded && _readyToJump)
        {
            _readyToJump = false;

            //Add jump forces
            _rb.AddForce(Vector2.up * _jumpForce * 1.5f);
            _rb.AddForce(_normalVector * _jumpForce * 0.5f);

            //If jumping while falling, reset y velocity.
            Vector3 vel = _rb.velocity;
            if (_rb.velocity.y < 0.5f)
                _rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (_rb.velocity.y > 0)
                _rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

            Invoke(nameof(ResetJump), _jumpCooldown);
        }

        //Walljump
        if (GetComponent<WallrunMovement>().GetWallrunning())
        {
            _readyToJump = false;

            if (GetComponent<WallrunMovement>().GetWallrunningRight() && Input.GetKey(KeyCode.Space))
            {
                _rb.AddForce(Vector2.up * _jumpForce * 2f);
                _rb.AddForce(_normalVector * _jumpForce * 1f);
                _rb.AddForce(-_orientation.right * _jumpForce * 3.2f);
            }

            if (GetComponent<WallrunMovement>().GetWallrunningLeft() && Input.GetKey(KeyCode.Space))
            {
                _rb.AddForce(Vector2.up * _jumpForce * 2f);
                _rb.AddForce(_normalVector * _jumpForce * 1f);
                _rb.AddForce(_orientation.right * _jumpForce * 3.2f);
            }

            //Always add forward force
            _rb.AddForce(_orientation.forward * _jumpForce * 1f);

            Invoke(nameof(ResetJump), _jumpCooldown);
        }
    }

    private float desiredX;
    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * _sensitivity * Time.fixedDeltaTime * _sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * _sensitivity * Time.fixedDeltaTime * _sensMultiplier;

        //Find current look rotation
        Vector3 rot = _playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;

        //Rotate, and also make sure we dont over- or under-rotate.
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        //Perform the rotations
        _playerCam.transform.localRotation = Quaternion.Euler(_xRotation, desiredX, GetComponent<WallrunMovement>().GetWallrunCameraTilt());
        _orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);
    }

    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = _orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(_rb.velocity.x, _rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = _rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!_grounded || jumping) return;

        //Slow down sliding
        if (crouching)
        {
            _rb.AddForce(_moveSpeed * Time.deltaTime * -_rb.velocity.normalized * GetComponent<CrouchMovement>().GetCrouchCountermovement());
            return;
        }

        //Counter movement
        if (Math.Abs(mag.x) > _threshold && Math.Abs(x) < 0.05f || (mag.x < -_threshold && x > 0) || (mag.x > _threshold && x < 0))
        {
            _rb.AddForce(_moveSpeed * _orientation.transform.right * Time.deltaTime * -mag.x * _counterMovement);
        }
        if (Math.Abs(mag.y) > _threshold && Math.Abs(y) < 0.05f || (mag.y < -_threshold && y > 0) || (mag.y > _threshold && y < 0))
        {
            _rb.AddForce(_moveSpeed * _orientation.transform.forward * Time.deltaTime * -mag.y * _counterMovement);
        }
        
        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(_rb.velocity.x, 2) + Mathf.Pow(_rb.velocity.z, 2))) > _maxSpeed)
        {
            float fallspeed = _rb.velocity.y;
            Vector3 n = _rb.velocity.normalized * _maxSpeed;
            _rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < _maxSlopeAngle;
    }

    private bool cancellingGrounded;
    
    private void OnCollisionStay(Collision other)
    {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (_whatIsGround != (_whatIsGround | (1 << layer))) return;

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(normal))
            {
                if (_highSpeedImpact)
                {
                    GameObject dustObject = Instantiate(dustPuff, this.transform.position, this.transform.rotation) as GameObject;
                    dustParticle = dustObject.GetComponent<ParticleSystem>();                                                     
                    _highSpeedImpact = false;
                }

                _grounded = true;
                cancellingGrounded = false;
                _normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 27.5f)
        {
            dustPuff.GetComponent<ParticleSystem>().startSpeed = collision.relativeVelocity.magnitude / 10;
            _highSpeedImpact = true;
        }
    }

    private void ResetJump()
    {
        _readyToJump = true;
    }

    private void ActivateGravity()
    {
        _rb.useGravity = true;
    }

    private void StopGrounded()
    {
        _grounded = false;
    }
    
    public bool GetGrounded()
    {
        return _grounded;
    }

    public void SetCounterMovement(float amount)
    {
        _counterMovement = amount;
    }

    public void SetMaxSpeed(float amount)
    {
        _maxSpeed = amount;
    }

    public void SetMaxSpeedCooldown(bool trigger)
    {
        _maxSpeedCooldown = trigger;
    }

    public Vector3 getPlayerScale()
    {
        return _playerScale;
    }
}
