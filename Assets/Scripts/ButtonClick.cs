using UnityEngine;
using System.Collections;

public class ButtonClick : MonoBehaviour {

    [SerializeField]
    [FMODUnity.EventRef]
    public string buttonClick;

    public void OnButtonClick() {
        FMODUnity.RuntimeManager.PlayOneShot(buttonClick);
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
