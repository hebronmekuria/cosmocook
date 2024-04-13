using System;
using System.Collections.Generic;
using UnityEngine;
using static Popup;


public class MainScreen_ShowData_Event
{
    public string title;
    public string text;
    public string image;

    public MainScreen_ShowData_Event(string title_f, string text_f, string image_f)
    {
        Debug.Log("MainScreen_ShowData_Event");
        title = title_f;
        text = text_f;
        image = image_f;
    }
}
public class IngredientFound_Event
{
    public string ingredient;

    public IngredientFound_Event(string ingredient_f)
    {
        Debug.Log("IngredientFound_Event");
        ingredient = ingredient_f;
    }
}

public class IngredientsScreen_ShowData_Event
{
    public RecipeData recipeData;

    public IngredientsScreen_ShowData_Event(RecipeData r_f)
    {
        Debug.Log("IngredientsScreen_ShowData_Event");
        recipeData = r_f;
    }
}


public class LoadingStarted_Event
{
    public LoadingStarted_Event()
    {
        Debug.Log("LoadingStarted_Event");
    }
}
public class LoadingFinished_Event
{
    public LoadingFinished_Event()
    {
        Debug.Log("LoadingFinished_Event");
    }
}


public class CreatePopup_Event
{
    public PopupData popupData;

    public CreatePopup_Event(PopupData popupData_f)
    {
        popupData = popupData_f;
        Debug.Log("CreatePopup_Event");
    }
}


public class RecipeDataReceived_Event
{
    public RecipeData Recipe;

    public RecipeDataReceived_Event(RecipeData recipe_f)
    {
        Recipe = recipe_f;
        Debug.Log("RecipeDataReceived_Event");
    }
}