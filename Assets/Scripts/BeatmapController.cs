using UnityEngine;
using System.Collections.Generic;
using Rewired;

public class BeatmapController : Singleton<BeatmapController> {

    protected Player rewiredPlayer;

    [SerializeField]
    protected float impulse;
    [SerializeField]
    protected float impulseTime;
    [SerializeField]
    protected float holdForce;

    [SerializeField]
    protected float preStartTime;

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

    [SerializeField]
    protected float maxPreHoldEndTime;
    [SerializeField]
    protected float maxPostHoldEndTime;

    [SerializeField]
    protected float p10Time;

    [Range(-0.2f, 0.2f)]
    [SerializeField]
    protected float timeOffset;

    public bool songIsRunning;
    protected float songStartTime;
    public float currentSongTime {
        get {
            if(songIsRunning) {
                song.getTimelinePosition(out int pos);
                return pos / 1000f + timeOffset;
            } else if(inPrestart) {
                return -3.564f + (Time.time - prestartStartedAt);
            } else {
                return -5;
            }
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

    public int currentStreak;
    public int maxStreak;

    public ScoreHolder score;

    public bool hitLeft;
    public bool hitRight;

    public List<FanController> fans;

    [FMODUnity.EventRef]
    public string chant;
    [FMODUnity.EventRef]
    public string pup;

    [System.NonSerialized]
    public List<(bool, int, float)> heldNotes;

    [System.NonSerialized]
    public int sbrcount = 0;

    protected int currentBeat = 0;

    private Dictionary<int, List<SkyEventProgram>> programsForSong;
    private List<SkyEventProgram> succeedEvents;
    private List<SkyEventProgram> failEvents;

    protected int holdForceHandle;

    [System.NonSerialized]
    public bool inPrestart;
    private float prestartStartedAt;

    [SerializeField]
    protected GameObject band;
    [SerializeField]
    protected CameraController cameraController;

    [SerializeField]
    [FMODUnity.EventRef]
    protected string playEvent;

    public TMPro.TMP_Text scoreText;
    public TMPro.TMP_Text multiText;

    protected void Awake() {
        Newtonsoft.Json.Utilities.AotHelper.EnsureList<Beatmap>();
        Newtonsoft.Json.Utilities.AotHelper.EnsureDictionary<int, string[]>();
        Newtonsoft.Json.Utilities.AotHelper.EnsureDictionary<int, float>();
        Newtonsoft.Json.Utilities.AotHelper.EnsureDictionary<int, Vector2Int>();
        Newtonsoft.Json.Utilities.AotHelper.EnsureList<string>();
        Newtonsoft.Json.Utilities.AotHelper.EnsureList<Beatmap.Beat>();
        Newtonsoft.Json.Utilities.AotHelper.EnsureType<Beatmap.Beat>();
    }

    protected void Start() {
        rewiredPlayer = ReInput.players.GetPlayer(0);
    }

    protected void Update() {
        if(!songIsRunning) {
            return;
        }
        var lastBeat = GetBeatNumber();

        if(lastBeat > currentBeat) {
            currentBeat = lastBeat;
            if(programsForSong.TryGetValue(currentBeat, out var list)) {
                foreach(var program in list) {
                    SkyEventManager.Instance.ExecuteProgram(program);
                }
            }
        }

        score.Multiplier = currentStreak + 1;

        song.getPlaybackState(out var state);
        if(lastBeat >= currentBeatmap.map.Length) {
            GameManager.Instance.ResultsScreen(numPerfect, numGood, numOk, numMissed, numHit / (float)numNotes, score.Score, maxStreak + 1);
            FMODUnity.RuntimeManager.PlayOneShot(chant);
            SimulatorManager.Instance.currentSim.Stop();
            EndSong(true);
            foreach(var program in succeedEvents) {
                SkyEventManager.Instance.ExecuteProgram(program);
            }
            cameraController.MoveTo(new Vector2(cameraController.transform.position.x, 7f), 1.5f);
        }

        maxAngle = Mathf.Max(maxAngle, Mathf.Abs(SimulatorManager.Instance.currentSim.GetAngle()));

        ProcessInput();
        crowd.Tick();

        var twoBeatsAgo = lastBeat - 1;
        var threeBeatsAgo = lastBeat - 2;
        var curPostHitTime = GetPostHitTime();
        var postTwoBeats = curPostHitTime + currentBeatmap.sixteenthTime;
        var postThreeBeats = curPostHitTime + currentBeatmap.sixteenthTime * 2;

        if(lastBeat >= 0 && lastBeat < currentBeatmap.map.Length && currentBeatmap.map[lastBeat].beat != BeatType.None && !consumeArray[lastBeat] && curPostHitTime > maxPosthitTime) {
            consumeArray[lastBeat] = true;
            heldKilled[lastBeat] = true;
            numMissed += 1;
            numNotes += 1;
            OnBadHit();
        }

        if(twoBeatsAgo >= 0 && twoBeatsAgo < currentBeatmap.map.Length && currentBeatmap.map[twoBeatsAgo].beat != BeatType.None && !consumeArray[twoBeatsAgo] && postTwoBeats > maxPosthitTime) {
            consumeArray[twoBeatsAgo] = true;
            heldKilled[twoBeatsAgo] = true;
            numMissed += 1;
            numNotes += 1;
            OnBadHit();
        }

        if(threeBeatsAgo >= 0 && threeBeatsAgo < currentBeatmap.map.Length && currentBeatmap.map[threeBeatsAgo].beat != BeatType.None && !consumeArray[threeBeatsAgo] && postThreeBeats > maxPosthitTime) {
            consumeArray[threeBeatsAgo] = true;
            heldKilled[threeBeatsAgo] = true;
            numMissed += 1;
            numNotes += 1;
            OnBadHit();
        }

        if(songIsRunning && lastTapped <= lastBeat) {
            lastTapped += 4;
            //tap.Play();
        }

        var leftHeld = false;
        var rightHeld = false;
        var curHoldForce = 0.0f;
        for(int i = heldNotes.Count - 1; i >= 0; i--) {
            var held = heldNotes[i];

            held.Item3 += Time.deltaTime;
            if(held.Item1) {
                curHoldForce += holdForce;
                rightHeld = true;
            } else {
                curHoldForce -= holdForce;
                leftHeld = true;
            }

            if(held.Item3 >= p10Time) {
                score.Hit10();
                held.Item3 = 0;
            }

            heldNotes[i] = held;
        }
        SimulatorManager.Instance.currentSim.UpdateForce(holdForceHandle, curHoldForce);

        rightHoldAnim.SetBool("IsHolding", rightHeld);
        leftHoldAnim.SetBool("IsHolding", leftHeld);
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
        currentStreak = 0;
    }

    public void ProcessInput() {
        hitLeft = false;
        hitRight = false;
        bool left = rewiredPlayer.GetButtonDown("Left");
        bool right = rewiredPlayer.GetButtonDown("Right");
        var buttonPressed = left || right;

        var leftHeld = rewiredPlayer.GetButton("Left");
        var rightHeld = rewiredPlayer.GetButton("Right");

        var lastBeat = GetBeatNumber();
        var nextBeat = lastBeat + 1;

        var inputConsumed = false;

        for(int i = heldNotes.Count - 1; i >= 0; i--) {
            var holdEndsAt = currentBeatmap.GetTimeOfHeldEnd(heldNotes[i].Item2);
            var holdEndDif = currentSongTime - holdEndsAt;

            var reasonableHold = false;
            if(holdEndDif < maxPostHoldEndTime && holdEndDif > -maxPreHoldEndTime) {
                reasonableHold = true;
            }

            if(heldNotes[i].Item1 && !rightHeld) {
                heldKilled[heldNotes[i].Item2] = !reasonableHold; 
                heldNotes.RemoveAt(i);

                if(reasonableHold) {
                    PopManager.Instance.DoPerfPop(false, false);
                    score.HitNote1000();
                }
            } else if(!heldNotes[i].Item1 && !leftHeld) {
                heldKilled[heldNotes[i].Item2] = !reasonableHold;
                heldNotes.RemoveAt(i);

                if(reasonableHold) {
                    PopManager.Instance.DoPerfPop(true, false);
                    score.HitNote1000();
                }
            }

            if(holdEndDif > maxPostHoldEndTime) {
                heldNotes.RemoveAt(i);
            }
        }

        if(buttonPressed && lastBeat >= 0 && lastBeat < currentBeatmap.map.Length && !consumeArray[lastBeat] && GetPostHitTime() < maxPosthitTime) {
            if((currentBeatmap.map[lastBeat].beat == BeatType.Center) || (left && currentBeatmap.map[lastBeat].beat == BeatType.Left) || (right && currentBeatmap.map[lastBeat].beat == BeatType.Right)) {
                consumeArray[lastBeat] = true;
                inputConsumed = true;
                numNotes += 1;
                numHit += 1;
                currentStreak += 1;
                maxStreak = Mathf.Max(currentStreak, maxStreak);

                if(GetPostHitTime() < perfectPosthitTime) {
                    PopManager.Instance.DoPerfPop(left, false);
                    numPerfect += 1;
                    crowd.OnPerfectHit();
                    score.HitNote300();
                } else if(GetPostHitTime() < goodPosthitTime) {
                    PopManager.Instance.DoGoodPop(left, false);
                    numGood += 1;
                    score.HitNote200();
                } else {
                    PopManager.Instance.DoOkPop(left, false);
                    crowd.OnOkHit();
                    numOk += 1;
                    score.HitNote100();
                }

                if(currentBeatmap.map[lastBeat].beatsHeld != 0) {
                    var side = right;
                    heldNotes.Add((side, lastBeat, 0));
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
            if((currentBeatmap.map[nextBeat].beat == BeatType.Center) || (left && currentBeatmap.map[nextBeat].beat == BeatType.Left) || (right && currentBeatmap.map[nextBeat].beat == BeatType.Right)) {
                consumeArray[nextBeat] = true;
                inputConsumed = true;
                numNotes += 1;
                numHit += 1;
                currentStreak += 1;

                if(GetPreHitTime() < perfectPrehitTime) {
                    PopManager.Instance.DoPerfPop(left, false);
                    numPerfect += 1;
                    crowd.OnPerfectHit();
                    score.HitNote300();
                } else if(GetPreHitTime() < goodPrehitTime) {
                    PopManager.Instance.DoGoodPop(left, false);
                    numGood += 1;
                    score.HitNote200();
                } else {
                    PopManager.Instance.DoOkPop(left, false);
                    crowd.OnOkHit();
                    numOk += 1;
                    score.HitNote100();
                }

                if(currentBeatmap.map[nextBeat].beatsHeld != 0) {
                    var side = right;
                    heldNotes.Add((side, nextBeat, 0));
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
        this.heldNotes = new List<(bool, int, float)>();
        BeatmapDrawer.Instance.ResetBeatsDrawn();
        song = FMODUnity.RuntimeManager.CreateInstance(currentBeatmap.songEvent);
        song.start();
        song.setPaused(true);
        crowd.StartNewSong();

        score = new ScoreHolder();

        numHit = 0;
        numNotes = 0;
        numPerfect = 0;
        numGood = 0;
        numOk = 0;
        numMissed = 0;
        maxAngle = 0;
        sbrcount = 0;
        currentStreak = 0;
        maxStreak = 0;
        currentBeat = -1;
        holdForceHandle = SimulatorManager.Instance.currentSim.StartForce(0);

        programsForSong = new Dictionary<int, List<SkyEventProgram>>();
        if(currentBeatmap.events != null) {
            foreach(var programPair in currentBeatmap.events) {
                foreach(var source in programPair.Value) {
                    if(programsForSong.TryGetValue(programPair.Key, out var listForKey)) {
                        listForKey.Add(SkyEventManager.Instance.CreateProgram(source));
                    }
                }
            }
        }

        succeedEvents = new List<SkyEventProgram>();
        failEvents = new List<SkyEventProgram>();

        if(currentBeatmap.presongEvent != null) {
            foreach(var pse in currentBeatmap.presongEvent) {
                SkyEventManager.Instance.CreateAndExecuteProgram(pse);
            }
        }
        if(currentBeatmap.succeedEvent != null) {
            foreach(var se in currentBeatmap.succeedEvent) {
                succeedEvents.Add(SkyEventManager.Instance.CreateProgram(se));
            }
        }
        if(currentBeatmap.failEvent != null) {
            foreach(var fe in currentBeatmap.failEvent) {
                failEvents.Add(SkyEventManager.Instance.CreateProgram(fe));
            }
        }
    }

    public void StartNewSong() {
        prestartStartedAt = Time.time;
        inPrestart = true;
        StartCoroutine(StartSongCoroutine());
        StartCoroutine(StartSongCameraCoroutine());

        FMODUnity.RuntimeManager.PlayOneShot(playEvent);
    }

    private System.Collections.IEnumerator StartSongCoroutine() {
        yield return new WaitForSeconds(preStartTime);

        if(GameManager.Instance.state == GameManager.GameState.Play || GameManager.Instance.state == GameManager.GameState.Test) {
            inPrestart = false;
            songIsRunning = true;
            songStartTime = Time.time + 5;
            lastTapped = 0;
            song.setPaused(false);
            foreach(var fc in fans) {
                fc.Sync();
            }
        }
        inPrestart = false;
    }

    private System.Collections.IEnumerator StartSongCameraCoroutine() {
        float downTime = 1f;
        float stayTime = 1f;
        float upTime = 1.5f;
        float cameraMoveAmount = 7f;

        float time = 0;
        cameraController.MoveTo(new Vector2(0, -cameraMoveAmount), downTime);
        while(time < downTime) {
            time += Time.deltaTime;
            band.transform.position = Vector3.Lerp(new Vector3(0, -20f, band.transform.position.z), new Vector3(0, -16f, band.transform.position.z), Easings.EaseInOutSine(time / downTime));
            yield return null;
        }

        yield return new WaitForSeconds(stayTime);

        time = 0;
        cameraController.MoveTo(new Vector2(0, 0), upTime);
        while(time < upTime) {
            time += Time.deltaTime;
            band.transform.position = Vector3.Lerp(new Vector3(0, -16f, band.transform.position.z), new Vector3(0, -26f, band.transform.position.z), Easings.EaseInOutSine(time / upTime));
            yield return null;
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
        SimulatorManager.Instance.currentSim.EndForce(holdForceHandle);
        inPrestart = false;
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
        return currentBeatmap.GetBeatForTime(currentSongTime);
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

    public void ExecuteFailPrograms() {
        foreach(var program in failEvents) {
            SkyEventManager.Instance.ExecuteProgram(program);
        }
    }

}