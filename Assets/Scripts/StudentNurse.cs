﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StudentNurse : MonoBehaviour
{
    [SerializeField] private Text voiceToText;
    private bool canSpeakAgain = false;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    /// <summary>
    /// Function that determines if the student nurse is done speaking a sentence/question
    /// The next phase would be to parse what was just said.
    /// </summary>
    /// <returns></returns>
    public bool IsDialogueFinalized()
    {
        if (voiceToText.text.Contains("Final"))
        {
            voiceToText.text = "";
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsPatientInterupted()
    {
        if (voiceToText.text != "")
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
