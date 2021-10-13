using UnityEngine;
using System.Collections;

public class PopSim : MonoBehaviour {

    public Vector3 velocity;

    public float airDrag;
    public float gravity;

    public float lifetime;
    public bool smallerOverLifetime;
    public bool rotateOverLifetime;
    public float time;

    // Use this for initialization
    void Start() {
        time = 0;
    }

    private void OnEnable() {
        time = 0;
    }

    // Update is called once per frame
    void Update() {
        transform.position += velocity * Time.deltaTime;
        velocity = velocity + new Vector3(0, -gravity, 0) * Time.deltaTime - velocity * airDrag * Time.deltaTime;

        time += Time.deltaTime;
        if(smallerOverLifetime) {
            var s = 1 - time / lifetime;
            transform.localScale = new Vector3(s, s, 1);
        }

        if(rotateOverLifetime) {
            transform.rotation = Quaternion.Euler(0, 0, -velocity.x * time * 20);
        }

        if(time > lifetime) {
            ObjectPool.Instance.DestroyObject(this.gameObject);
        }
    }
}
