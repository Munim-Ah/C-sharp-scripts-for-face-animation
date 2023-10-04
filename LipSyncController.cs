using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LipSyncController : MonoBehaviour
{
    Dictionary<string, string> visemeToBlendShape = new Dictionary<string, string>()
{
    { "h#", "sil" },
    { "d", "JawOpen" },
    { "ih", "IH" },
    { "dcl", "smile" },
    { "jh", "LipsPucker" },
    { "ux", "LipsFunnel" },
    { "n", "nn" },
    { "ow", "OH" },
    { "hv", "JawFwd" },
    { "iy", "smile" },
    { "z", "ss" },
    { "ae", "OU" },
    { "v", "pp" },
    { "axr", "LipsFunnel" },
    { "tcl", "LipsLowerClose" },
    { "t", "LipsLowerDown" },
    { "ay", "smile" },
    { "ng", "JawChew" },
    { "hh", "JawRight" },
    { "m", "LipsPucker" },
    { "r", "rr" },
    { "ey", "LipsOpen" },
    { "ax", "base_head" },
    { "kcl", "kk" },
    { "k", "JawOpen" },
    { "w", "smile" },
    { "q", "OU" },
    { "ix", "smile" },
    { "f", "ff" },
    { "s", "ss" },
    { "l", "LipsLowerOpen" },
    { "dh", "OU" },
    { "epi", "base_head" },
    { "eh", "OU" },
    { "b", "LipsPucker" },
    { "ao", "OU" },
    { "th", "TH" },
    { "er", "LipsFunnel" },
    { "bcl", "smile" },
    { "aw", "Puff" },
    { "y", "LipsFunnel" },
    { "aa", "JawOpen" },
    { "ax-h", "OU" },
    { "ah", "LipsLowerDown" },
    { "nx", "OU" },
    { "en", "LipsLowerOpen" },
    { "ch", "OU" },
    { "sh", "LipsLowerDown" },
    { "pcl", "pp" },
    { "p", "smile" },
    { "dx", "LipsPucker" },
    { "el", "LipsFunnel" },
    { "oy", "LipsRound" },
    { "gcl", "JawChew" },
    { "g", "JawFwd" },
    { "pau", "OU" },
    { "uw", "LipsRound" },
    { "zh", "smile" },
    { "uh", "LipsOpen" },
    { "em", "smile" },
    { "eng", "JawFwd" }
};


    public string serverIP = "127.0.0.1";
    public SkinnedMeshRenderer faceModel;
    public string[] blendShapeNames;
    public float blendShapeSpeed = 100f;

    // Declare an array of viseme names in the order they are in your Python script
    string[] visemeNames = { "h#", "d", "ih", "dcl", "jh", "ux", "n", "ow", "hv", "iy", "z", "ae", "v", "axr",
                             "tcl", "t", "ay", "ng", "hh", "m", "r", "ey", "ax", "kcl", "k", "w", "q", "ix", "f",
                             "s", "l", "dh", "epi", "eh", "b", "ao", "th", "er", "bcl", "aw", "y", "aa", "ax-h",
                             "ah", "nx", "en", "ch", "sh", "pcl", "p", "dx", "el", "oy", "gcl", "g", "pau", "uw",
                             "zh", "uh", "em", "eng"};

    UdpSocket udpSocket;
    Queue<string> visimeQueue;

    void Start()
    {
        udpSocket = GetComponent<UdpSocket>();
        visimeQueue = new Queue<string>();
    }

    public void LipSync(string viseme)
    {
        if (visemeToBlendShape.ContainsKey(viseme))
        {
            string blendShapeName = visemeToBlendShape[viseme];
            int blendShapeIndex = faceModel.sharedMesh.GetBlendShapeIndex(blendShapeName);
            if (blendShapeIndex >= 0)
            {
                Debug.Log("Animating blend shape: " + blendShapeName);
                float targetValue = 50.0f; // set the value according to the needs
                float currentValue = faceModel.GetBlendShapeWeight(blendShapeIndex);
                // Smoothly interpolate to the target blend shape value
                float newValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * blendShapeSpeed);
                faceModel.SetBlendShapeWeight(blendShapeIndex, newValue);
            }
            else
            {
                Debug.Log("Blend shape not found: " + blendShapeName);
            }
        }
    }

   

    void Update()
    {
        if (udpSocket.isTxStarted)
        {
            string visimeData = udpSocket.GetLastReceivedText();
            //Debug.Log("Received viseme data: " + visimeData);

            // Split the received data into individual viseme indices
            string[] visemeIndices = visimeData.Split(',');
            foreach (string visemeIndexStr in visemeIndices)
            {
                // Convert the viseme index string to an integer
                int visemeIndex;
                if (int.TryParse(visemeIndexStr, out visemeIndex))
                {
                    // Check the viseme index is valid
                    if (visemeIndex >= 0 && visemeIndex < visemeNames.Length)
                    {
                        string visemeName = visemeNames[visemeIndex];
                        LipSync(visemeName);
                    }
                    else
                    {
                        Debug.Log("Received invalid viseme index: " + visemeIndex);
                    }
                }
                else
                {
                    Debug.Log("Failed to parse viseme index: " + visemeIndexStr);
                }
            }
        }
        // Reset all blend shapes at the end of every frame
        for (int i = 0; i < faceModel.sharedMesh.blendShapeCount; i++)
        {
            float currentValue = faceModel.GetBlendShapeWeight(i);
            float newValue = Mathf.Lerp(currentValue, 0, Time.deltaTime * blendShapeSpeed);
            faceModel.SetBlendShapeWeight(i, newValue);
        }
    }
}
