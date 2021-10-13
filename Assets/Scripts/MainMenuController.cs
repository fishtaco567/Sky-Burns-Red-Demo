using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class MainMenuController : Singleton<MainMenuController> {

    [SerializeField]
    protected GameObject beatmapSelectPrefab;

    [SerializeField]
    protected GameObject beatmapSelectRoot;

    [SerializeField]
    protected TextAsset[] preloadedBeatmaps;

    [SerializeField]
    protected float buttonOffset;

    public List<Beatmap> currentBeatmaps;

    public void SetupMainMenu() {
        StartSliders();
        for(int i = beatmapSelectRoot.transform.childCount - 1; i >= 0; i--) {
            Destroy(beatmapSelectRoot.transform.GetChild(i).gameObject);
        }

        currentBeatmaps = GetBeatmaps();

        for(int i = 0; i < currentBeatmaps.Count; i++) {
            int q = i;
            var inst = Instantiate(beatmapSelectPrefab);
            inst.transform.SetParent(beatmapSelectRoot.transform, false);
            inst.transform.localPosition = new Vector3(0, -i * buttonOffset, 0);
            var comp = inst.GetComponent<BeatmapSelector>();
            var difficultyString = currentBeatmaps[i].difficulty;
            if(difficultyString == "Epic") {
                difficultyString = string.Format("{0}</color>", difficultyString);
            } else if(difficultyString == "Fine") {
                difficultyString = string.Format("{0}</color>", difficultyString);
            } else if(difficultyString == "Meh") {
                difficultyString = string.Format("{0}</color>", difficultyString);
            }

            var seconds = currentBeatmaps[i].songTime;
            var minutes = Mathf.FloorToInt(seconds / 60);
            seconds -= minutes * 60;
            var fmt = "00.##";
            comp.titleText.text = string.Format("{0}</color>    {1}    {2}:{3:D2}", difficultyString, currentBeatmaps[i].name, minutes, seconds.ToString(fmt));
            comp.startButton.onClick.AddListener(delegate { PlayPressed(q); });
            comp.editButton.onClick.AddListener(delegate { EditPressed(q); });
        }
    }

    public List<Beatmap> GetBeatmaps() {
        var list = new List<Beatmap>();
        foreach(var ta in preloadedBeatmaps) {
            list.Add(JsonUtility.FromJson<Beatmap>(ta.text));
        }

        foreach(var file in Directory.GetFiles(Application.persistentDataPath, "*.json")) {
            list.Add(JsonUtility.FromJson<Beatmap>(File.ReadAllText(file)));
        }

        return list;
    }

    public void PlayPressed(int i) {
        GameManager.Instance.OpenPlayMode(currentBeatmaps[i]);
    }

    public void EditPressed(int i) {
        GameManager.Instance.OpenEditMode(currentBeatmaps[i]);
    }

    public void NewBeatmap() {
        var newBeatmap = new Beatmap();
        newBeatmap.Setup(100, new Vector2Int(4, 4), 120, "NewBeatmap", "Fine", "");
        GameManager.Instance.OpenEditMode(newBeatmap);
    }

    public string musicBus;
    public string sfxBus;
    public string masterBus;
    public UnityEngine.UI.Slider masterSlider;
    public UnityEngine.UI.Slider musicSlider;
    public UnityEngine.UI.Slider sfx;

    public void StartSliders() {
        FMODUnity.RuntimeManager.GetBus(musicBus).getVolume(out float mvol);
        musicSlider.value = mvol;
        FMODUnity.RuntimeManager.GetBus(sfxBus).getVolume(out float svol);
        sfx.value = svol;
        FMODUnity.RuntimeManager.GetBus(masterBus).getVolume(out float ovol);
        masterSlider.value = ovol;
    }

    public void MusicVolUpdated(float s) {
        FMODUnity.RuntimeManager.GetBus(musicBus).setVolume(musicSlider.value);
    }
    public void SoundEffectVolUpdate(float s) {
        FMODUnity.RuntimeManager.GetBus(sfxBus).setVolume(sfx.value);
    }
    public void MasterSoundVolUpdate(float s) {
        FMODUnity.RuntimeManager.GetBus(masterBus).setVolume(masterSlider.value);
    }

}