using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

    public GameObject helpPanel;
    public GameObject menuPanel;

    public void SwapScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }

    public void ExitToDesktop() {
        Application.Quit();
    }

    public void OpenHelpPanel() {
        helpPanel.SetActive(true);
        menuPanel.SetActive(false);
    }


}
