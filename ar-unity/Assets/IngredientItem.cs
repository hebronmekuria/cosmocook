using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class IngredientItem : MonoBehaviour
{
    public Ingredient ingredient;
    public TextMeshPro Text;
    public Interactable Checkbox;

    private void Start()
    {
        EventBus.Subscribe<IngredientFound_Event>(OnIngredientFound);
    }

    public void Init(Ingredient ing_f)
    {
        ingredient = ing_f;
        Checkbox.IsToggled = false;
        Text.text = ingredient.quantity + " " + ingredient.name;
    }

    private void OnIngredientFound(IngredientFound_Event e)
    {
        if (e.ingredient == ingredient.name)
        {
            // found
            Checkbox.IsToggled = true;
        }
    }
}
