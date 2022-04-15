using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Beatmap {

    [System.Serializable]
    public struct Beat {
        public BeatType beat;
        public int beatsHeld;
        public bool leftPowerup;
        public bool rightPowerup;
        public int powerupType;

        public Beat(BeatType beat, int beatsHeld, bool leftPowerup, bool rightPowerup, int powerupType) {
            this.beat = beat;
            this.beatsHeld = beatsHeld;
            this.leftPowerup = leftPowerup;
            this.rightPowerup = rightPowerup;
            this.powerupType = powerupType;
        }
    }

    public Beat[] map;

    public Vector2Int timeSignature;

    public float tempo;
    public int numMeasures;

    public string name;
    public string difficulty;
    public string songEvent;

    public Dictionary<int, string[]> events;
    public Dictionary<int, float> tempoChanges;
    public Dictionary<int, Vector2Int> timeSignatureChanges;

    public List<string> presongEvent;
    public List<string> succeedEvent;
    public List<string> failEvent;

    public string version = "0.0.1";

    public int sixteenthsInABeat {
        get {
            return 16 / timeSignature.y;
        }
    }

    public int sixteenthsInAMeasure {
        get {
            return timeSignature.x * sixteenthsInABeat;
        }
    }

    public float sixteenthTime {
        get {
            return (60 / (tempo * sixteenthsInABeat));
        }
    }

    public float songTime {
        get {
            return sixteenthTime * sixteenthsInAMeasure * numMeasures;
        }
    }

    public float GetTimeOfHeldEnd(int beat) {
        return beat * sixteenthTime + map[beat].beatsHeld * sixteenthTime;
    }

    public int GetBeatForTime(float time) {
        return Mathf.FloorToInt(time / sixteenthTime);
    }

    public void Setup(int measures, Vector2Int timeSignature, float tempo, string name, string difficulty, string songEvent) {
        if((timeSignature.y & (timeSignature.y - 1)) != 0) {
            Debug.Log("Invalid Time Signature");
            return;
        }

        this.numMeasures = measures;
        this.timeSignature = timeSignature;
        this.tempo = tempo;
        map = new Beat[numMeasures * sixteenthsInAMeasure];
        events = new Dictionary<int, string[]>();
        timeSignatureChanges = new Dictionary<int, Vector2Int>();
        tempoChanges = new Dictionary<int, float>();
        presongEvent = new List<string>();
        succeedEvent = new List<string>();
        failEvent = new List<string>();

        this.name = name;
        this.difficulty = difficulty;
        this.songEvent = songEvent;
    }

    public void AutoResize() {
        Resize(numMeasures);
    }

    public void Resize(int measures) {
        System.Array.Resize(ref map, measures * sixteenthsInAMeasure);
    }

    public void ChangeTimeSignature(Vector2Int timeSignature) {
        if((timeSignature.y & (timeSignature.y - 1)) != 0) {
            Debug.Log("Invalid Time Signature");
            return;
        }

        this.timeSignature = timeSignature;
        Resize(numMeasures);
    }

}