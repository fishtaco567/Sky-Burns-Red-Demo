using UnityEngine;
using TMPro;

public class ResultsScreenDrawer : Singleton<ResultsScreenDrawer> {

    public TMP_Text perf;
    public TMP_Text good;
    public TMP_Text ok;
    public TMP_Text miss;
    public TMP_Text percent;
    public TMP_Text score;
    public TMP_Text bestMulti;

    public CameraController cameraController;

    [SerializeField]
    protected float firstFewDelay;
    [SerializeField]
    protected float lastDelay;

    public GameObject perfObj;
    public GameObject goodObj;
    public GameObject okObj;
    public GameObject missObj;
    public GameObject percentObj;
    public GameObject scoreObj;
    public GameObject bestMultiObj;

    [FMODUnity.EventRef]
    public string normalHitSound;
    [FMODUnity.EventRef]
    public string endHitSound;

    public void Setup(int perf, int good, int ok, int miss, float percent, int score, int bestMulti) {
        this.perf.text = perf.ToString();
        this.good.text = good.ToString();
        this.ok.text = ok.ToString();
        this.miss.text = miss.ToString();
        string fmt = "00.##";
        this.percent.text = string.Format("{0}%", (percent * 100).ToString(fmt));
        this.score.text = string.Format("{0:n0}", score);
        this.bestMulti.text = string.Format("x{0:n0}", bestMulti);
        StartCoroutine(DoDraw());
    }

    public System.Collections.IEnumerator DoDraw() {
        yield return new WaitForSeconds(firstFewDelay);
        FMODUnity.RuntimeManager.PlayOneShot(normalHitSound);
        perfObj.SetActive(true);
        yield return new WaitForSeconds(firstFewDelay);
        FMODUnity.RuntimeManager.PlayOneShot(normalHitSound);
        goodObj.SetActive(true);
        yield return new WaitForSeconds(firstFewDelay);
        FMODUnity.RuntimeManager.PlayOneShot(normalHitSound);
        okObj.SetActive(true);
        yield return new WaitForSeconds(firstFewDelay);
        FMODUnity.RuntimeManager.PlayOneShot(normalHitSound);
        missObj.SetActive(true);
        yield return new WaitForSeconds(firstFewDelay);
        FMODUnity.RuntimeManager.PlayOneShot(normalHitSound);
        percentObj.SetActive(true);
        yield return new WaitForSeconds(lastDelay);
        FMODUnity.RuntimeManager.PlayOneShot(endHitSound);
        scoreObj.SetActive(true);
        bestMultiObj.SetActive(true);
    }

    public void ReturnToMenu() {
        GameManager.Instance.GoToMainMenu();
        cameraController.MoveTo(new Vector2(0, 0), 0.5f);

        perfObj.SetActive(false);
        goodObj.SetActive(false);
        okObj.SetActive(false);
        missObj.SetActive(false);
        percentObj.SetActive(false);
        scoreObj.SetActive(false);
        bestMultiObj.SetActive(false);
    }

}