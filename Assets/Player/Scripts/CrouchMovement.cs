using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchMovement : MonoBehaviour
{
    //Assingables
    [SerializeField] private Transform _orientation;
    private Rigidbody _rb;
    private PlayerMovement _playerMovement;

    //Other
    private float _timer = 0f;

    //Crouch & Slide
    [SerializeField] private float _slideForce = 400;
    [SerializeField] private float _slideCounterMovement = 0.2f;
    private Vector3 _crouchScale = new Vector3(1f, 1f, 1f);

    //Input
    public bool crouching, isCrouching;

    //Sliding
    private Vector3 _normalVector = Vector3.up;
    private Vector3 _wallNormalVector;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _playerMovement = GetComponent<PlayerMovement>();
    }

    public void StartCrouch()
    {
        isCrouching = true;
        transform.localScale = _crouchScale;
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        if (_rb.velocity.magnitude > 0.5f)
        {
            if (_playerMovement.GetGrounded() == true)
            {
                _rb.AddForce(_orientation.transform.forward * _slideForce);
            }
        }
    }

    public void StopCrouch()
    {
        isCrouching = false;
        transform.localScale = _playerMovement.getPlayerScale();
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
    }

    public IEnumerator Timer()
    {
        _playerMovement.SetCounterMovement(0f);
        yield return new WaitForSeconds(1f);
        _playerMovement.SetMaxSpeed(20f);
        _playerMovement.SetMaxSpeedCooldown(false);
        _playerMovement.SetCounterMovement(0.175f);
    }

    public float GetCrouchCountermovement()
    {
        return _slideCounterMovement;
    }
}
