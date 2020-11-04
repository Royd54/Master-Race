using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteTimer : MonoBehaviour
{
    [SerializeField] private float _timer = 5f;

    // Update is called once per frame
    void Update()
    {
        Destroy(gameObject, _timer);
    }
}
