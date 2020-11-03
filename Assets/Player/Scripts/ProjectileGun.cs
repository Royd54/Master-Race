using UnityEngine;
using TMPro;
using System.Collections;

public class ProjectileGun : MonoBehaviour
{
    //bullet 
    [SerializeField] private GameObject _bullet;

    //bullet force
    [SerializeField] private float _shootForce, _upwardForce;

    //Gun stats
    [SerializeField] private float _timeBetweenShooting, _spread, _reloadTime, _timeBetweenShots;
    [SerializeField] private int _magazineSize, _bulletsPerTap;
    [SerializeField] private bool _allowButtonHold;
    [SerializeField] private bool slideOnShoot;

    int bulletsLeft, bulletsShot;

    //Recoil
    [SerializeField] private Rigidbody _playerRb;
    [SerializeField] private float _recoilForce;

    //bools
    bool shooting, readyToShoot, reloading;
    
    //Reference
    [SerializeField] private Camera _fpsCam;
    [SerializeField] private Transform _attackPoint;
    [SerializeField] private GameObject _weaponObj;

    //Graphics
    [SerializeField] private GameObject _muzzleFlash;
    [SerializeField] private GameObject _muzzleTrailRenderer;
    [SerializeField] private TextMeshProUGUI _ammunitionDisplay;
    [SerializeField] private float _smokeTrailTime;

    //Sound
    [SerializeField] private AudioClip _ShootSound;

    //bug fixing :D
    public bool allowInvoke = true;
    public bool allowSound = true;

    private void Awake()
    {
        //make sure magazine is full
        bulletsLeft = _magazineSize;
        readyToShoot = true;
    }

    private void Update()
    {
        MyInput();
        DisableTrail2(_muzzleTrailRenderer.GetComponent<TrailRenderer>());
        //Set ammo display, if it exists 
        if (_ammunitionDisplay != null)
            _ammunitionDisplay.SetText(bulletsLeft / _bulletsPerTap + " / " + _magazineSize / _bulletsPerTap);
    }
    private void MyInput()
    {
        //Check if allowed to hold down button and take corresponding input
        if (_allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);

        //Reloading 
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < _magazineSize && !reloading) Reload();
        //Reload automatically when trying to shoot without ammo
        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0) Reload();

        //Shooting
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            //Set bullets shot to 0
            bulletsShot = 0;

            Shoot();
        }
    }

    private void Shoot()
    {
        readyToShoot = false;

        //Find the exact hit position using a raycast
        Ray ray = _fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); //Just a ray through the middle of current view
        RaycastHit hit;

        //check if ray hits something
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(75); //Just a point far away from the player

        //Calculate direction from attackPoint to targetPoint
        Vector3 directionWithoutSpread = targetPoint - _attackPoint.position;

        //Calculate spread
        float x = Random.Range(-_spread, _spread);
        float y = Random.Range(-_spread, _spread);

        //Calculate new direction with spread
        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0); //Just add spread to last direction

        //Instantiate bullet/projectile
        GameObject currentBullet = Instantiate(_bullet, _attackPoint.position, Quaternion.identity); //store instantiated bullet in currentBullet
        if (allowSound)
        {
            AudioManager.Instance.PlaySFX(_ShootSound);
            _weaponObj.GetComponent<Animator>().SetTrigger("Shooting");
            allowSound = false;
        }
        
        //Rotate bullet to shoot direction
        currentBullet.transform.forward = directionWithSpread.normalized;

        //Add forces to bullet
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * _shootForce, ForceMode.Impulse);
        currentBullet.GetComponent<Rigidbody>().AddForce(_fpsCam.transform.up * _upwardForce, ForceMode.Impulse);

        //Instantiate muzzle flash
        if (_muzzleFlash != null)
            _muzzleTrailRenderer.GetComponent<TrailRenderer>().enabled = true;
            _muzzleTrailRenderer.GetComponent<TrailRenderer>().time = _smokeTrailTime;
            Instantiate(_muzzleFlash, _attackPoint.position, Quaternion.identity);

        bulletsLeft--;
        bulletsShot++;

        //Invoke resetShot function (if not already invoked), with timeBetweenShooting
        if (allowInvoke)
        {
            Invoke("ResetShot", _timeBetweenShooting);
            allowInvoke = false;

            //Add recoil to player (should only be called once)
            if (slideOnShoot)
            {
                GetComponentInParent<PlayerMovement>().CounterMovementSetter();                                     
            }
            
            _playerRb.AddForce(-directionWithSpread.normalized * _recoilForce, ForceMode.Impulse);
        }

        //if more than one bulletsPerTap make sure to repeat shoot function
        if (bulletsShot < _bulletsPerTap && bulletsLeft > 0)
            Invoke("Shoot", _timeBetweenShots);
    }

    private void ResetShot()
    {
        //Allow shooting and invoking again
        _weaponObj.GetComponent<Animator>().SetTrigger("Idle");
        allowSound = true;
        readyToShoot = true;
        allowInvoke = true;
    }

    private void Reload()
    {
        reloading = true;
        _weaponObj.GetComponent<Animator>().SetTrigger("Reloading");
        _weaponObj.GetComponent<Animator>().speed = _reloadTime;
        Invoke("ReloadFinished", _reloadTime); //Invoke ReloadFinished function with your reloadTime as delay
    }

    private void ReloadFinished()
    {
        //Fill magazine
        bulletsLeft = _magazineSize;
        reloading = false;
    }
}
