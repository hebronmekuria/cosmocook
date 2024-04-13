using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    public GameObject PopupPrefab;

    private void Start()
    {
        EventBus.Subscribe<CreatePopup_Event>(CreatePopup);
    }

    public void CreatePopup(CreatePopup_Event e)
    {
        Popup p = Instantiate(PopupPrefab, transform).GetComponent<Popup>();
        p.SetPopup(e.popupData);
    }
}
