using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public GameObject Tonko;
    public GameObject Avocado;

    public bool isLoading;
    Queue<string> commands; // set active states

    private void Start()
    {
        Tonko.SetActive(true);
        Avocado.SetActive(false);
        isLoading = false;
        commands = new();

        EventBus.Subscribe<LoadingStarted_Event>(e =>
        {
            isLoading = true;
            commands.Enqueue("SetActiveStates");
        });
        EventBus.Subscribe<LoadingFinished_Event>(e =>
        {
            isLoading = false;
            commands.Enqueue("SetActiveStates");
        });
    }

    private void Update()
    {
        if (commands.Count != 0)
        {
            commands.Dequeue();
            SetActiveStates();
        }
    }

    private void SetActiveStates()
    {
        if (isLoading)
        {
            Tonko.SetActive(false);
            Avocado.SetActive(true);
            Debug.Log("Loading true");
        }
        else
        {
            Tonko.SetActive(true);
            Avocado.SetActive(false);
            Debug.Log("Loading false");
        }
    }

}
