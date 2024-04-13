using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public GameObject Tonko;
    public GameObject Avocado;

    public bool isLoading;

    private void Start()
    {
        Tonko.SetActive(true);
        Avocado.SetActive(false);
        isLoading = false;

        EventBus.Subscribe<LoadingStarted_Event>(e =>
        {
            SetActiveStates();
            isLoading = true;
        });
        EventBus.Subscribe<LoadingFinished_Event>(e =>
        {
            SetActiveStates();
            isLoading = false;
        });
    }

    private void SetActiveStates()
    {
        if (isLoading)
        {
            Tonko.SetActive(false);
            Avocado.SetActive(true);
        }
        else
        {
            Tonko.SetActive(true);
            Avocado.SetActive(false);
        }
    }

}
