using UnityEngine;
using System.Collections;

public class StartCsreen : MonoBehaviour {

    [SerializeField]
    AudioSource BGM;
    [SerializeField]
    AudioSource SE;

    private void Awake()
    {
        Screen.SetResolution(720, 576, false);
    }

    public void StartGame()
    {
        PlaySe();
        DontDestroyOnLoad(SE);
        DontDestroyOnLoad(BGM);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    void PlaySe()
    {
        SE.clip = Resources.Load<AudioClip>("sounds/mouse-click");
        SE.Play();
    }

}
