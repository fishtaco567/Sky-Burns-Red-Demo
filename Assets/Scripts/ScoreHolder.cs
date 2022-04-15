public class ScoreHolder {

    public int Score {
        get {
            return _score;
        }
    }

    private int _score;

    public int Multiplier {
        get {
            return _multiplier;
        }
        set {
            _multiplier = value;
            BeatmapController.Instance.multiText.SetText(string.Format("x{0:n0}", _multiplier));
        }
    }

    private int _multiplier;

    public ScoreHolder() {
        _score = 0;
        _multiplier = 1;
        BeatmapController.Instance.multiText.SetText(string.Format("x{0:n0}", _multiplier));
        BeatmapController.Instance.scoreText.SetText(string.Format("Score: {0:n0}", _score));
    }

    private void AddScore(int add) {
        _score += add * Multiplier;
        BeatmapController.Instance.scoreText.SetText(string.Format("Score: {0:n0}", _score));
    }

    public void HitNote100() {
        AddScore(100);
        PopManager.Instance.DoScorePop("Plus100", 0, 0.4f);
    }
    public void HitNote200() {
        AddScore(200);
        PopManager.Instance.DoScorePop("Plus200", 0, 0.4f);
    }
    public void HitNote300() {
        AddScore(300);
        PopManager.Instance.DoScorePop("Plus300", 0, 0.4f);
    }
    public void HitNote1000() {
        AddScore(1000);
        PopManager.Instance.DoScorePop("Plus1000", 0, 0.4f);
    }

    public void Hit10() {
        AddScore(10);
        PopManager.Instance.DoScorePop("Plus10", 0, 0.4f);
    }

    public void PerfectlyBalanced() {
        AddScore(1000);
        PopManager.Instance.DoScorePop("Plus1000", 0, 0.4f);

        if(SimulatorManager.Instance.currentSim.GetAngle() > 0) {
            PopManager.Instance.DoPopStandSide("PerfectlyBalanced", 2, 0.6f, true);
        } else {
            PopManager.Instance.DoPopStandSide("PerfectlyBalanced", -2, 0.6f, false);
        }
    }

    public void MaxSpeed() {
        AddScore(1000);
        PopManager.Instance.DoScorePop("Plus1000", 0, 2);

        if(SimulatorManager.Instance.currentSim.GetAngle() > 0) {
            PopManager.Instance.DoPopStandSide("MaxSpeed", 2, 0.6f, true);
        } else {
            PopManager.Instance.DoPopStandSide("MaxSpeed", -2, 0.6f, false);
        }
    }

    public void OnTheEdge() {
        AddScore(1000);
        PopManager.Instance.DoScorePop("Plus1000", 0, 2);

        if(SimulatorManager.Instance.currentSim.GetAngle() > 0) {
            PopManager.Instance.DoPopStandSide("OnTheEdge", 2, 0.6f, true);
        } else {
            PopManager.Instance.DoPopStandSide("OnTheEdge", -2, 0.6f, false);
        }
    }

    public void Shaker() {
        AddScore(1000);
        PopManager.Instance.DoScorePop("Plus1000", 0, 2);

        if(SimulatorManager.Instance.currentSim.GetAngle() > 0) {
            PopManager.Instance.DoPopStandSide("Shaker", 2, 0.6f, true);
        } else {
            PopManager.Instance.DoPopStandSide("Shaker", -2, 0.6f, false);
        }
    }

}