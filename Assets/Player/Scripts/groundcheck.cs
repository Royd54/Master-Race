using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class groundcheck : MonoBehaviour
{
    public bool grounded = false;

    private void OnCollisionEnter(Collision collision)
    {
        grounded = true;
    }
}
