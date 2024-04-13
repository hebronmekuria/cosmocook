using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static RecipeData Recipe = null;
    public GameObject MainScreen;

    private void Start()
    {
        SpawnScreen(1, MainScreen);
        EventBus.Subscribe<RecipeDataReceived_Event>(RecipeDataReceived);
    }

    public GameObject SpawnScreen(float delay, GameObject screenPrefab)
    {
        IEnumerator _SpawnScreen(float delay, GameObject screen)
        {
            yield return new WaitForSeconds(delay);
            screen.SetActive(true);
            screen.transform.position = Camera.main.transform.position + Camera.main.transform.forward;
        }

        GameObject g = Instantiate(screenPrefab);
        g.SetActive(false);

        StartCoroutine(_SpawnScreen(delay, g));

        return g;
    }

    private void RecipeDataReceived(RecipeDataReceived_Event e)
    {
        Recipe = e.Recipe;
    }
}
