using UnityEngine;

public class PlayerEffectState : MonoBehaviour
{
    [Header("Configuraciones")]
    public float mouthProximityThreshold = 0.15f;
    
    public Transform mouthPosition;
    
    [Header("Depuración")]
    public bool isSmokingCurrently = false;
    public bool isDrinkingCurrently = false;
    public GameObject currentActiveItem = null;
    private void Start()
    {
        if (mouthPosition == null)
        {
            mouthPosition = transform;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (mouthPosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(mouthPosition.position, mouthProximityThreshold);
        }
    }    public bool IsObjectNearMouth(Transform objectTransform)
    {
        if (mouthPosition == null) return false;
        
        float distance = Vector3.Distance(mouthPosition.position, objectTransform.position);
        return distance <= mouthProximityThreshold;
    }
      
    public void SetSmokingState(bool isSmoking, GameObject smokingItem = null)
    {
        isSmokingCurrently = isSmoking;
        if (isSmoking)
        {
            isDrinkingCurrently = false;
            currentActiveItem = smokingItem;
        }
        else if (currentActiveItem == smokingItem)
        {
            currentActiveItem = null;
        }
    }
    
    public void SetDrinkingState(bool isDrinking, GameObject drinkingItem = null)
    {
        isDrinkingCurrently = isDrinking;
        if (isDrinking)
        {
            isSmokingCurrently = false;
            currentActiveItem = drinkingItem;
        }
        else if (currentActiveItem == drinkingItem)
        {
            currentActiveItem = null;
        }
    }
}