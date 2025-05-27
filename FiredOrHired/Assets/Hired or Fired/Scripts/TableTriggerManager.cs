using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableTriggerManager : MonoBehaviour
{
    public LayerMask tableItemLayer;

    void OnTriggerEnter(Collider other)
    {
        if (IsTableItem(other.gameObject))
        {
            var returner = other.GetComponent<ReturnToTable>();
            if (returner != null)
                returner.SetOnTable(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsTableItem(other.gameObject))
        {
            var returner = other.GetComponent<ReturnToTable>();
            if (returner != null)
                returner.SetOnTable(false);
        }
    }

    bool IsTableItem(GameObject obj)
    {
        return ((1 << obj.layer) & tableItemLayer) != 0;
    }
}
