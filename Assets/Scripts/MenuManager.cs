using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject menu;
    private bool on;

    void Start(){
        on = menu.activeSelf;
    }

    public void MenuSwitch(){
        on = !on;
        menu.SetActive(on);
    }
}
