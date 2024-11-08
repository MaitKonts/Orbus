using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ActivateUI : NetworkBehaviour
{
    public KeyCode keyCode;
    public GameObject uIToToggle;
    private bool waiting;
    public bool startOff;

    // Start is called before the first frame update
    void Start()
    {
        if (startOff)
        {
            uIToToggle.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return; // Ensure only the owner can toggle the UI

        if (!waiting)
        {
            if (Input.GetKeyDown(keyCode))
            {
                ToggleUI();
            }
        }
    }

    private void ToggleUI()
    {
        uIToToggle.SetActive(!uIToToggle.activeSelf);
    }
}
