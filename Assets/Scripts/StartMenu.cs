using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour {

    public Button play;
    public Button quit;

    //Called when the scene first starts
	void Start ()
    {
        //Assign the play and quit buttons
        play = play.GetComponent<Button>();
        quit = quit.GetComponent<Button>();
    }
	
    //If play is pressed, transistion to the lobby scene
    public void PlayPressed()
    {
        SceneManager.LoadScene("lobby");
    }

    //If quit is pressed, quit out of the game
    public void QuitPressed()
    {
        Application.Quit();
    }
}
