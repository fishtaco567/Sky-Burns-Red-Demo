using UnityEngine;

[System.Serializable]
public class Beatmap {

    [System.Serializable]
    public struct Beat {
        public bool hasBeat;
        public int beatsHeld;
        public bool leftPowerup;
        public bool rightPowerup;
        public int powerupType;

        public Beat(bool hasBeat, int beatsHeld, bool leftPowerup, bool rightPowerup, int powerupType) {
            this.hasBeat = hasBeat;
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

    public void Setup(int measures, Vector2Int timeSignature, float tempo, string name, string difficulty, string songEvent) {
        if((timeSignature.y & (timeSignature.y - 1)) != 0) {
            Debug.Log("Invalid Time Signature");
            return;
        }

        this.numMeasures = measures;
        this.timeSignature = timeSignature;
        this.tempo = tempo;
        map = new Beat[numMeasures * sixteenthsInAMeasure];

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