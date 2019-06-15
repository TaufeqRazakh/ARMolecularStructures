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

public class VisionManager : MonoBehaviour
{

    public static VisionManager instance;

    // Replace <Subscription Key> with your valid subscription key.
    private string subscriptionKey = "4744d22311cc47008b360329706d63bd";

    // You must use the same Azure region in your REST API method as you used to
    // get your subscription keys. For example, if you got your subscription keys
    // from the West US region, replace "westcentralus" in the URL
    // below with "westus".
    //
    // Free trial subscription keys are generated in the "westus" region.
    // If you use a free trial subscription key, you shouldn't need to change
    // this region.
    const string uriBase =
        "https://westus2.api.cognitive.microsoft.com/vision/v2.0/read/core/asyncBatchAnalyze";

    internal byte[] imageBytes;

    internal string imagePath;

    private void Awake()
    {
        instance = this;
    }

    private async Task ReadHandwrittenText(string imageFilePath)
    {
        try
        {
            Debug.Log("Preparing Http request");
            HttpClient client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add(
                "Ocp-Apim-Subscription-Key", subscriptionKey);

            // Assemble the URI for the REST API method.
            string uri = uriBase;

            HttpResponseMessage response;

            // Two REST API methods are required to extract handwritten text.
            // One method to submit the image for processing, the other method
            // to retrieve the text found in the image.

            // operationLocation stores the URI of the second REST API method,
            // returned by the first REST API method.
            string operationLocation;

            // Reads the contents of the specified local image
            // into a byte array.
            byte[] byteData = GetImageAsByteArray(imageFilePath);

            // Adds the byte array as an octet stream to the request body.
            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses the "application/octet-stream" content type.
                // The other content types you can use are "application/json"
                // and "multipart/form-data".
                content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/octet-stream");

                // The first REST API method, Batch Read, starts
                // the async process to analyze the written text in the image.
                response = await client.PostAsync(uri, content);
            }

            // The response header for the Batch Read method contains the URI
            // of the second method, Read Operation Result, which
            // returns the results of the process in the response body.
            // The Batch Read operation does not return anything in the response body.
            if (response.IsSuccessStatusCode)
                operationLocation =
                    response.Headers.GetValues("Operation-Location").FirstOrDefault();
            else
            {
                // Display the JSON error data.
                string errorString = await response.Content.ReadAsStringAsync();
                Debug.Log("\n\nResponse:\n{0}\n" +
                    JToken.Parse(errorString).ToString());
                return;
            }

            // If the first REST API method completes successfully, the second 
            // REST API method retrieves the text written in the image.
            //
            // Note: The response may not be immediately available. Handwriting
            // recognition is an asynchronous operation that can take a variable
            // amount of time depending on the length of the handwritten text.
            // You may need to wait or retry this operation.
            //
            // This example checks once per second for ten seconds.
            string contentString;
            int i = 0;
            do
            {
                System.Threading.Thread.Sleep(1000);
                response = await client.GetAsync(operationLocation);
                contentString = await response.Content.ReadAsStringAsync();
                ++i;
            }
            while (i < 10 && contentString.IndexOf("\"status\":\"Succeeded\"") == -1);

            if (i == 10 && contentString.IndexOf("\"status\":\"Succeeded\"") == -1)
            {
                Debug.Log("\nTimeout error.\n");
                return;
            }

            // Display the JSON response.
            Debug.Log("succeeded\nResponse:\n\n{0}\n" +
                JToken.Parse(contentString).ToString());

            Debug.Log("List initialised");
            // Append identefied text response to a List.
            List<String> responseTextList = new List<string>();

            // Make the response a JObject to query with LINQ utilities 
            JObject responseObject = JObject.Parse(contentString);
            JArray recognizedLines = (JArray)responseObject["recognitionResults"][0]["lines"];
            foreach (var li in recognizedLines)
            {
                responseTextList.Add((string)li["text"]);
                Debug.Log("Adding the detected word " + li["text"] + " to the list");
            }
            //ResultsLabel.instance.sayHeyDumDum();
            // Send the list to result label
            ResultsLabel.instance.SetTextToLastLabel(responseTextList);
        }
        catch (Exception e)
        {
            Debug.Log("\n" + e.Message);
        }
    }

    public IEnumerator AnalyseLastImageCaptured()
    {
        Debug.Log("Arrived in Analyse Captured Image function");
        if (File.Exists(imagePath))
        {
            // Call the REST API method.
            Debug.Log("Wait a moment for the results to appear for " + imagePath);
            yield return ReadHandwrittenText(imagePath);
        }
        else
        {
            Debug.Log("\nInvalid file path");
        }
        Debug.Log("Ready to exit from image analyser");

        yield return null;

        /// <summary>
        /// Returns the contents of the specified file as a byte array.
        /// </summary>
        /// <param name="imageFilePath">The image file to read.</param>
        /// <returns>The byte array of the image data.</returns>
    }
    private static byte[] GetImageAsByteArray(string imageFilePath)
    {
        // Open a read-only file stream for the specified file.
        using (FileStream fileStream =
            new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
        {
            // Read the file's contents into a byte array.
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }
    }
}