﻿// TODO: I would like to implement a function which would for failed image saearches create a label with a lifetime for 10 senconds or so 
// with the appropriate error message 

using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

public class ResultsLabel : MonoBehaviour
{
    public static ResultsLabel instance;

    public GameObject cursor;

    public Transform labelPrefab;

    [HideInInspector]
    public Transform lastLabelPlaced;

    [HideInInspector]
    public TextMesh lastLabelPlacedText;

    private void Awake()
    {
        // allows this instance to behave like a singleton
        instance = this;
    }

    /// <summary>
    /// Instantiate a Label in the appropriate location relative to the Main Camera.
    /// </summary>
    public void CreateLabel()
    {
        lastLabelPlaced = Instantiate(labelPrefab, cursor.transform.position, transform.rotation);

        lastLabelPlacedText = lastLabelPlaced.GetComponent<TextMesh>();

        // Change the text of the label to show that has been placed
        // The final text will be set at a later stage
        lastLabelPlacedText.text = "Analysing...";
    }

    /// <summary>
    /// Set the Tags as Text of the last Label created. 
    /// </summary>
    public void SetTextToLastLabel(List<string> textList)
    {
        lastLabelPlacedText = lastLabelPlaced.GetComponent<TextMesh>();

        // At this point we go through all the tags received and set them as text of the label
        lastLabelPlacedText.text = "I see: \n";

        foreach (var text in textList)
        {
            Debug.Log(text + " will be put in the result box");
            lastLabelPlacedText.text += text + " ";
            Debug.Log("successfully altered text in the label");
        }
    }

    public void sayHeyDumDum()
    {
        Debug.Log("Saying Hey Dum Dum");
    }
}
