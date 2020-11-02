using UnityEngine;

public class GrapplingGun : MonoBehaviour {

    [SerializeField] private LayerMask _whatIsGrappleable;
    [SerializeField] private string _whatIsGrappleableTag = "Grappleable";
    [SerializeField] private Transform _gunTip, _camera, _player;

    private LineRenderer _lr;
    private Vector3 _grapplePoint;
    private float _maxDistance = 100f;
    private SpringJoint _joint;

    void Awake() {
        _lr = GetComponent<LineRenderer>();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.F)) {
            StartGrapple();
        }
        else if (Input.GetKeyUp(KeyCode.F)) {
            StopGrapple();
        }
    }

    void LateUpdate() {
        DrawRope();
    }

    void StartGrapple() {
        RaycastHit hit;
        if (Physics.Raycast(_camera.position, _camera.forward, out hit, _maxDistance, _whatIsGrappleable)&& hit.transform.gameObject.tag == _whatIsGrappleableTag) {
            _grapplePoint = hit.point;
            _joint = _player.gameObject.AddComponent<SpringJoint>();
            _joint.autoConfigureConnectedAnchor = false;
            _joint.connectedAnchor = _grapplePoint;

            float distanceFromPoint = Vector3.Distance(_player.position, _grapplePoint);

            //The distance grapple will try to keep from grapple point. 
            _joint.maxDistance = distanceFromPoint * 0.5f;
            _joint.minDistance = distanceFromPoint * 0.25f;

            _joint.spring = 4.5f;
            _joint.damper = 7f;
            _joint.massScale = 4.5f;

            _lr.positionCount = 2;
            currentGrapplePosition = _gunTip.position;
        }
    }

    void StopGrapple() {
        _lr.positionCount = 0;
        Destroy(_joint);
    }

    private Vector3 currentGrapplePosition;
    
    void DrawRope() {
        //If not grappling, don't draw rope
        if (!_joint) return;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, _grapplePoint, Time.deltaTime * 8f);
        
        _lr.SetPosition(0, _gunTip.position);
        _lr.SetPosition(1, currentGrapplePosition);
    }

    public bool IsGrappling() {
        return _joint != null;
    }

    public Vector3 GetGrapplePoint() {
        return _grapplePoint;
    }
}
