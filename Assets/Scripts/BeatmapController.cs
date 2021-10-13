﻿using UnityEngine;
using System.Collections.Generic;
using Rewired;

public class BeatmapController : Singleton<BeatmapController> {

    protected Player rewiredPlayer;

    [SerializeField]
    protected float impulse;
    [SerializeField]
    protected float impulseTime;

    public Beatmap currentBeatmap;
    public bool[] consumeArray;
    public bool[] leftPup;
    public bool[] rightPup;
    public bool[] heldKilled;

    [SerializeField]
    protected float maxPrehitTime;
    [SerializeField]
    protected float goodPrehitTime;
    [SerializeField]
    protected float perfectPrehitTime;
    [SerializeField]
    protected float perfectPosthitTime;
    [SerializeField]
    protected float goodPosthitTime;
    [SerializeField]
    protected float maxPosthitTime;

    [Range(-0.2f, 0.2f)]
    [SerializeField]
    protected float timeOffset;

    public bool songIsRunning;
    protected float songStartTime;
    public float currentSongTime {
        get {
            song.getTimelinePosition(out int pos);
            return pos / 1000f + timeOffset;
        }
    }

    protected FMOD.Studio.EventInstance song;

    [SerializeField]
    [FMODUnity.EventRef]
    protected string badHit;

    [SerializeField]
    protected int lastTapped;

    [SerializeField]
    protected Animator leftHitAnim;
    [SerializeField]
    protected Animator rightHitAnim;
    [SerializeField]
    protected Animator leftHoldAnim;
    [SerializeField]
    protected Animator rightHoldAnim;

    [SerializeField]
    public Crowd crowd;

    public int numHit;
    public int numNotes;
    public int numPerfect;
    public int numGood;
    public int numOk;
    public int numMissed;

    public float maxAngle;

    public bool hitLeft;
    public bool hitRight;

    public List<FanController> fans;

    [FMODUnity.EventRef]
    public string chant;
    [FMODUnity.EventRef]
    public string pup;

    public int sbrcount = 0;

    protected void Start() {
        rewiredPlayer = ReInput.players.GetPlayer(0);
    }

    protected void Update() {
        if(!songIsRunning) {
            return;
        }
        var lastBeat = GetBeatNumber();

        song.getPlaybackState(out var state);
        if(lastBeat >= currentBeatmap.map.Length) {
            GameManager.Instance.ResultsScreen(numPerfect, numGood, numOk, numMissed, numHit / (float)numNotes, maxAngle);
            FMODUnity.RuntimeManager.PlayOneShot(chant);
            SimulatorManager.Instance.currentSim.Stop();
            EndSong(true);
        }

        maxAngle = Mathf.Max(maxAngle, Mathf.Abs(SimulatorManager.Instance.currentSim.GetAngle()));

        ProcessInput();
        crowd.Tick();

        var twoBeatsAgo = lastBeat - 1;
        var threeBeatsAgo = lastBeat - 2;
        var curPostHitTime = GetPostHitTime();
        var postTwoBeats = curPostHitTime + currentBeatmap.sixteenthTime;
        var postThreeBeats = curPostHitTime + currentBeatmap.sixteenthTime * 2;

        if(lastBeat >= 0 && lastBeat < currentBeatmap.map.Length && currentBeatmap.map[lastBeat].hasBeat && !consumeArray[lastBeat] && curPostHitTime > maxPosthitTime) {
            consumeArray[lastBeat] = true;
            numMissed += 1;
            numNotes += 1;
            OnBadHit();
        }

        if(twoBeatsAgo >= 0 && twoBeatsAgo < currentBeatmap.map.Length && currentBeatmap.map[twoBeatsAgo].hasBeat && !consumeArray[twoBeatsAgo] && postTwoBeats > maxPosthitTime) {
            consumeArray[twoBeatsAgo] = true;
            numMissed += 1;
            numNotes += 1;
            OnBadHit();
        }

        if(threeBeatsAgo >= 0 && threeBeatsAgo < currentBeatmap.map.Length && currentBeatmap.map[threeBeatsAgo].hasBeat && !consumeArray[threeBeatsAgo] && postThreeBeats > maxPosthitTime) {
            consumeArray[threeBeatsAgo] = true;
            numMissed += 1;
            numNotes += 1;
            OnBadHit();
        }

        if(songIsRunning && lastTapped <= lastBeat) {
            lastTapped += 4;
            //tap.Play();
        }
    }

    public void SetCurrentBarWithLeadIn(int bar) {
        StartCoroutine(TrySet(bar));
    }

    public System.Collections.IEnumerator TrySet(int bar) {
        var res = song.setTimelinePosition((int)(bar * currentBeatmap.sixteenthTime) * 1000);
        while(res != FMOD.RESULT.OK) {
            res = song.setTimelinePosition((int)(bar * currentBeatmap.sixteenthTime) * 1000);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void OnBadHit() {
        PopManager.Instance.DoNopePop(Random.value < 0.5f, false);
        crowd.OnBadHit();

        FMODUnity.RuntimeManager.PlayOneShot(badHit);
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Fucked", 1);
    }

    public void ProcessInput() {
        hitLeft = false;
        hitRight = false;
        bool left = rewiredPlayer.GetButtonDown("Left");
        bool right = rewiredPlayer.GetButtonDown("Right");
        var buttonPressed = left || right;

        var lastBeat = GetBeatNumber();
        var nextBeat = lastBeat + 1;

        var inputConsumed = false;

        if(buttonPressed && lastBeat >= 0 && lastBeat < currentBeatmap.map.Length && !consumeArray[lastBeat] && GetPostHitTime() < maxPosthitTime) {
            if(currentBeatmap.map[lastBeat].hasBeat) {
                consumeArray[lastBeat] = true;
                inputConsumed = true;
                numNotes += 1;
                numHit += 1;

                if(GetPostHitTime() < perfectPosthitTime) {
                    PopManager.Instance.DoPerfPop(left, false);
                    numPerfect += 1;
                    crowd.OnPerfectHit();
                } else if(GetPostHitTime() < goodPosthitTime) {
                    PopManager.Instance.DoGoodPop(left, false);
                    numGood += 1;
                } else {
                    PopManager.Instance.DoOkPop(left, false);
                    crowd.OnOkHit();
                    numOk += 1;
                }

                if(currentBeatmap.map[lastBeat].leftPowerup && left) {
                    var pi = FMODUnity.RuntimeManager.CreateInstance(pup);
                    pi.setParameterByName("PupSide", 0);
                    pi.start();
                    pi.release();
                    leftPup[lastBeat] = true;
                    if(currentBeatmap.map[lastBeat].powerupType == 0) {
                        SimulatorManager.Instance.currentSim.Repair(8);
                    } else {
                        crowd.OnPowerup();
                    }
                } else if(currentBeatmap.map[lastBeat].rightPowerup && right) {
                    var pi = FMODUnity.RuntimeManager.CreateInstance(pup);
                    pi.setParameterByName("PupSide", 0);
                    pi.start();
                    pi.release();
                    rightPup[lastBeat] = true;
                    if(currentBeatmap.map[lastBeat].powerupType == 0) {
                        SimulatorManager.Instance.currentSim.Repair(8);
                    } else {
                        crowd.OnPowerup();
                    }
                }
            }
        }

        if(buttonPressed && !inputConsumed && nextBeat >= 0 && nextBeat < currentBeatmap.map.Length && !consumeArray[nextBeat] && GetPreHitTime() < maxPrehitTime) {
            if(currentBeatmap.map[nextBeat].hasBeat) {
                consumeArray[nextBeat] = true;
                inputConsumed = true;
                numNotes += 1;
                numHit += 1;

                if(GetPreHitTime() < perfectPrehitTime) {
                    PopManager.Instance.DoPerfPop(left, false);
                    numPerfect += 1;
                    crowd.OnPerfectHit();
                } else if(GetPreHitTime() < goodPrehitTime) {
                    PopManager.Instance.DoGoodPop(left, false);
                    numGood += 1;
                } else {
                    PopManager.Instance.DoOkPop(left, false);
                    crowd.OnOkHit();
                    numOk += 1;
                }

                if(currentBeatmap.map[nextBeat].leftPowerup && left) {
                    var pi = FMODUnity.RuntimeManager.CreateInstance(pup);
                    pi.setParameterByName("PupSide", 0);
                    pi.start();
                    pi.release();
                    leftPup[nextBeat] = true;
                    if(currentBeatmap.map[nextBeat].powerupType == 0) {
                        SimulatorManager.Instance.currentSim.Repair(8);
                    } else {
                        crowd.OnPowerup();
                    }
                } else if(currentBeatmap.map[nextBeat].rightPowerup && right) {
                    var pi = FMODUnity.RuntimeManager.CreateInstance(pup);
                    pi.setParameterByName("PupSide", 1);
                    pi.start();
                    pi.release();
                    rightPup[nextBeat] = true;
                    if(currentBeatmap.map[nextBeat].powerupType == 0) {
                        SimulatorManager.Instance.currentSim.Repair(8);
                    } else {
                        crowd.OnPowerup();
                    }
                }
            }
        }

        if(buttonPressed && !inputConsumed) {
            OnBadHit();
        } else if(buttonPressed && inputConsumed) {
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Fucked", 0);
            if(rewiredPlayer.GetButtonDown("Left")) {
                hitLeft = true;
                leftHitAnim.Play("HitLeft");
                SimulatorManager.Instance.currentSim.ApplySmoothedImpulse(-impulse, impulseTime);
            } else if(rewiredPlayer.GetButtonDown("Right")) {
                hitRight = true;
                rightHitAnim.Play("HitRight");
                SimulatorManager.Instance.currentSim.ApplySmoothedImpulse(impulse, impulseTime);
            }
        }
    }

    public void SetupNewBeatmap(Beatmap beatmap) {
        SimulatorManager.Instance.currentSim.Reset();
        this.currentBeatmap = beatmap;
        this.consumeArray = new bool[beatmap.map.Length];
        this.leftPup = new bool[beatmap.map.Length];
        this.rightPup = new bool[beatmap.map.Length];
        this.heldKilled = new bool[beatmap.map.Length];
        BeatmapDrawer.Instance.ResetBeatsDrawn();
        song = FMODUnity.RuntimeManager.CreateInstance(currentBeatmap.songEvent);
        crowd.StartNewSong();

        numHit = 0;
        numNotes = 0;
        numPerfect = 0;
        numGood = 0;
        numOk = 0;
        numMissed = 0;
        maxAngle = 0;
        sbrcount = 0;
    }

    public void StartNewSong() {
        songIsRunning = true;
        songStartTime = Time.time + 5;
        lastTapped = 0;
        song.start();
        foreach(var fc in fans) {
            fc.Sync();
        }
    }

    public void PauseSong() {
        songIsRunning = false;
        song.setPaused(true);
    }

    public void RestartSong() {
        songIsRunning = true;
        song.setPaused(false);
    }

    public void EndSong(bool canFall) {
        songIsRunning = false;
        BeatmapDrawer.Instance.Stop(canFall);
        song.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        crowd.EndSong();
    }

    public float GetTimeLeft() {
        song.getDescription(out var desc);
        song.getTimelinePosition(out int pos);
        var actPos = pos / 1000f;
        desc.getLength(out int len);
        var actLen = len / 1000f;
        return actLen - actPos;
    }

    //This is the beat number "before" the current time
    public int GetBeatNumber() {
        return Mathf.FloorToInt(currentSongTime / currentBeatmap.sixteenthTime);
    }

    public float GetPostHitTime() {
        return currentSongTime % currentBeatmap.sixteenthTime;
    }

    public float GetPreHitTime() {
        return currentBeatmap.sixteenthTime - GetPostHitTime();
    }

    public void GiveUp() {
        EndSong(true);
        songIsRunning = false;
        GameManager.Instance.GoToMainMenu();
        SimulatorManager.Instance.currentSim.Stop();
    }

}