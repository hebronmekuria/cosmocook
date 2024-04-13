using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupPickedUp : MonoBehaviour
{
    public Popup parentPopup;

    void Start()
    {
        parentPopup.WasMoved();
    }
}
