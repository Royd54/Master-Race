using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] float health;
    [SerializeField] public Transform head;
    public static Player player;

    public Vector3 playerPos;
    [SerializeField] private ParticleSystem _bloodSplatterFX;

    private void FixedUpdate()
    {
        playerPos = head.position;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.LogError(string.Format("Player health: {0}",health));
    }

    public Vector3 GetHeadPosition()
    {
        return head.position;
    }

    public Vector3 GetPlayerPos()
    {
        return playerPos;
    }

    public void TakeDamage(float damage, Vector3 contactPoint)
    {
        health -= damage;
        if (health <= 0)
            Destroy(gameObject);
    }
}
