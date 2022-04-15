using UnityEngine;

public class Scroll : MonoBehaviour {

    public Vector2 scroll;

    private Vector3 basePos;

    public void Start() {
        basePos = transform.position;
    }

    public void Update() {
        var s = Time.time * scroll;
        transform.position = new Vector3(s.x, s.y, 0) + basePos;
    }

}