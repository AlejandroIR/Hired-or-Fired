using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShelfMinigame : MonoBehaviour
{
    // Start is called before the first frame update
    //Array de gameobjects que representan los objetos en la estantería a romper:
    public GameObject[] shelfObjects;
    public GameObject fireWorkPrefab;
    public Transform fireWorkSpawnPoint;
    private bool hasWon = false;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (hasWon) return;
        CheckWinCondition();
        DebugKeysDestroyOneObject();
    }

    private void CheckWinCondition()
    {
        foreach (GameObject obj in shelfObjects)
        {
            if (obj != null && obj.activeInHierarchy)
            {
                return;
            }
        }

        Debug.Log("¡Has ganado el minijuego!");

        // Use the rotation of the spawn point when instantiating
        Instantiate(fireWorkPrefab, fireWorkSpawnPoint.position, fireWorkSpawnPoint.rotation);
        hasWon = true;
    }
    
    private void DebugKeysDestroyOneObject()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            foreach (GameObject obj in shelfObjects)
            {
                if (obj != null && obj.activeInHierarchy)
                {
                    obj.SetActive(false);
                    Debug.Log("Objeto destruido: " + obj.name);
                    break;
                }
            }
        }
    }
}
