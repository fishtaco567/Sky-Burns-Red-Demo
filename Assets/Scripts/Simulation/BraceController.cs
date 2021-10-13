using UnityEngine;
using System.Collections;

public class BraceController : MonoBehaviour {

    public Transform anchor;
    public Transform pointAt;

    protected float initialLen;

    public float offset;
    public float flip;

    [SerializeField]
    protected SpriteRenderer sr;

    [SerializeField]
    protected Sprite s1;
    [SerializeField]
    protected Sprite s2;
    [SerializeField]
    protected Sprite s3;
    [SerializeField]
    protected Sprite s4;

    [SerializeField]
    protected ParticleSystem ps;

    public void Start() {
        initialLen = Vector3.Distance(anchor.position, pointAt.position);
    }

    public void Update() {
        transform.position = anchor.position;
        transform.rotation = Quaternion.Euler(0, 0, flip * Vector3.Angle(Vector3.up, (pointAt.position - anchor.position).normalized) - offset);

        var currentLen = Vector3.Distance(anchor.position, pointAt.position);
        var lrScale = currentLen / initialLen;
        transform.localScale = new Vector3(lrScale, lrScale, 1);
    }

    public void SetStage(int num) {
        ps.Stop();
        switch(num) {
            case 0:
                sr.sprite = s1;
                break;
            case 1:
                sr.sprite = s2;
                ps.Play();
                break;
            case 2:
                sr.sprite = s3;
                ps.Play();
                break;
            case 3:
                sr.sprite = s4;
                ps.Play();
                break;
            default:
                break;
        }
    }

}
