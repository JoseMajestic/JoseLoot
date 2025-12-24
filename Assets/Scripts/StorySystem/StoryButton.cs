using UnityEngine;
using System;

/// <summary>
/// Helper class para botones dinámicos en el sistema de historia
/// </summary>
[System.Serializable]
public class StoryButton
{
    public string text;
    public Action onClick;
    
    public StoryButton(string text, Action onClick)
    {
        this.text = text;
        this.onClick = onClick;
    }
}
