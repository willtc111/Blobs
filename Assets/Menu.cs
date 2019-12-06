using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Menu : MonoBehaviour {
    
    [SerializeField]
    public GameObject scoreboard;
    
    public static int score;

    public void PlayGame() {
        score = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public static void LoseGame() {
        Debug.Log("Score is " + score);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void RestartGame() {
        score = 0;
        SceneManager.LoadScene(0);
    }

    public void Start() {
        if( score != 0 ) {
            TextMeshProUGUI text = scoreboard.GetComponent<TextMeshProUGUI>();
            text.text = "Your Score: " + score;
        }
    }
}
