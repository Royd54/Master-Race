using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    //Player object
    private PlayerMovement _player;

    //Input
    float x, y;
    bool jumping, sprinting, crouching;

    private void Start()
    {
        _player = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        MyInput();
    }

    private void MyInput()
    {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
        jumping = Input.GetButton("Jump");
        crouching = Input.GetKey(KeyCode.LeftControl);

        _player.MyInput(x, y, jumping, sprinting, crouching);
    }
}
