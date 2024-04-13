using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Device;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Instance = this;
        }
    }

    public static RecipeData Recipe = null;
    public static int CurrentIndex = 0;
    public GameObject MainScreen;
    public GameObject IngredientScreen;

    private void Start()
    {
        SpawnScreen(0.2f, MainScreen);
        EventBus.Subscribe<RecipeDataReceived_Event>(RecipeDataReceived);
    }

    public GameObject SpawnScreen(float delay, GameObject screenPrefab)
    {
        IEnumerator _SpawnScreen(float delay, GameObject screen)
        {
            yield return new WaitForSeconds(delay);
            screen.SetActive(true);
            screen.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.5f;
        }

        GameObject g = Instantiate(screenPrefab);
        g.SetActive(false);

        StartCoroutine(_SpawnScreen(delay, g));

        return g;
    }

    private void RecipeDataReceived(RecipeDataReceived_Event e)
    {
        Recipe = e.Recipe;
        CurrentIndex = 0;

        GoToStep(CurrentIndex);

        GameObject g = Instantiate(IngredientScreen);
        g.SetActive(false);
        g.SetActive(true);
        g.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.4f;

        IEnumerator _CallEvent()
        {
            yield return new WaitForSeconds(0.1f);
            EventBus.Publish<IngredientsScreen_ShowData_Event>(new(Recipe));
        }
        StartCoroutine(_CallEvent());
    }

    public static void GoToStep(int index)
    {
        if (index < Recipe.steps.Count)
        {
            Step s = Recipe.steps[index];
            EventBus.Publish<MainScreen_ShowData_Event>(new(
                "Step " + index,
                s.description,
                s.image_url));
            CurrentIndex = index;
        }
        else if (index == Recipe.steps.Count)
        {
            EventBus.Publish<MainScreen_ShowData_Event>(new(
                "Finished",
                "Enjoy your meal!",
                ""));
            CurrentIndex = index;
        }
        else
        {
            EventBus.Publish<MainScreen_ShowData_Event>(new("CosmoCook", "What would you like to cook?", null));
            CurrentIndex = 0;
        }
    }

    public static void GoToNextStep()
    {
        GoToStep(CurrentIndex + 1);
    }
}
