using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{


    void Update()
    {
        if (Input.GetButtonDown("Submit"))
        {
            SceneManager.LoadScene("GameScene");
        }
    }
}