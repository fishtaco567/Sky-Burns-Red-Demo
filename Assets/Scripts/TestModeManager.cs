using UnityEngine;
using System.Collections.Generic;

public class TestModeManager : MonoBehaviour {

    public void ToEditor() {
        GameManager.Instance.OpenEditMode(BeatmapController.Instance.currentBeatmap);
        BeatmapEditorDrawer.Instance.SetCurrentBar(BeatmapController.Instance.GetBeatNumber());
    }

}
