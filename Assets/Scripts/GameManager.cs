using UnityEngine;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager> {

    public enum GameState {
        MainMenu,
        Edit,
        Test,
        Play,
        Pause,
        Results,
        Fail
    }

    [SerializeField]
    protected GameObject[] mainMenuRoots;
    [SerializeField]
    protected GameObject[] editMenuRoots;
    [SerializeField]
    protected GameObject[] testRoots;
    [SerializeField]
    protected GameObject[] playRoots;
    [SerializeField]
    protected GameObject[] pauseRoots;
    [SerializeField]
    protected GameObject[] resultsRoots;
    [SerializeField]
    protected GameObject[] failRoots;

    public GameState state;
    protected GameState lastState;

    protected void Start() {
        GoToMainMenu();
    }

    public void OpenEditMode(Beatmap b) {
        ActiveAll(mainMenuRoots, false);
        ActiveAll(testRoots, false);
        ActiveAll(playRoots, false);
        ActiveAll(pauseRoots, false);
        ActiveAll(resultsRoots, false);
        ActiveAll(failRoots, false);
        ActiveAll(editMenuRoots, true);
        state = GameState.Edit;
        BeatmapEditorDrawer.Instance.StartEditorForBeatmap(b);
        BeatmapController.Instance.EndSong(false);
    }

    public void OpenPlayMode(Beatmap b) {
        ActiveAll(mainMenuRoots, false);
        ActiveAll(editMenuRoots, false);
        ActiveAll(testRoots, false);
        ActiveAll(pauseRoots, false);
        ActiveAll(resultsRoots, false);
        ActiveAll(failRoots, false);
        ActiveAll(playRoots, true);
        state = GameState.Play;
        BeatmapController.Instance.SetupNewBeatmap(b);
        BeatmapController.Instance.StartNewSong();
    }

    public void OpenTestMode(Beatmap b) {
        ActiveAll(mainMenuRoots, false);
        ActiveAll(editMenuRoots, false);
        ActiveAll(playRoots, false);
        ActiveAll(pauseRoots, false);
        ActiveAll(resultsRoots, false);
        ActiveAll(failRoots, false);
        ActiveAll(testRoots, true);
        state = GameState.Test;
        BeatmapController.Instance.SetupNewBeatmap(b);
        BeatmapController.Instance.StartNewSong();
    }

    public void OpenPauseMode() {
        ActiveAll(pauseRoots, true);
        lastState = state;
        state = GameState.Pause;
        Time.timeScale = 0;
        BeatmapController.Instance.PauseSong();
    }

    public void ClosePauseMode() {
        ActiveAll(pauseRoots, false);
        state = lastState;
        Time.timeScale = 1;
        BeatmapController.Instance.RestartSong();
    }

    public void GoToMainMenu() {
        state = GameState.MainMenu;
        ActiveAll(editMenuRoots, false);
        ActiveAll(testRoots, false);
        ActiveAll(playRoots, false);
        ActiveAll(pauseRoots, false);
        ActiveAll(resultsRoots, false);
        ActiveAll(failRoots, false);
        ActiveAll(mainMenuRoots, true);
        state = GameState.MainMenu;
        MainMenuController.Instance.SetupMainMenu();
        BeatmapController.Instance.EndSong(false);
    }

    public void ResultsScreen(int numPerf, int numGood, int numOk, int numMiss, float percent, float maxAngle) {
        state = GameState.Results;
        ActiveAll(editMenuRoots, false);
        ActiveAll(testRoots, false);
        ActiveAll(playRoots, false);
        ActiveAll(pauseRoots, false);
        ActiveAll(mainMenuRoots, false);
        ActiveAll(failRoots, false);
        ActiveAll(resultsRoots, true);
        ResultsScreenDrawer.Instance.Setup(numPerf, numGood, numOk, numMiss, percent, maxAngle);
    }

    public void ToFailScreen(float seconds) {
        state = GameState.Fail;
        ActiveAll(editMenuRoots, false);
        ActiveAll(testRoots, false);
        ActiveAll(playRoots, false);
        ActiveAll(pauseRoots, false);
        ActiveAll(mainMenuRoots, false);
        ActiveAll(resultsRoots, false);
        ActiveAll(failRoots, true);
        FailScreen.Instance.Setup(seconds);
    }

    public void ActiveAll(GameObject[] a, bool b) {
        foreach(var go in a) {
            go.SetActive(b);
        }
    }

}
