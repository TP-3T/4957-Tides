using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class TabButton : MonoBehaviour
{
    public UnityEvent onTabSelected;
    public UnityEvent onTabDeselected;

    [HideInInspector]
    public Image background;

    void Start()
    {
        background = GetComponent<Image>();
    }

    public void Select()
    {
        if (onTabSelected != null)
        {
            onTabSelected.Invoke();
        }
    }

    public void Deselect()
    {
        if (onTabDeselected != null)
        {
            onTabSelected.Invoke();
        }
    }
}
