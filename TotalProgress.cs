using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TotalProgress : MonoBehaviour {
    public GameObject[] Downloads;
    public Slider totalprogressbar;
    public Text percent;
    int index;
    int indexv;
    public GameObject nextscene;
    bool debugged;

    void Update () {
        int count;
        for (int i = 0; i < Downloads.Length; i++) {
            if (Downloads[i] == Downloads[i].gameObject.activeSelf) {
                count = i + 1;
                index = count;
                indexv = count;
            }
        }
        for (int i = 0; i < Downloads.Length; i++) {

            if (Downloads[i].activeSelf == true) {
                Downloads.Last ();
                totalprogressbar.value = 100 / Downloads.Length + (i * 100 / Downloads.Length) + 1;
                percent.text = totalprogressbar.value.ToString ("0") + " %";
            }
        }
        if (percent.text == "100 %") {
            nextscene.SetActive (true);
            if (!debugged) {
                Debug.LogWarning ("File " + Downloads.Last ().GetComponent<DownloadScript> ().newFileName + " complete.");
                debugged = true;
            }
        }
    }
    public void Next () {
        for (int i = 0; i < transform.childCount; i++) {
            transform.GetChild (i).gameObject.SetActive (index == i);
        }
    }
    public void NextScript () {
        for (int i = 0; i < transform.childCount; i++) {
            transform.GetChild (i).gameObject.SetActive (indexv == i);
            Debug.Log ("NextScript call");
            Downloads[indexv + 1].GetComponent<DownloadScript> ().onStart = false;
            Downloads[indexv + 1].GetComponent<DownloadScript> ().onVerify = true;
        }
    }
}
