using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    private Coroutine currentMoveCoroutine;
    private bool isCurrentlyMoving = false;

    [SerializeField]
    protected GameObject focus;

    [SerializeField]
    protected float dragAmount;
    [SerializeField]
    protected float pullAmount;

    public void Update() {
        if(!isCurrentlyMoving && BeatmapController.Instance.songIsRunning) {
            var focusPosAdj = focus.transform.position * dragAmount;
            focusPosAdj.z = transform.position.z;
            transform.position = Vector3.Lerp(transform.position, focusPosAdj, Time.deltaTime * pullAmount);
        }
    }

    public void MoveTo(Vector2 position, float time) {
        if(isCurrentlyMoving && currentMoveCoroutine != null) {
            StopCoroutine(currentMoveCoroutine);
        }

        var actPos = new Vector3(position.x, position.y, transform.position.z);
        isCurrentlyMoving = true;
        StartCoroutine(MoveOverTime(transform.position, actPos, time));
    }

    protected IEnumerator MoveOverTime(Vector3 initialPosition, Vector3 finalPosition, float time) {
        float t = 0;
        while(t < time) {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(initialPosition, finalPosition, Easings.EaseOutSine(t / time));
            yield return null;
        }

        isCurrentlyMoving = false;
        transform.position = finalPosition;
    }

}