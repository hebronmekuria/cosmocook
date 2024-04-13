using System;
using System.Collections.Generic;
using UnityEngine;


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

