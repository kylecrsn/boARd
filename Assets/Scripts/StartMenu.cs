using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour {

    public Button play;
    public Button quit;

	void Start ()
    {
        play = play.GetComponent<Button>();
        quit = quit.GetComponent<Button>();
    }
	
    public void PlayPressed()
    {
        SceneManager.LoadScene("lobby");
    }

    public void ExitPressed()
    {
        Application.Quit();
    }
}
