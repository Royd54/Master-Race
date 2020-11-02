using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{

    //Assingables
    [SerializeField] private Transform _playerCam;
    [SerializeField] private Transform _orientation;

    //Other
    private Rigidbody _rb;

    //Rotation and look
    private float _xRotation;
    private float _sensitivity = 50f;
    private float _sensMultiplier = 1f;

    //Movement
    [SerializeField] private float _moveSpeed = 4500;
    [SerializeField] private float _maxSpeed = 20;
    [SerializeField] private bool _grounded;
    [SerializeField] private LayerMask _whatIsGround;

    [SerializeField] public float _counterMovement = 0.175f;
    [SerializeField] private float _maxSlopeAngle = 35f;
    private float _threshold = 0.01f;

    //Input
    float x, y;
    bool jumping, sprinting, crouching, isCrouching;

    //Jumping
    private Vector3 _normalVector = Vector3.up;

    //Impacts/particles 
    private bool _highSpeedImpact = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        _playerScale = transform.localScale;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void Update()
    {
        Look();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 27.5f)
        {
            dustPuff.GetComponent<ParticleSystem>().startSpeed = collision.relativeVelocity.magnitude / 10;
            _highSpeedImpact = true;
        }
    }

    //Receives movement values from the PlayerInput class, and uses those values for the movement calculations
    public void MyInput(float receivedX, float receivedY, bool receivedJumping, bool receivedSprinting, bool receivedCrouching)
    {
        x = receivedX;
        y = receivedY;
        jumping = receivedJumping;
        crouching = receivedCrouching;
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

        //insert speed player feedback later
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
    }

    private void ResetJump()
    {
        _readyToJump = true;
    }

    private void ActivateGravity()
    {
        _rb.useGravity = true;
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
        _playerCam.transform.localRotation = Quaternion.Euler(_xRotation, desiredX, _wallRunCameraTilt);
        _orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!_grounded || jumping) return;

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
                    //play land particle here later
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

    private void StopGrounded()
    {
        _grounded = false;
    }

}
