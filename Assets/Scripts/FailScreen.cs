using UnityEngine;

public class FailScreen : Singleton<FailScreen> {

    public TMPro.TMP_Text text;

    public void Setup(float secondsLeft) {
        var mins = Mathf.FloorToInt(secondsLeft / 60);
        secondsLeft = secondsLeft % 60;
        var secondsLeftInt = (int)secondsLeft;
        text.text = string.Format("{0}:{1:D2}", mins, secondsLeftInt); 
    }

}