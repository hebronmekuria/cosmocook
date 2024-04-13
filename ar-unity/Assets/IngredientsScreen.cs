using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using WebSocketSharp;

public class IngredientsScreen : MonoBehaviour
{
    public float titleHeight;
    public float lineHeight;
    public float imageHeight;
    public float startIngredients;
    public string debugText;

    public TextMeshPro Title;
    public TextMeshPro Text;
    public Transform Backplate;

    public AudioClip Clip;

    public List<IngredientItem> Ingredients;
    public GameObject IngredientPrefab;

    void Start()
    {
        EventBus.Subscribe<IngredientsScreen_ShowData_Event>(ShowData);
    }

    [ContextMenu("func SetBackplateSize")]
    public void SetBackplateSize()
    {
        float init_y = Backplate.localPosition.y;
        float init_height = Backplate.localScale.y;

        float height = titleHeight + (lineHeight * Text.textInfo.lineCount) + (lineHeight * Ingredients.Count);

        height += 0.06f;

        float y = init_y - (height - init_height) / 2f;

        Backplate.localScale = new(Backplate.localScale.x, height, Backplate.localScale.z);
        Backplate.localPosition = new(Backplate.localPosition.x, y, Backplate.localPosition.z);


        // set box collider
        Vector3 quadSize = Backplate.GetComponent<MeshRenderer>().bounds.size;

        // Get the box collider
        BoxCollider collider = GetComponent<BoxCollider>();
        if (collider != null)
        {
            collider.size = quadSize;
        }
    }

    private void ShowData(IngredientsScreen_ShowData_Event e)
    {
        Debug.Log("Showing data");

        Title.text = e.recipeData.recipe_name;
        Text.text = "Estimated " + e.recipeData.cook_time_minutes.ToString() + " minutes";

        // create all ingredients list
        int j = 0;
        foreach (Ingredient i in e.recipeData.ingredients)
        {
            IngredientItem item = Instantiate(IngredientPrefab, transform).GetComponent<IngredientItem>();
            item.Init(i);
            Ingredients.Add(item);
            item.gameObject.transform.localPosition = new Vector3(-0.304f, -j*lineHeight, 0.304f);
            j++;
        }

        IEnumerator _SetBackplate()
        {
            yield return new WaitForSeconds(0.07f);
            SetBackplateSize();
        }

        StartCoroutine(_SetBackplate());

        GetComponent<AudioSource>().PlayOneShot(Clip);
    }
}
