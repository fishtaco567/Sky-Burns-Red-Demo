using UnityEngine;
using System.Collections.Generic;

public class Crowd : MonoBehaviour {

    //Zero is perfect, One is worst;
    [Header("Don't Touch")]
    public float currentShake;
    public float currentStageDamagePerSecond;
    public float desiredShake;
    public float desiredStageDamagePerSecond;
    public float mood;

    [Header("Please Touch")]
    public float moodHitForOk;
    public float moodHitForBad;
    public float moodBoostForPerfect;

    public float stageDamagePerSecondPerMood;
    public float shakePerSecondPerMood;
    public float shakeOnBad;

    public float stageDamageEasingFactor;
    public float shakeEasingFactor;

    public float shakeTimescale;
    public float shakeScale;

    protected int forceNum;

    [SerializeField]
    protected SpriteRenderer indicator;
    [SerializeField]
    protected Transform indicatorBase;
    [SerializeField]
    protected float indicatorSize;
    [SerializeField]
    protected Sprite[] indicatorSprites;

    public void StartNewSong() {
        SimulatorManager.Instance.currentSim.StartForce(0);
        mood = 0;
        currentShake = 0;
        desiredShake = 0;
        currentStageDamagePerSecond = 0;
        desiredStageDamagePerSecond = 0;
    }

    public void Update() {
        mood = Mathf.Clamp01(mood);
        var shakeyshake = (Mathf.PerlinNoise(0.1594920421572589f, Time.time * shakeTimescale) - 0.5f) * currentShake * shakeScale;
        SimulatorManager.Instance.currentSim.UpdateForce(forceNum, shakeyshake);
        SimulatorManager.Instance.currentSim.OnDamage(currentStageDamagePerSecond);

        indicator.transform.position = Vector3.Lerp(indicator.transform.position,
            new Vector3(indicatorBase.position.x, indicatorBase.position.y - indicatorSize / 2 + indicatorSize * (1 - mood), indicator.transform.position.z),
            3f * Time.deltaTime);
        indicator.sprite = indicatorSprites[Mathf.Clamp(Mathf.FloorToInt(mood * indicatorSprites.Length), 0, indicatorSprites.Length - 1)];
    }

    public void EndSong() {
        SimulatorManager.Instance.currentSim.EndForce(forceNum);
    }

    public void Tick() {
        mood = Mathf.Clamp01(mood);
        desiredStageDamagePerSecond = mood * stageDamagePerSecondPerMood;
        desiredShake = mood * shakePerSecondPerMood;
        currentShake = Mathf.Lerp(currentShake, desiredShake, shakeEasingFactor);
        currentStageDamagePerSecond = Mathf.Lerp(currentStageDamagePerSecond, desiredStageDamagePerSecond, stageDamageEasingFactor);
    }

    public void OnPerfectHit() {
        mood -= moodBoostForPerfect;
        currentShake = 0;
        currentStageDamagePerSecond = 0;
    }

    public void OnOkHit() {
        mood += moodHitForOk;
    }

    public void OnBadHit() {
        mood += moodHitForBad;
        currentShake = shakeOnBad;
    }

    public void OnPowerup() {
        mood = 0;
        currentShake = 0;
    }

}
