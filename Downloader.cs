using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class DownloadScript : MonoBehaviour {
    public string savePath = "C:/Example";
    public string downloadUrl = "";
    public string pathfile = "";
    public bool originalName = true;
    public string newFileName = "";
    public bool persistentDataPath = false;
    public bool bytesPerSecond = true;
    public bool onStart = false;
    public bool onVerify = false;
    public GameObject progressBar;
    private GameObject go;
    Text progressText;
    Text bytesText;
    public Text actualFile;
    public GameObject finishedButton;
    public GameObject downloadButton;
    WebClient client;
    bool cancelled;
    string uri;
    float progress;
    string bytes;
    bool downloading;
    bool finished;

    public static class Algorithms {
        public static readonly HashAlgorithm MD5 = new MD5CryptoServiceProvider ();
    }
    public static string GetHashFromFile (string fileName, HashAlgorithm algorithm) {
        using (var stream = new BufferedStream (File.OpenRead (fileName), 100000)) {
            return BitConverter.ToString (algorithm.ComputeHash (stream)).Replace ("-", string.Empty);
        }
    }
    public void CheckMD5FileChanges () {
        if (File.ReadAllText (pathfile).Contains (GetHashFromFile (savePath + "/" + newFileName, Algorithms.MD5) + " " + newFileName)) {
            try {
                GameObject.Find ("DownloaderGestor").GetComponent<TotalProgress> ().NextScript ();
                finished = true;
                actualFile.text = newFileName;
                bytesText.text = "File : " + " in last version.";
                progressText.text = "100 %";
                go.GetComponent<Slider> ().value = 100f;
                go.SetActive (false);
                Verification ();
            } catch (Exception ex) {
                Debug.Log ("Finish " + ex);
            }
        } else {
            DownloadFile ();
        }
    }
    public void Verification () {
        try {
            if (File.ReadAllText (pathfile).Contains (GetHashFromFile (savePath + "/" + newFileName, Algorithms.MD5) + " " + newFileName)) {
                try {
                    Debug.Log ("Coincide, no debes hacer nada.");
                    GameObject.Find ("DownloaderGestor").GetComponent<TotalProgress> ().Next ();
                    go.SetActive (false);
                    actualFile.text = newFileName;
                    bytesText.text = "File : " + " Complete";
                    progressText.text = "100 %";
                    go.GetComponent<Slider> ().value = 100f;
                    finished = true;
                } catch (Exception ex) {
                    Debug.Log ("File exists?" + ex);
                }
            } else {
                DownloadFile ();
            }
        } catch (Exception ex) {
            DownloadFile ();
        }

    }

    void Start () {
        go = Instantiate (progressBar, new Vector3 (0, 0, 0), Quaternion.identity) as GameObject;
        go.transform.SetParent (GameObject.Find ("Content").transform, false);
        progressText = go.transform.GetChild (2).GetComponent<Text> ();
        bytesText = go.transform.GetChild (3).GetComponent<Text> ();
        try {
            if (go.GetComponent<Slider> ().maxValue != 100f)
                go.GetComponent<Slider> ().maxValue = 100f;

            go.GetComponent<Slider> ().value = 0;

            uri = downloadUrl;
            if (originalName)
                newFileName = Path.GetFileName (uri);

            DirectoryInfo df = new DirectoryInfo (savePath);
            if (!df.Exists)
                Directory.CreateDirectory (savePath);

            if (onStart)
                Verification ();

            if (onVerify)
                CheckMD5FileChanges ();
        } catch (Exception ex) {
            Debug.Log ("Start realizado." + ex);
        }

    }

    void Update () {
        if (downloading) {
            go.GetComponent<Slider> ().value = progress;
            progressText.text = progress.ToString () + "% ";

            if (bytesporsegundo)
                bytesText.text = "Recibido : " + bytes + " kb";
        }

        if (finished) {
            if (cancelled) {
                bytesText.text = "Cancel";
                progressText.text = "0 %";
                finished = false;
            } else {
                bytesText.text = "Bytes : " + " Complete";
                progressText.text = "100 %";
                go.GetComponent<Slider> ().value = 100f;
                go.SetActive (false);
            }
        }
        if (finished == true) {
            GameObject.Find ("DownloaderGestor").GetComponent<TotalProgress> ().Next ();
            go.SetActive (false);
        }
    }

    public void DownloadFile () {
        actualFile.text = newFileName;
        downloadButton.SetActive (false);
        cancelled = false;
        client = new WebClient ();
        if (!persistentDataPath)
            client.DownloadFileAsync (new System.Uri (downloadUrl), savePath + "/" + newFileName);
        else
            client.DownloadFileAsync (new System.Uri (uri), Application.persistentDataPath + "/" + newFileName);

        downloading = true;
        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler (client_DownloadProgressChanged);
        client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler (DownloadFileCompleted);
    }

    void DownloadFileCompleted (object sender, System.ComponentModel.AsyncCompletedEventArgs e) {
        if (cancelled) {
            Debug.Log ("Cancelado");
            downloading = false;
            finished = true;
        } else {
            if (e.Error == null) {
                downloading = false;
                if (onVerify == true) {
                    GameObject.Find ("DownloaderGestor").GetComponent<TotalProgress> ().NextScript ();
                    finished = true;
                } else {
                    finished = true;
                }
            } else
                Debug.Log (e.Error.ToString ());
        }
    }
    void client_DownloadProgressChanged (object sender, DownloadProgressChangedEventArgs e) {
        progress = (e.BytesReceived * 100 / e.TotalBytesToReceive);
        bytes = e.BytesReceived / 1000 + " / " + e.TotalBytesToReceive / 1000;
    }

    public void CancelDownload () {
        cancelled = true;
        go.GetComponent<Slider> ().value = 100f;
        Debug.Log ("Archivo por cancelar");
        if (client != null) {
            client.CancelAsync ();
            Debug.Log ("Archivo cancelado");
        }
        downloadButton.SetActive (true);
    }

    void OnDisable () {
        cancelled = true;
        if (client != null)
            client.CancelAsync ();
    }

}
