using UnityEngine;

public class MoveCamera : MonoBehaviour {

    [SerializeField] private Transform _player;

    void Update() {
        transform.position = _player.transform.position;
    }
}
