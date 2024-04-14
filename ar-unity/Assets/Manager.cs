using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Device;

public class Manager : MonoBehaviour
{
    public static Manager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public static RecipeData Recipe = null;
    public static int CurrentIndex = 0;
    public GameObject HomeScreen;
    public GameObject MainScreen;
    public GameObject IngredientScreen;

    public GameObject IngredientsInstance;
    public GameObject SpeechToText;

    private void Start()
    {
        EventBus.Subscribe<RecipeDataReceived_Event>(RecipeDataReceived);

        IEnumerator _SpawnScreen(float delay, GameObject screen)
        {
            yield return new WaitForSeconds(delay);
            screen.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            screen.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.5f;
        }

        HomeScreen.SetActive(false);

        StartCoroutine(_SpawnScreen(0.2f, HomeScreen));
    }

    private void RecipeDataReceived(RecipeDataReceived_Event e)
    {
        Recipe = e.Recipe;
        CurrentIndex = 0;
        HomeScreen.SetActive(false);

        GoToStep(CurrentIndex);

        IngredientsInstance = Instantiate(IngredientScreen);
        IngredientsInstance.SetActive(false);
        IngredientsInstance.SetActive(true);
        IngredientsInstance.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.4f;
        CurrentIndex = -1;

        IEnumerator _CallEvent()
        {
            yield return new WaitForSeconds(0.1f);
            EventBus.Publish<IngredientsScreen_ShowData_Event>(new(Recipe));
        }
        StartCoroutine(_CallEvent());
    }

    public void GoToStep(int index)
    {
        if (index < Recipe.steps.Count)
        {
            Step s = Recipe.steps[index];
            EventBus.Publish<MainScreen_ShowData_Event>(new(
                "Step " + Recipe.steps[index].step_number.ToString(),
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

    public void GoToNextStep()
    {
        if (CurrentIndex == -1)
        {
            GameObject g = Instantiate(Instance.MainScreen);
            g.transform.position = Camera.main.transform.position + Camera.main.transform.forward*0.8f - Camera.main.transform.up*0.1f;
            Debug.Log("g position: " + g.transform.position.ToString());
            Instance.IngredientsInstance.transform.position = Instance.IngredientsInstance.transform.position  + Camera.main.transform.forward * 0.1f;
        }
        IEnumerator _Next()
        {
            yield return new WaitForSeconds(0.1f);
            GoToStep(CurrentIndex + 1);
        }
        StartCoroutine(_Next());
    }
}
