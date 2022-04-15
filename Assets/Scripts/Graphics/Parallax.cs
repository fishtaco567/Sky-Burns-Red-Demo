using UnityEngine;

public class Parallax : MonoBehaviour {

    public GameObject focus;
    public Vector2 parallax;

    private Vector3 basePos;

    public void Start() {
        basePos = transform.position;
    }

    public void Update() {
        var dif = Vector3.zero - focus.transform.position;
        var asV2 = new Vector2(dif.x, dif.y);
        var p = asV2 * parallax;
        transform.position = basePos + new Vector3(p.x, p.y, 0);
    }

}