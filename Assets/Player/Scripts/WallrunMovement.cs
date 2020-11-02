using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WallrunMovement : MonoBehaviour
{
    //Assingables
    [SerializeField] private Transform _playerCam;
    [SerializeField] private Transform _orientation;
    private Rigidbody _rb;

    //Wallrunning
    [SerializeField] private LayerMask _whatIsWall;
    [SerializeField] private float _wallrunForce, _maxWallrunTime, _maxWallSpeed;
    [SerializeField] private float _maxWallRunCameraTilt, _wallRunCameraTilt;
    bool isWallRight, isWallLeft;
    bool isWallRunning;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        //While Wallrunning
        //Tilts camera in .5 second
        if (Math.Abs(_wallRunCameraTilt) < _maxWallRunCameraTilt && isWallRunning && isWallRight)
            _wallRunCameraTilt += Time.deltaTime * _maxWallRunCameraTilt * 2;
        if (Math.Abs(_wallRunCameraTilt) < _maxWallRunCameraTilt && isWallRunning && isWallLeft)
            _wallRunCameraTilt -= Time.deltaTime * _maxWallRunCameraTilt * 2;

        //Tilts camera back again
        if (_wallRunCameraTilt > 0 && !isWallRight && !isWallLeft)
            _wallRunCameraTilt -= Time.deltaTime * _maxWallRunCameraTilt * 2;
        if (_wallRunCameraTilt < 0 && !isWallRight && !isWallLeft)
            _wallRunCameraTilt += Time.deltaTime * _maxWallRunCameraTilt * 2;
    }

    public void WallRunInput() //Update
    {
        //Wallrun
        if (Input.GetKey(KeyCode.D) && isWallRight) StartWallrun();
        if (Input.GetKey(KeyCode.A) && isWallLeft) StartWallrun();
    }

    private void StartWallrun()
    {
        _rb.useGravity = false;
        isWallRunning = true;
        if (_rb.velocity.magnitude <= _maxWallSpeed)
        {
            _rb.AddForce(_orientation.forward * _wallrunForce * Time.deltaTime);

            //Make sure char sticks to wall
            if (isWallRight)
                _rb.AddForce(_orientation.right * _wallrunForce / 5 * Time.deltaTime);
            else
                _rb.AddForce(-_orientation.right * _wallrunForce / 5 * Time.deltaTime);
        }
    }

    private void StopWallRun()
    {
        isWallRunning = false;
        _rb.useGravity = true;
    }

    public void CheckForWall() //Update
    {
        isWallRight = Physics.Raycast(transform.position, _orientation.right, 1f, _whatIsWall);
        isWallLeft = Physics.Raycast(transform.position, -_orientation.right, 1f, _whatIsWall);

        //leave wall run
        if (!isWallLeft && !isWallRight) StopWallRun();
    }

    public bool GetWallrunning()
    {
        return isWallRunning;
    }

    public bool GetWallrunningRight()
    {
        return isWallRight;
    }

    public bool GetWallrunningLeft()
    {
        return isWallLeft;
    }

    public float GetWallrunCameraTilt()
    {
        return _wallRunCameraTilt;
    }
}
