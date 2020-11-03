using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomBullet : MonoBehaviour
{
    //Assignables
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private GameObject _explosion;
    [SerializeField] private LayerMask _whatIsEnemies;

    //Stats
    [Range(0f,1f)]
    [SerializeField] private float _bounciness;
    [SerializeField] private bool _useGravity;

    //Damage
    [SerializeField] private int _explosionDamage;
    [SerializeField] private float _explosionRange;
    [SerializeField] private float _explosionForce;

    //Lifetime
    [SerializeField] private int _maxCollisions;
    [SerializeField] private float _maxLifetime;
    [SerializeField] private bool _explodeOnTouch = true;
    private bool _exploded = false;

    int collisions;
    PhysicMaterial physics_mat;

    private void Start()
    {
        Setup();
    }

    private void Update()
    {
        //When to explode:
        if (collisions > _maxCollisions) Explode();

        //Countdown lifetime
        _maxLifetime -= Time.deltaTime;
        if (_maxLifetime <= 0) Explode();
    }

    private void Explode()
    {
        //Instantiate explosion
        if (_explosion != null && _exploded == false) { Instantiate(_explosion, transform.position, Quaternion.identity); _exploded = true; }

        //Check for enemies 
        Collider[] enemies = Physics.OverlapSphere(transform.position, _explosionRange//, whatIsEnemies
            );
        for (int i = 0; i < enemies.Length; i++)
        {
            //Get component of enemy and call Take Damage
            if (enemies[i].tag == "Enemy" && enemies[i].GetComponent<TakeCoverAI>())
                enemies[i].GetComponent<TakeCoverAI>().TakeDamage(_explosionDamage, transform.position);

            //Add explosion force (if enemy has a rigidbody)
            if (enemies[i].tag == "Player" && enemies[i].GetComponent<PlayerMovement>())
                enemies[i].GetComponent<PlayerMovement>().CounterMovementSetter();
            
            if (enemies[i].tag != "Bullet" && enemies[i].GetComponent<Rigidbody>())
                    enemies[i].GetComponent<Rigidbody>().AddExplosionForce(_explosionForce, transform.position, _explosionRange);      
        }

        //Add a little delay, just to make sure everything works fine
        Invoke("Delay", 0.05f);
    }
    private void Delay()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Don't count collisions with other bullets
        if (collision.collider.CompareTag("Bullet")) return;

        //Count up collisions
        collisions ++;

        //Explode if bullet hits an enemy directly and explodeOnTouch is activated
        if (collision.collider.tag == "Enemy") Explode();

        //Explode if bullet hits an enemy directly and explodeOnTouch is activated
        if (_explodeOnTouch) Explode();
    }

    private void Setup()
    {
        //Create a new Physic material
        physics_mat = new PhysicMaterial();
        physics_mat.bounciness = _bounciness;
        physics_mat.frictionCombine = PhysicMaterialCombine.Minimum;
        physics_mat.bounceCombine = PhysicMaterialCombine.Maximum;
        //Assign material to collider
        GetComponent<SphereCollider>().material = physics_mat;

        //Set gravity
        _rb.useGravity = _useGravity;
    }

    /// Just to visualize the explosion range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explosionRange);
    }
}
