using UnityEngine;
using System.Collections.Generic;
using Rewired;
using System.IO;
using UnityEngine.EventSystems;
using System.Linq;

public class BeatmapEditorDrawer : Singleton<BeatmapEditorDrawer> {

    public struct EditorBeatObject {
        public int beat;
        public GameObject obj;

        public EditorBeatObject(int beat, GameObject obj) {
            this.beat = beat;
            this.obj = obj;
        }
    }

    [SerializeField]
    protected GameObject sixteenthLine;

    [SerializeField]
    protected GameObject beatLine;

    [SerializeField]
    protected GameObject measureLine;

    [SerializeField]
    protected GameObject beat;

    [SerializeField]
    protected GameObject holdLine;
    [SerializeField]
    protected GameObject holdHead;
    [SerializeField]
    protected GameObject pup1;
    [SerializeField]
    protected GameObject pup2;
    [SerializeField]
    protected float pupOffset;

    public List<EditorBeatObject> beatObjects;

    public List<EditorBeatObject> bars;

    public GameObject baseObject;

    public Beatmap currentBeatmap;

    [SerializeField]
    protected float barOffset;
    [SerializeField]
    protected float holdOffset;
    [SerializeField]
    protected float holdEndOffset;
    [SerializeField]
    protected float beatOffset;

    protected Player rewiredPlayer;

    protected int currentBar;

    [SerializeField]
    protected float easing;

    [SerializeField]
    protected TMPro.TMP_InputField measuresField;
    [SerializeField]
    protected TMPro.TMP_InputField tempoField;
    [SerializeField]
    protected TMPro.TMP_InputField topTimeSig;
    [SerializeField]
    protected TMPro.TMP_InputField bottomTimeSig;
    [SerializeField]
    protected TMPro.TMP_InputField nameField;
    [SerializeField]
    protected TMPro.TMP_Dropdown difficultyField;
    [SerializeField]
    protected TMPro.TMP_Dropdown musicEventField;

    protected void Start() {
        rewiredPlayer = ReInput.players.GetPlayer(0);
    }

    public void StartEditorForBeatmap(Beatmap b) {
        currentBeatmap = b;
        if(beatObjects == null) {
            beatObjects = new List<EditorBeatObject>();
        }

        if(bars == null) {
            bars = new List<EditorBeatObject>();
        }
        RedrawEditor(b);

        measuresField.text = currentBeatmap.numMeasures.ToString();
        tempoField.text = currentBeatmap.tempo.ToString();
        topTimeSig.text = currentBeatmap.timeSignature.x.ToString();
        bottomTimeSig.text = currentBeatmap.timeSignature.y.ToString();
        nameField.text = currentBeatmap.name;

        var difFieldText = difficultyField.options.Select(x => x.text).ToList();
        var musFieldText = musicEventField.options.Select(x => x.text).ToList();

        difficultyField.value = difFieldText.IndexOf(currentBeatmap.difficulty);
        musicEventField.value = musFieldText.IndexOf(currentBeatmap.songEvent);
    }

    public void Update() {
        if(EventSystem.current?.currentSelectedGameObject?.GetComponent<TMPro.TMP_InputField>() != null) {
            return;
        }
        var click = rewiredPlayer.GetButtonDown("Click");
        var up = rewiredPlayer.GetButtonDown("Up");
        var down = rewiredPlayer.GetButtonDown("Down");
        var left = rewiredPlayer.GetButtonDown("Left");
        var right = rewiredPlayer.GetButtonDown("Right");
        var save = rewiredPlayer.GetButtonDown("Save");
        var lpup1 = rewiredPlayer.GetButtonDown("LeftPup1");
        var lpup2 = rewiredPlayer.GetButtonDown("LeftPup2");
        var rpup1 = rewiredPlayer.GetButtonDown("RightPup1");
        var rpup2 = rewiredPlayer.GetButtonDown("RightPup2");

        if(click) {
            ClickBar(currentBar);
        }

        if(up) {
            currentBar--;
        }

        if(down) {
            currentBar++;
        }

        if(left) {
            currentBar -= currentBeatmap.sixteenthsInAMeasure;
        }
        
        if(right) {
            currentBar += currentBeatmap.sixteenthsInAMeasure;
        }

        if(save) {
            Save();
        }

        if(lpup1 || lpup2 || rpup1 || rpup2) {
            Pup((lpup1 || lpup2), (rpup1 || lpup1), currentBar);
        }

        currentBar = Mathf.Clamp(currentBar, 0, currentBeatmap.map.Length - 1);

        baseObject.transform.position = Vector3.Lerp(baseObject.transform.position, new Vector3(0, currentBar * barOffset + 1, 0), easing);
    }

    public void Pup(bool left, bool type, int bar) {
        if(bar > 0 || bar <= currentBeatmap.map.Length) {
            currentBeatmap.map[bar].powerupType = type ? 0 : 1;
            if(left) {
                currentBeatmap.map[bar].leftPowerup = !currentBeatmap.map[bar].leftPowerup;
            } else {
                currentBeatmap.map[bar].rightPowerup = !currentBeatmap.map[bar].rightPowerup;
            }

            RedrawEditor(currentBeatmap);
        }
    }

    public void Save() {
        UpdateBeatmapButton();
        currentBeatmap.name = nameField.text;
        var difs = difficultyField.options[difficultyField.value].text;
        difs = difs.Substring(difs.Length - 3);
        var filename = Application.persistentDataPath + "/" + nameField.text + difs + ".json";
        var json = JsonUtility.ToJson(currentBeatmap);
        File.WriteAllText(filename, json);
    }

    public void RedrawEditor(Beatmap b) {
        foreach(EditorBeatObject obj in beatObjects) {
            obj.obj.transform.SetParent(null);
            ObjectPool.Instance.DestroyObject(obj.obj);
        }
        beatObjects.Clear();

        foreach(EditorBeatObject obj in bars) {
            obj.obj.transform.SetParent(null);
            ObjectPool.Instance.DestroyObject(obj.obj);
        }
        bars.Clear();

        for(int i = 0; i < currentBeatmap.map.Length; i++) {
            if(i % currentBeatmap.sixteenthsInAMeasure == 0) {
                var newBar = ObjectPool.Instance.GetObject(measureLine);
                newBar.transform.SetParent(baseObject.transform);
                newBar.transform.localPosition = new Vector3(0, -i * barOffset, 0);
                newBar.GetComponentInChildren<TMPro.TMP_Text>().text = (i / currentBeatmap.sixteenthsInAMeasure + 1).ToString();
                bars.Add(new EditorBeatObject(i, newBar));
            } else if(i % currentBeatmap.sixteenthsInABeat == 0) {
                var newBar = ObjectPool.Instance.GetObject(beatLine);
                newBar.transform.SetParent(baseObject.transform);
                newBar.transform.localPosition = new Vector3(0, -i * barOffset, 0);
                bars.Add(new EditorBeatObject(i, newBar));
            } else {
                var newBar = ObjectPool.Instance.GetObject(sixteenthLine);
                newBar.transform.SetParent(baseObject.transform);
                newBar.transform.localPosition = new Vector3(0, -i * barOffset, 0);
                bars.Add(new EditorBeatObject(i, newBar));
            }

            if(currentBeatmap.map[i].hasBeat) {
                var newBeat = ObjectPool.Instance.GetObject(beat);
                newBeat.transform.SetParent(baseObject.transform);
                newBeat.transform.localPosition = new Vector3(0, -i * barOffset - beatOffset, 0);
                beatObjects.Add(new EditorBeatObject(i, newBeat));
                if(currentBeatmap.map[i].beatsHeld != 0) {
                    var offset = 0f;
                    for(int j = 0; j < currentBeatmap.map[j].beatsHeld - 1; j++) {
                        offset -= holdOffset;
                        var newHold1 = ObjectPool.Instance.GetObject(holdLine);
                        newHold1.transform.SetParent(baseObject.transform);
                        newHold1.transform.localPosition = new Vector3(0, -i * barOffset - offset - beatOffset, 0);
                        beatObjects.Add(new EditorBeatObject(i, newHold1));
                        offset -= holdOffset;
                        var newHold2 = ObjectPool.Instance.GetObject(holdLine);
                        newHold2.transform.SetParent(baseObject.transform);
                        newHold2.transform.localPosition = new Vector3(0, -i * barOffset - offset - beatOffset, 0);
                        beatObjects.Add(new EditorBeatObject(i, newHold2));
                    }

                    var holdEndObj = ObjectPool.Instance.GetObject(holdHead);
                    holdEndObj.transform.SetParent(baseObject.transform);
                    holdEndObj.transform.localPosition = new Vector3(0, -i * barOffset - offset - beatOffset, 0);
                    beatObjects.Add(new EditorBeatObject(i, holdEndObj));
                }
            }

            if(currentBeatmap.map[i].leftPowerup) {
                var newPup = ObjectPool.Instance.GetObject(currentBeatmap.map[i].powerupType == 0 ? pup1 : pup2);
                newPup.transform.SetParent(baseObject.transform);
                newPup.transform.localPosition = new Vector3(-pupOffset, -i * barOffset - beatOffset, -1);
                beatObjects.Add(new EditorBeatObject(i, newPup));
            }
            if(currentBeatmap.map[i].rightPowerup) {
                var newPup = ObjectPool.Instance.GetObject(currentBeatmap.map[i].powerupType == 0 ? pup1 : pup2);
                newPup.transform.SetParent(baseObject.transform);
                newPup.transform.localPosition = new Vector3(pupOffset, -i * barOffset - beatOffset, -1);
                beatObjects.Add(new EditorBeatObject(i, newPup));
            }
        }
    }

    public void ClickBar(int bar) {
        if(bar > 0 || bar <= currentBeatmap.map.Length) {
            bool hasBeat = currentBeatmap.map[bar].hasBeat;

            if(hasBeat) {
                currentBeatmap.map[bar] = new Beatmap.Beat();
                for(int i = beatObjects.Count - 1; i >= 0; i--) {
                    if(beatObjects[i].beat == bar) {
                        beatObjects[i].obj.transform.SetParent(null);
                        ObjectPool.Instance.DestroyObject(beatObjects[i].obj);
                        beatObjects.RemoveAt(i);
                    }
                }
            } else {
                currentBeatmap.map[bar] = new Beatmap.Beat(true, 0, false, false, 0);
                var newBeat = ObjectPool.Instance.GetObject(beat);
                newBeat.transform.SetParent(baseObject.transform);
                newBeat.transform.localPosition = new Vector3(0, -bar * barOffset - beatOffset, 0);
                beatObjects.Add(new EditorBeatObject(bar, newBeat));
            }
        }
    }

    public void UpdateBeatmapButton() {
        if(int.TryParse(measuresField.text, out int newMeasures)) {
            currentBeatmap.numMeasures = newMeasures;
        }

        if(int.TryParse(tempoField.text, out int newTempo)) {
            currentBeatmap.tempo = newTempo;
        }

        if(int.TryParse(topTimeSig.text, out int topTimeSigNew)) {
            currentBeatmap.timeSignature = new Vector2Int(topTimeSigNew, currentBeatmap.timeSignature.y);
        }

        if(int.TryParse(bottomTimeSig.text, out int bottomTimeSigNew)) {
            currentBeatmap.timeSignature = new Vector2Int(currentBeatmap.timeSignature.x, bottomTimeSigNew);
        }

        currentBeatmap.name = nameField.text;
        currentBeatmap.difficulty = difficultyField.options[difficultyField.value].text;
        currentBeatmap.songEvent = musicEventField.options[musicEventField.value].text;
        currentBeatmap.AutoResize();

        RedrawEditor(currentBeatmap);
    }

    public void SaveButton() {
        Save();
    }

    public void SetCurrentBar(int bar) {
        currentBar = bar;
    }

    public void TestButton() {
        GameManager.Instance.OpenTestMode(currentBeatmap);
        BeatmapController.Instance.SetCurrentBarWithLeadIn(currentBar);
    }

    public void ReturnButton() {
        GameManager.Instance.GoToMainMenu();
    }

}
