using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupController : MonoBehaviour
{
    [SerializeField] private Transform _equipPos;
    [SerializeField] private float _distance = 10f;

    [SerializeField] private bool _canGrab;

    private GameObject _currentWeapon;
    private GameObject _wp;



    private void Update()
    {
        CheckWeapons();

        if (_canGrab)
        {
            if (Input.GetKey(KeyCode.E))
            {
                if (_currentWeapon != null)
                    Drop();

                PickUp();
            }
        }
        if (_currentWeapon != null)
        {
            if (Input.GetKey(KeyCode.Q))
                Drop();
        }

    }

    private void CheckWeapons()
    {
        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, _distance))
        {
            if (hit.transform.tag == "CanGrab")
            {
                _canGrab = true;
                _wp = hit.transform.gameObject;
            }
        }
        else _canGrab = false;
    }

    private void PickUp()
    {
        _currentWeapon = _wp;
        foreach (Transform trans in _currentWeapon.GetComponentsInChildren<Transform>(true))
        {
            trans.gameObject.layer = 12;
        }
        _currentWeapon.transform.position = _equipPos.position;
        _currentWeapon.transform.parent = _equipPos;
        _currentWeapon.transform.eulerAngles = _equipPos.transform.eulerAngles;
        _currentWeapon.GetComponent<Rigidbody>().isKinematic = true;
        _currentWeapon.GetComponent<ProjectileGun>().enabled = true;
    }

    private void Drop()
    {
        foreach (Transform trans in _currentWeapon.GetComponentsInChildren<Transform>(true))
        {
            trans.gameObject.layer = 0;
        }
        _currentWeapon.transform.parent = null;
        _currentWeapon.GetComponent<Rigidbody>().isKinematic = false;
        _currentWeapon.GetComponent<ProjectileGun>().enabled = false;
        _currentWeapon = null;
    }
}
