using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testsc : MonoBehaviour
{
    public DialogSystem dialogSystem;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V)) 
        {
            dialogSystem.StartDialog(0);
        }
    }
}
