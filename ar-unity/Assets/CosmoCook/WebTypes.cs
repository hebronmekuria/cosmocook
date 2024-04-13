using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Ingredient
{
    public string name;
    public string quantity;
    public string unit;
}

[Serializable]
public class Step
{
    public int step_number;
    public string description;
    public string image_url;
}

[Serializable]
public class RecipeData
{
    public string recipe_name;
    public string description;
    public int prep_time_minutes;
    public int cook_time_minutes;
    public int total_time_minutes;
    public List<Ingredient> ingredients;
    public List<Step> steps;
    public int servings;

    public RecipeData()
    {
        Debug.Log("Creating recipe data");
    }

    public override string ToString()
    {
        return recipe_name;
    }
}

[Serializable]
public class PopupData
{
    public string text;
    public string image;
    public string video_url;

    public PopupData()
    {
        Debug.Log("new popup");
    }

    public override string ToString()
    {
        return text + " : " + image + video_url;
    }
}



[Serializable]
public class Request_GetRecipe
{
    public string type;
    public Query data;
}

[Serializable]
public class Query
{
    public string query;

    public Query(string q_f)
    {
        query = q_f;
    }
}