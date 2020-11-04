using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class TakeCoverAI : MonoBehaviour
{
    //Animator stuff
    private Animator _animator;
    const string RUN_TRIGGER = "Run";
    const string WalkBackwards_TRIGGER = "WalkBackwards";
    const string CROUCH_TRIGGER = "Crouch";
    const string SHOOT_TRIGGER = "Shoot";

    [SerializeField] private NavMeshAgent _nav;
    [SerializeField] private int _frameInterval = 10;
    [SerializeField] private int _facePlayerFactor = 50;

    [SerializeField] private Player _player;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private GameObject _muzzlePos;

    [SerializeField] private Vector3 _randomPosition;
    [SerializeField] private Vector3 _coverPoint;
    [SerializeField] private float _rangeRandPoint = 6f;
    [SerializeField] private bool _isHiding = false;

    [SerializeField] private LayerMask _coverLayer;
    [SerializeField] private Vector3 _coverObj;
    [SerializeField] private LayerMask _visivleLayer;

    [SerializeField] private float _maxCovDist = 30f;
    [SerializeField] private bool _coverIsClose;
    [SerializeField] private bool _coverNotReached = true;

    [SerializeField] private float _distToCoverPos = 1f;
    [SerializeField] private float _distToCoverObj = 20f;

    [SerializeField] private float _rangeDist = 15f;
    [SerializeField] private bool _playerInRange = false;
    [SerializeField] private bool _playerInSight = false;

    [SerializeField] private int _testCoverPos = 10;

    [SerializeField] private ParticleSystem _bloodSplatterFX;
    [SerializeField] private AudioClip _shootingSoundFX;

    public float health;
    private bool _isDead = false;

    [SerializeField] private Collider _maincollider;
    public Collider[] allcolliders;

    private bool RandomPoint(Vector3 center, float rangeRandPoint, out Vector3 resultCover)
    {
        for (int i = 0; i < _testCoverPos; i++)
        {
            _randomPosition = center + Random.insideUnitSphere * rangeRandPoint;
            Vector3 direction = _player.GetPlayerPos() - _randomPosition;
            RaycastHit hitTestCov;
            if (Physics.Raycast(_randomPosition, direction.normalized, out hitTestCov, rangeRandPoint, _visivleLayer))
            {
                if (hitTestCov.collider.gameObject.layer == 13)
                {
                    resultCover = _randomPosition;
                    return true;
                }
            }
        }
        resultCover = Vector3.zero;
        return false;
    }

    private void Start()
    {
        _nav = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _animator.SetTrigger(WalkBackwards_TRIGGER);
    }

    public void Init(Player player, Transform spawnpoint)
    {
        transform.position = spawnpoint.position;
        this._player = player;
    }

    private void Update()
    {
        if (_nav.isActiveAndEnabled)
        {
            if (Time.frameCount % _frameInterval == 0)
            {
                float distance = ((_player.GetPlayerPos() - transform.position).sqrMagnitude);

                if (distance < 10f)
                {
                    _nav.speed = 2;
                    _animator.SetTrigger(WalkBackwards_TRIGGER);
                }

                if (distance < _rangeDist * _rangeDist)
                {
                    _playerInRange = true;
                }
                else _playerInRange = false;

                CheckPlayerInSight();

            }

            if (_playerInRange == true && _playerInSight == true)
            {
                CheckCoverDist();

                if (_coverIsClose == true)
                {
                    if (_coverNotReached == true)
                    {
                        _nav.SetDestination(_coverObj);
                    }
                    if (_coverNotReached == false)
                    {
                        TakeCover();

                       FacePlayer();
                    }
                }
                if (_coverIsClose == false)
                {
                    //Behaviour when player is in range, but not close to cover.
                }
            }
        }
    }

    private void CheckCoverDist()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _maxCovDist, _coverLayer);
        Collider nearestCollider = null;
        float minSqrDistance = Mathf.Infinity;

        Vector3 AI_Position = transform.position;

        for (int i = 0; i < colliders.Length; i++)
        {
            float minSqrDistanceToCenter = (AI_Position - colliders[i].transform.position).sqrMagnitude;
            if (minSqrDistanceToCenter < minSqrDistance)
            {
                minSqrDistance = minSqrDistanceToCenter;
                nearestCollider = colliders[i];

                float coverDistance = (nearestCollider.transform.position - AI_Position).sqrMagnitude;

                if (coverDistance <= _maxCovDist*_maxCovDist)
                {
                    _coverIsClose = true;
                    _coverObj = nearestCollider.transform.position;
                    if (coverDistance <= _distToCoverObj * _distToCoverObj)
                    {
                        _coverNotReached = false;
                    }
                    else if (coverDistance > _distToCoverObj*_distToCoverObj)
                    {
                        _coverNotReached = true;
                    }
                }
                if (coverDistance >= _maxCovDist*_maxCovDist)
                {
                    _coverIsClose = false;
                }
            }
        }
        if (colliders.Length < 1)
        {
            _coverIsClose = false;
        }
    }

    private void TakeCover()
    {
        if (RandomPoint(transform.position, _rangeRandPoint, out _coverPoint))
        {
            if (_nav.isActiveAndEnabled)
            {
                _nav.SetDestination(_coverPoint);
                if ((_coverPoint - transform.position).sqrMagnitude <= _distToCoverPos*_distToCoverPos)
                {
                    _isHiding = true;
                    _nav.speed = 0;
                    _animator.SetTrigger(SHOOT_TRIGGER);
                }
            }
        }
    }

    private void CheckPlayerInSight()
    {
        Vector3 targetDir = _player.GetPlayerPos() - _muzzlePos.transform.position;
        float angleToPlayer = (Vector3.Angle(targetDir, transform.forward));
        RaycastHit hit;
        if (Physics.Raycast(_muzzlePos.transform.position, targetDir, out hit))
        {
            if (hit.collider.gameObject.tag == "Player")
                _playerInSight = true;
            else
                _playerInSight = false;
        }
    }

    public void Shoot()
    {
        if (_playerInSight)
        {
            Rigidbody rb = Instantiate(_bulletPrefab, _muzzlePos.transform.position, Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
            GetComponent<AudioSource>().PlayOneShot(_shootingSoundFX);
        }
    }

    private void FacePlayer()
    {
        Vector3 direction = _player.GetHeadPosition() - transform.position;
        direction.y = 0;
        Quaternion rotation = Quaternion.LookRotation(direction);
        rotation = Quaternion.RotateTowards(transform.rotation, rotation, _facePlayerFactor * Time.deltaTime);
        transform.rotation = rotation;
    }

    public void TakeDamage(float damage, Vector3 contactPoint)
    {
        health -= damage;
        if (health <= 0 && !_isDead)
        {
            EnableRagdoll(true);
        }
        else
        {
            ParticleSystem effect = Instantiate(_bloodSplatterFX, contactPoint, Quaternion.LookRotation(_player.transform.position - contactPoint));
            effect.Stop();
            effect.Play();
        }
    }

    private void EnableRagdoll(bool isRagdoll)
    {
        _isDead = true;
        _nav.enabled = false;

        foreach (var col in allcolliders) col.enabled = isRagdoll;

        _maincollider.enabled = !isRagdoll;
        GetComponent<Rigidbody>().useGravity = !isRagdoll;
        GetComponent<Animator>().enabled = !isRagdoll;

        Destroy(gameObject, 5);
    }
}
