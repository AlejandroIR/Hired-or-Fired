using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableTriggerManager : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        var returner = other.GetComponent<ReturnToTable>();
        if (returner != null)
        {
            returner.SetOnTable(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var returner = other.GetComponent<ReturnToTable>();
        if (returner != null)
        {
            returner.SetOnTable(false);
        }
    }
}
