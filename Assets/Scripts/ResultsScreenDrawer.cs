using UnityEngine;
using TMPro;

public class ResultsScreenDrawer : Singleton<ResultsScreenDrawer> {

    public TMP_Text perf;
    public TMP_Text good;
    public TMP_Text ok;
    public TMP_Text miss;
    public TMP_Text percent;
    public TMP_Text angle;

    public void Setup(int perf, int good, int ok, int miss, float percent, float angle) {
        this.perf.text = perf.ToString();
        this.good.text = good.ToString();
        this.ok.text = ok.ToString();
        this.miss.text = miss.ToString();
        string fmt = "00.##";
        this.percent.text = string.Format("{0}%", (percent * 100).ToString(fmt));
        this.angle.text = string.Format("{0:G4}°", angle);
    }

    public void ReturnToMenu() {
        GameManager.Instance.GoToMainMenu();
    }

}