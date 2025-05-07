using UnityEngine;

public class InputTest : MonoBehaviour
{
    public GameManager gameManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            gameManager.Hire();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            gameManager.Fire();
        }
    }
}