using UnityEngine;
using System.Collections.Generic;

public class BeatmapDrawer : Singleton<BeatmapDrawer> {

    [SerializeField]
    protected GameObject sixteenthLine;

    [SerializeField]
    protected GameObject beatLine;

    [SerializeField]
    protected GameObject measureLine;

    [SerializeField]
    protected GameObject beat;
    [SerializeField]
    protected GameObject beatLeft;
    [SerializeField]
    protected GameObject beatRight;
    [SerializeField]
    protected float leftBeatOffset;
    [SerializeField]
    protected float rightBeatOffset;

    [SerializeField]
    protected GameObject deadBeat;
    [SerializeField]
    protected GameObject deadBeatLeft;
    [SerializeField]
    protected GameObject deadBeatRight;

    [SerializeField]
    protected GameObject holdLine;
    [SerializeField]
    protected GameObject holdHead;
    [SerializeField]
    protected GameObject holdLineDead;
    [SerializeField]
    protected GameObject holdHeadDead;
    [SerializeField]
    protected GameObject pup1;
    [SerializeField]
    protected GameObject pup2;
    [SerializeField]
    protected float pupOffset;
    [SerializeField]
    protected int beatsAhead;

    [SerializeField]
    protected Transform topAnchor;
    [SerializeField]
    protected float beatOffset;
    [SerializeField]
    protected float holdOffset;
    [SerializeField]
    protected float endHoldOffset;

    protected Animator holdSparks;
    protected Animator hitSparks;

    [SerializeField]
    protected Transform beatmap;

    [System.Serializable]
    public struct BeatObject {
        public int beat;
        public int actBeat;
        public Vector3 postAtZero;
        public GameObject obj;
        public Vector3 vel;
        public bool isPup;
        public bool left;

        public BeatObject(int beat, Vector3 postAtZero, GameObject obj, bool isPup, bool left) {
            this.beat = beat;
            this.actBeat = beat;
            this.postAtZero = postAtZero;
            this.obj = obj;
            vel = new Vector3(0, 0, 0);
            this.isPup = isPup;
            this.left = left;
        }

        public BeatObject(int beat, int actBeat, Vector3 postAtZero, GameObject obj, bool isPup, bool left) {
            this.beat = beat;
            this.actBeat = actBeat;
            this.postAtZero = postAtZero;
            this.obj = obj;
            vel = new Vector3(0, 0, 0);
            this.isPup = isPup;
            this.left = left;
        }

    }

    public List<BeatObject> beatObjects;
    protected List<BeatObject> connectedToBeat;
    protected List<BeatObject> connectedToHold;

    [SerializeField]
    protected int currentBeat;

    protected bool canFall;

    protected void Start() {
        canFall = false;
        currentBeat = -1;
        beatObjects = new List<BeatObject>();
        connectedToBeat = new List<BeatObject>();
        connectedToHold = new List<BeatObject>();
    }

    public void ResetBeatsDrawn() {
        currentBeat = -1;
        canFall = false;
        if(beatObjects != null) {
            foreach(var thing in beatObjects) {
                thing.obj.transform.SetParent(null);
                ObjectPool.Instance.DestroyObject(thing.obj);
            }
            beatObjects.Clear();
        }
        if(connectedToBeat != null) {
            foreach(var thing in connectedToBeat) {
                thing.obj.transform.SetParent(null);
                ObjectPool.Instance.DestroyObject(thing.obj);
            }
            connectedToBeat.Clear();
        }
        if(connectedToHold != null) {
            foreach(var thing in connectedToHold) {
                thing.obj.transform.SetParent(null);
                ObjectPool.Instance.DestroyObject(thing.obj);
            }
            connectedToHold.Clear();
        }
    }

    public void Stop(bool canFall) {
        this.canFall = canFall;
        if(canFall) {
            StartCoroutine(End());

            if(beatObjects != null) {
                for(int i = 0; i < beatObjects.Count; i++) {
                    var thing = beatObjects[i];
                    thing.obj.transform.position = new Vector3(Random.value < 0.5f ? -0.01f : 0.01f, thing.obj.transform.position.y, thing.obj.transform.position.z);
                    thing.obj.transform.rotation = Quaternion.Euler(0, 0, Random.value < 0.5f ? -0.1f : 0.1f);
                    thing.vel = new Vector3(Random.value - 0.5f, Random.value, 0);
                    thing.vel.x *= 5;
                    beatObjects[i] = thing;
                }
            }
            if(connectedToBeat != null) {
                for(int i = 0; i < connectedToBeat.Count; i++) {
                    var thing = connectedToBeat[i];
                    thing.obj.transform.position = new Vector3(Random.value < 0.5f ? -0.01f : 0.01f, thing.obj.transform.position.y, thing.obj.transform.position.z);
                    thing.obj.transform.rotation = Quaternion.Euler(0, 0, Random.value < 0.5f ? -0.1f : 0.1f);
                    thing.vel = new Vector3(Random.value - 0.5f, Random.value, 0);
                    thing.vel.x *= 5;
                    connectedToBeat[i] = thing;
                }
            }
            if(connectedToHold != null) {
                for(int i = 0; i < connectedToHold.Count; i++) {
                    var thing = connectedToHold[i];
                    thing.obj.transform.position = new Vector3(Random.value < 0.5f ? -0.01f : 0.01f, thing.obj.transform.position.y, thing.obj.transform.position.z);
                    thing.obj.transform.rotation = Quaternion.Euler(0, 0, Random.value < 0.5f ? -0.1f : 0.1f);
                    thing.vel = new Vector3(Random.value - 0.5f, Random.value, 0);
                    thing.vel.x *= 5;
                    connectedToHold[i] = thing;
                }
            }
        } else {
            if(beatObjects != null) {
                foreach(var thing in beatObjects) {
                    thing.obj.transform.SetParent(null);
                    ObjectPool.Instance.DestroyObject(thing.obj);
                }
                beatObjects.Clear();
            }
            if(connectedToBeat != null) {
                foreach(var thing in connectedToBeat) {
                    thing.obj.transform.SetParent(null);
                    ObjectPool.Instance.DestroyObject(thing.obj);
                }
                connectedToBeat.Clear();
            }
            if(connectedToHold != null) {
                foreach(var thing in connectedToHold) {
                    thing.obj.transform.SetParent(null);
                    ObjectPool.Instance.DestroyObject(thing.obj);
                }
                connectedToHold.Clear();
            }
        }
    }

    protected System.Collections.IEnumerator End() {
        var seconds = 0.0f;
        while(true) {
            seconds += Time.unscaledDeltaTime;
            if(seconds >= 4f) {
                break;
            }
            if(GameManager.Instance.state == GameManager.GameState.MainMenu) {
                break;
            }
            yield return null;
        }
        canFall = false;
        if(beatObjects != null) {
            foreach(var thing in beatObjects) {
                thing.obj.transform.SetParent(null);
                thing.obj.transform.rotation = Quaternion.Euler(0, 0, 0);
                ObjectPool.Instance.DestroyObject(thing.obj);
            }
            beatObjects.Clear();
        }
        if(connectedToBeat != null) {
            foreach(var thing in connectedToBeat) {
                thing.obj.transform.SetParent(null);
                thing.obj.transform.rotation = Quaternion.Euler(0, 0, 0);
                ObjectPool.Instance.DestroyObject(thing.obj);
            }
            connectedToBeat.Clear();
        }
        if(connectedToHold != null) {
            foreach(var thing in connectedToHold) {
                thing.obj.transform.rotation = Quaternion.Euler(0, 0, 0);
                thing.obj.transform.SetParent(null);
                ObjectPool.Instance.DestroyObject(thing.obj);
            }
            connectedToHold.Clear();
        }
    }

    protected void Update() {
        if(canFall && !BeatmapController.Instance.songIsRunning) {
            if(beatObjects != null) {
                for(int i = 0; i < beatObjects.Count; i++) {
                    var thing = beatObjects[i];
                    thing.obj.transform.position = thing.obj.transform.position + thing.vel * Time.deltaTime;
                    thing.obj.transform.rotation = Quaternion.Euler(0, 0, thing.obj.transform.eulerAngles.z + Mathf.Sign(thing.obj.transform.eulerAngles.z) * Time.deltaTime * 10 * thing.vel.x);
                    thing.vel = thing.vel - new Vector3(0, 9.81f, 0) * Time.deltaTime;
                    beatObjects[i] = thing;
                }
            }
            if(connectedToBeat != null) {
                for(int i = 0; i < connectedToBeat.Count; i++) {
                    var thing = connectedToBeat[i];
                    thing.obj.transform.position = thing.obj.transform.position + thing.vel * Time.deltaTime;
                    thing.obj.transform.rotation = Quaternion.Euler(0, 0, thing.obj.transform.eulerAngles.z + Mathf.Sign(thing.obj.transform.eulerAngles.z) * Time.deltaTime * 10 * thing.vel.x);
                    thing.vel = thing.vel - new Vector3(0, 9.81f, 0) * Time.deltaTime;
                    connectedToBeat[i] = thing;
                }
            }
            if(connectedToHold != null) {
                for(int i = 0; i < connectedToHold.Count; i++) {
                    var thing = connectedToHold[i];
                    thing.obj.transform.position = thing.obj.transform.position + thing.vel * Time.deltaTime;
                    thing.obj.transform.rotation = Quaternion.Euler(0, 0, thing.obj.transform.eulerAngles.z + Mathf.Sign(thing.obj.transform.eulerAngles.z) * Time.deltaTime * 10 * thing.vel.x);
                    thing.vel = thing.vel - new Vector3(0, 9.81f, 0) * Time.deltaTime;
                    connectedToHold[i] = thing;
                }
            }
        }

        var beatNum = BeatmapController.Instance.GetBeatNumber();

        while(currentBeat < beatNum + beatsAhead && (BeatmapController.Instance.songIsRunning || BeatmapController.Instance.inPrestart)) {
            currentBeat++;

            if(currentBeat < BeatmapController.Instance.currentBeatmap.map.Length) {
                var beatFromMap = BeatmapController.Instance.currentBeatmap.map[currentBeat];

                //measure
                if(currentBeat % BeatmapController.Instance.currentBeatmap.sixteenthsInAMeasure == 0) {
                    var newThing = ObjectPool.Instance.GetObject(measureLine);
                    newThing.transform.rotation = Quaternion.identity;
                    newThing.GetComponentInChildren<TMPro.TMP_Text>().text = (currentBeat / BeatmapController.Instance.currentBeatmap.sixteenthsInAMeasure + 1).ToString();
                    newThing.transform.parent = beatmap;
                    beatObjects.Add(new BeatObject(currentBeat, topAnchor.position + new Vector3(0, 0, 1.5f), newThing, false, false));
                } else if(currentBeat % BeatmapController.Instance.currentBeatmap.sixteenthsInABeat == 0) { //beat
                    var newThing = ObjectPool.Instance.GetObject(beatLine);
                    newThing.transform.rotation = Quaternion.identity;
                    newThing.transform.parent = beatmap;
                    beatObjects.Add(new BeatObject(currentBeat, topAnchor.position + new Vector3(0, 0, 1.5f), newThing, false, false));
                } else { //sixteenth
                    var newThing = ObjectPool.Instance.GetObject(sixteenthLine);
                    newThing.transform.rotation = Quaternion.identity;
                    newThing.transform.parent = beatmap;
                    beatObjects.Add(new BeatObject(currentBeat, topAnchor.position + new Vector3(0, 0, 1.5f), newThing, false, false));
                }

                switch(beatFromMap.beat) {
                    case BeatType.Center: {
                        var newThing = ObjectPool.Instance.GetObject(beat);
                        newThing.transform.rotation = Quaternion.identity;
                        newThing.transform.parent = beatmap;
                        connectedToBeat.Add(new BeatObject(currentBeat, topAnchor.position + new Vector3(0, 0, 0), newThing, false, false));
                        if(beatFromMap.beatsHeld != 0) {
                            float offset = 0;
                            for(int i = 0; i < beatFromMap.beatsHeld - 1; i++) {
                                connectedToHold.Add(new BeatObject(currentBeat + i, currentBeat, topAnchor.position + new Vector3(0, offset, 1), ObjectPool.Instance.GetObject(holdLine), false, false));
                                connectedToHold.Add(new BeatObject(currentBeat + i, currentBeat, topAnchor.position + new Vector3(0, offset + 1, 1), ObjectPool.Instance.GetObject(holdLine), false, false));
                            }
                            connectedToHold.Add(new BeatObject(currentBeat + beatFromMap.beatsHeld, currentBeat,
                                topAnchor.position + new Vector3(0, offset - endHoldOffset, 1), ObjectPool.Instance.GetObject(holdHead), false, false));
                        }
                        break;
                    }
                    case BeatType.Left: {
                        var newThing = ObjectPool.Instance.GetObject(beatLeft);
                        newThing.transform.rotation = Quaternion.identity;
                        newThing.transform.parent = beatmap;
                        connectedToBeat.Add(new BeatObject(currentBeat, topAnchor.position + new Vector3(leftBeatOffset, 0, 0), newThing, false, false));
                        if(beatFromMap.beatsHeld != 0) {
                            float offset = 0;
                            for(int i = 0; i < beatFromMap.beatsHeld - 1; i++) {
                                connectedToHold.Add(new BeatObject(currentBeat + i, currentBeat, topAnchor.position + new Vector3(leftBeatOffset, offset, 1), ObjectPool.Instance.GetObject(holdLine), false, false));
                                connectedToHold.Add(new BeatObject(currentBeat + i, currentBeat, topAnchor.position + new Vector3(leftBeatOffset, offset + 1, 1), ObjectPool.Instance.GetObject(holdLine), false, false));
                            }
                            connectedToHold.Add(new BeatObject(currentBeat + beatFromMap.beatsHeld, currentBeat,
                                topAnchor.position + new Vector3(leftBeatOffset, offset - endHoldOffset, 1), ObjectPool.Instance.GetObject(holdHead), false, false));
                        }
                        break;
                    }
                    case BeatType.Right: {
                        var newThing = ObjectPool.Instance.GetObject(beatRight);
                        newThing.transform.rotation = Quaternion.identity;
                        newThing.transform.parent = beatmap;
                        connectedToBeat.Add(new BeatObject(currentBeat, topAnchor.position + new Vector3(rightBeatOffset, 0, 0), newThing, false, false));
                        if(beatFromMap.beatsHeld != 0) {
                            float offset = 0;
                            for(int i = 0; i < beatFromMap.beatsHeld - 1; i++) {
                                connectedToHold.Add(new BeatObject(currentBeat + i + 1, currentBeat, topAnchor.position + new Vector3(rightBeatOffset, offset, 1), ObjectPool.Instance.GetObject(holdLine), false, false));
                                connectedToHold.Add(new BeatObject(currentBeat + i + 1, currentBeat, topAnchor.position + new Vector3(rightBeatOffset, offset + 1, 1), ObjectPool.Instance.GetObject(holdLine), false, false));
                            }
                            connectedToHold.Add(new BeatObject(currentBeat + beatFromMap.beatsHeld, currentBeat,
                                topAnchor.position + new Vector3(rightBeatOffset, offset - endHoldOffset, 1), ObjectPool.Instance.GetObject(holdHead), false, false));
                        }
                        break;
                    }
                }

                if(beatFromMap.leftPowerup) {
                    var newPup = ObjectPool.Instance.GetObject(beatFromMap.powerupType == 0 ? pup1 : pup2);
                    newPup.transform.SetParent(beatmap);
                    newPup.transform.rotation = Quaternion.identity;
                    connectedToBeat.Add(new BeatObject(currentBeat, topAnchor.position + new Vector3(-pupOffset, 0, -1), newPup, true, true));
                }
                if(beatFromMap.rightPowerup) {
                    var newPup = ObjectPool.Instance.GetObject(beatFromMap.powerupType == 0 ? pup1 : pup2);
                    newPup.transform.SetParent(beatmap);
                    newPup.transform.rotation = Quaternion.identity;
                    connectedToBeat.Add(new BeatObject(currentBeat, topAnchor.position + new Vector3(pupOffset, 0, -1), newPup, true, false));
                }
            }
        }

        if(BeatmapController.Instance.songIsRunning || BeatmapController.Instance.inPrestart) {
            for(int i = beatObjects.Count - 1; i >= 0; i--) {
                if(beatObjects[i].beat < beatNum - 4) {
                    beatObjects[i].obj.transform.SetParent(null);
                    ObjectPool.Instance.DestroyObject(beatObjects[i].obj);
                    beatObjects.RemoveAt(i);

                    continue;
                }

                var beatDif = beatObjects[i].beat * BeatmapController.Instance.currentBeatmap.sixteenthTime - BeatmapController.Instance.currentSongTime;
                beatDif = beatDif / BeatmapController.Instance.currentBeatmap.sixteenthTime;

                beatObjects[i].obj.transform.position = beatObjects[i].postAtZero + new Vector3(0, beatOffset * beatDif, 0);
            }

            for(int i = connectedToBeat.Count - 1; i >= 0; i--) {
                if(connectedToBeat[i].beat < beatNum - 4) {
                    connectedToBeat[i].obj.transform.SetParent(null);
                    ObjectPool.Instance.DestroyObject(connectedToBeat[i].obj);
                    connectedToBeat.RemoveAt(i);

                    continue;
                }

                if(BeatmapController.Instance.consumeArray[connectedToBeat[i].beat]) {
                    var thing = connectedToBeat[i];
                    if(thing.isPup) {
                        if(thing.left && BeatmapController.Instance.leftPup[thing.beat]) {
                            connectedToBeat[i].obj.transform.SetParent(null);
                            ObjectPool.Instance.DestroyObject(connectedToBeat[i].obj);
                            connectedToBeat.RemoveAt(i);
                        }
                        if(!thing.left && BeatmapController.Instance.rightPup[thing.beat]) {
                            connectedToBeat[i].obj.transform.SetParent(null);
                            ObjectPool.Instance.DestroyObject(connectedToBeat[i].obj);
                            connectedToBeat.RemoveAt(i);
                        }

                    } else {
                        connectedToBeat[i].obj.transform.SetParent(null);
                        ObjectPool.Instance.DestroyObject(connectedToBeat[i].obj);
                        if(thing.isPup == false) {
                            var beatFab = deadBeat;
                            if(BeatmapController.Instance.currentBeatmap.map[thing.beat].beat == BeatType.Left) {
                                beatFab = deadBeatLeft;
                            } else if(BeatmapController.Instance.currentBeatmap.map[thing.beat].beat == BeatType.Right) {
                                beatFab = deadBeatRight;
                            }
                            var newThing = ObjectPool.Instance.GetObject(beatFab);
                            newThing.transform.SetParent(beatmap);
                            thing.obj = newThing;
                            connectedToBeat[i] = thing;
                        } else {
                            connectedToBeat.RemoveAt(i);
                        }
                    }
                }

                var beatDif = connectedToBeat[i].beat * BeatmapController.Instance.currentBeatmap.sixteenthTime - BeatmapController.Instance.currentSongTime;
                beatDif = beatDif / BeatmapController.Instance.currentBeatmap.sixteenthTime;

                connectedToBeat[i].obj.transform.position = connectedToBeat[i].postAtZero + new Vector3(0, beatOffset * beatDif, 0);
            }

            for(int i = connectedToHold.Count - 1; i >= 0; i--) {
                if(connectedToHold[i].beat < beatNum - 4) {
                    ObjectPool.Instance.DestroyObject(connectedToHold[i].obj);
                    connectedToHold.RemoveAt(i);

                    continue;
                }

                if(BeatmapController.Instance.heldKilled[connectedToHold[i].actBeat]) {
                    var isEnd = connectedToHold[i].obj.name == "HeldNoteEndFlipped";
                    var isDead = connectedToHold[i].obj.name.Contains("Dead");
                    if(!isDead) {
                        ObjectPool.Instance.DestroyObject(connectedToHold[i].obj);
                        var beatFab = holdLineDead;
                        if(isEnd) {
                            beatFab = holdHeadDead;
                        }
                        var thing = connectedToHold[i];
                        thing.obj = ObjectPool.Instance.GetObject(beatFab);
                        connectedToHold[i] = thing;
                    }
                }

                var beatDif = connectedToHold[i].beat * BeatmapController.Instance.currentBeatmap.sixteenthTime - BeatmapController.Instance.currentSongTime;
                beatDif = beatDif / BeatmapController.Instance.currentBeatmap.sixteenthTime;

                connectedToHold[i].obj.transform.position = connectedToHold[i].postAtZero + new Vector3(0, beatOffset * beatDif, .5f);
            }

            foreach(var held in BeatmapController.Instance.heldNotes) {
                for(int i = 0; i < connectedToHold.Count; i++) {
                    if(connectedToHold[i].actBeat == held.Item2) {
                        if(held.Item1) {
                            var beat = connectedToHold[i];
                            beat.postAtZero.x = rightBeatOffset;
                            connectedToHold[i] = beat;
                        } else {
                            var beat = connectedToHold[i];
                            beat.postAtZero.x = leftBeatOffset;
                            connectedToHold[i] = beat;
                        }
                    }
                }
            }
        }
    }

}
