using UnityEngine;
using System.Collections;

public class FanController : MonoBehaviour {

    public float mood;
    public float moodEasing;
    public float moodOffset;

    public Animator anim;

    public float chanceToFollowHit;

    public float heartChancePerSecond;
    public float booChancePerSecond;

    [FMODUnity.EventRef]
    public string crowdNoise;
    [FMODUnity.EventRef]
    public string boo;
    [FMODUnity.EventRef]
    public string whoo;
    [FMODUnity.EventRef]
    public string sucks;
    [FMODUnity.EventRef]
    public string rocks;

    public float crowdNoiseChancePerSec;
    public float sucksChancePerSec;
    public float rockChancePerSec;

    public void Start() {
        mood = 1;
    }

    public void Update() {
        anim.speed = (BeatmapController.Instance.currentBeatmap.sixteenthTime * BeatmapController.Instance.currentBeatmap.sixteenthsInABeat) / (0.6f);

        mood = Mathf.Lerp(mood, (1 - BeatmapController.Instance.crowd.mood) * 2 + moodOffset, moodEasing);
        mood = Mathf.Clamp(mood, 0, 2);
        var intMood = Mathf.RoundToInt(mood);
        anim.SetInteger("Mood", intMood);

        if(Random.value < chanceToFollowHit * Time.deltaTime) {
            if(BeatmapController.Instance.hitLeft) {
                anim.SetTrigger("Left");
            } else if(BeatmapController.Instance.hitRight) {
                anim.SetTrigger("Right");
            }
        }

        if(mood < 0.8f && Random.value < booChancePerSecond * Time.deltaTime) {
            PopManager.Instance.DoBooPop(transform.position + new Vector3(0, 3, 0));
            Play(boo);
        }

        if(mood < 0.7f && BeatmapController.Instance.songIsRunning) {
            PlayIf(sucks, sucksChancePerSec);
        }

        if(mood > 1.6f && BeatmapController.Instance.songIsRunning && BeatmapController.Instance.sbrcount == 0) {
            if(PlayIf(rocks, rockChancePerSec)) {
                BeatmapController.Instance.sbrcount++;
            }
        }
        PlayIf(crowdNoise, crowdNoiseChancePerSec);

        if(mood > 1.5f && Random.value < heartChancePerSecond * Time.deltaTime) {
            PopManager.Instance.DoHeartPop(transform.position + new Vector3(0, 3, 0));
            if(BeatmapController.Instance.songIsRunning) {
                Play(whoo);
            }
        }
    }

    public bool PlayIf(string play, float ifn) {
        if(Random.value < ifn * Time.deltaTime) {
            FMODUnity.RuntimeManager.PlayOneShot(play, transform.position);
            return true;
        }
        return false;
    }

    public void Play(string play) {
        FMODUnity.RuntimeManager.PlayOneShot(play, transform.position);
    }

    public void Sync() {
        anim.playbackTime = 0;
    }

    public void Reset() {
        mood = 1;
    }

}
