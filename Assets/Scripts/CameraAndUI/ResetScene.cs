using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetScene : MonoBehaviour
{

    void Update()
    {
        ResetCurrentScenePlease();
    }

    private void ResetCurrentScenePlease()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            SceneManager.LoadScene("Main");
    }
}
