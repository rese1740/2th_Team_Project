using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDestroy : MonoBehaviour
{
    public void DestroyUI()
    {
        Destroy(gameObject);
        ButtonGroupManager.Instance.isChangedElement = false;
    }
}
