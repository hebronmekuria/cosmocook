using System;
using System.Collections.Generic;

public class CookTimeMinutes
{
    public string type { get; set; }
    public int minimum { get; set; }
}

public class Description
{
    public string type { get; set; }
}

public class ImageUrl
{
    public string type { get; set; }
    public string format { get; set; }
}

public class Ingredients
{
    public string type { get; set; }
    public Items items { get; set; }
}

public class Items
{
    public string type { get; set; }
    public Properties properties { get; set; }
    public List<string> required { get; set; }
}

public class Name
{
    public string type { get; set; }
}

public class PrepTimeMinutes
{
    public string type { get; set; }
    public int minimum { get; set; }
}

public class Properties
{
    public RecipeName recipe_name { get; set; }
    public Description description { get; set; }
    public PrepTimeMinutes prep_time_minutes { get; set; }
    public CookTimeMinutes cook_time_minutes { get; set; }
    public TotalTimeMinutes total_time_minutes { get; set; }
    public Ingredients ingredients { get; set; }
    public Steps steps { get; set; }
    public Servings servings { get; set; }
    public Name name { get; set; }
    public Quantity quantity { get; set; }
    public Unit unit { get; set; }
    public StepNumber step_number { get; set; }
    public ImageUrl image_url { get; set; }
}

public class Quantity
{
    public string type { get; set; }
}

public class RecipeName
{
    public string type { get; set; }
}

[Serializable]
public class RecipeData
{
    public string type { get; set; }
    public Properties properties { get; set; }
    public List<string> required { get; set; }
}

public class Servings
{
    public string type { get; set; }
    public int minimum { get; set; }
}

public class StepNumber
{
    public string type { get; set; }
}

public class Steps
{
    public string type { get; set; }
    public Items items { get; set; }
}

public class TotalTimeMinutes
{
    public string type { get; set; }
    public int minimum { get; set; }
}

public class Unit
{
    public string type { get; set; }
}

