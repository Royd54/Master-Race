using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [SerializeField] private float _amount = 0.055f;
    [SerializeField] private float _maxAmount = 0.09f;

    float smooth = 3;
    Vector3 def;
    Vector3 defAth;
    Vector3 euler;

    GameObject ath;

    // Start is called before the first frame update
    void Start()
    {
        def = transform.localPosition;
        euler = transform.localEulerAngles;
    }
    float _smooth;

    // Update is called once per frame
    void Update()
    {
        _smooth = smooth;

        float factorX = -Input.GetAxis("Mouse X") * _amount;
        float factorY = -Input.GetAxis("Mouse Y") * _amount;

        if (factorX > _maxAmount)
            factorX = _maxAmount;

        if (factorX < -_maxAmount)
            factorX = -_maxAmount;

        if (factorY > _maxAmount)
            factorY = _maxAmount;

        if (factorY < -_maxAmount)
            factorY = -_maxAmount;

        Vector3 final = new Vector3(def.x + factorX, def.y + factorY, def.z);
        transform.localPosition = Vector3.Lerp(transform.localPosition, final, Time.deltaTime * _smooth);
    }
}
