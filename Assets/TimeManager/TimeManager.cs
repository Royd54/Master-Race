using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private float _slowdownFactor = 0.05f;
    [SerializeField] private float _slowdownLength = 2f;

    // Start is called before the first frame update
    void Start()
    {
        GameEvents.instance.onSlomo += OnSlomo;
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale += (1f / _slowdownLength) * Time.unscaledDeltaTime;
        Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
    }

    private void OnSlomo()
    {
        Time.timeScale = _slowdownFactor;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }
}
