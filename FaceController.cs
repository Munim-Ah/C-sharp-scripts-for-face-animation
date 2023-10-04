using System.Collections;
using UnityEngine;

public class FaceController : MonoBehaviour
{
    public SkinnedMeshRenderer meshRenderer;
    public string leftEyeBlendShapeName = "EyeBlink_L";
    public string rightEyeBlendShapeName = "EyeBlink_R";
    private int leftEyeBlendShapeIndex;
    private int rightEyeBlendShapeIndex;
    public float blinkDuration = 0.2f;
    public float minTimeBetweenBlinks = 2f;
    public float maxTimeBetweenBlinks = 10f;
    public float rotationRange = 2f; // The range of rotation for head movement
    private float currentAmplitude = 0;

    public void OnAudioAmplitude(float amplitude)
    {
        currentAmplitude = amplitude;
    }

    private Quaternion baseRotation;

    void Start()
    {
        // Save the initial rotation
        baseRotation = transform.rotation;

        // Get the index of the blink blend shape.
        leftEyeBlendShapeIndex = meshRenderer.sharedMesh.GetBlendShapeIndex(leftEyeBlendShapeName);
        rightEyeBlendShapeIndex = meshRenderer.sharedMesh.GetBlendShapeIndex(rightEyeBlendShapeName);

        if (leftEyeBlendShapeIndex < 0 || rightEyeBlendShapeIndex < 0)
        {
            Debug.LogError("Blink blend shapes not found!");
            this.enabled = false;
        }

        // Start the blink and head movement coroutines.
        StartCoroutine(BlinkCoroutine());
        StartCoroutine(RotateHeadCoroutine());
    }

    IEnumerator BlinkCoroutine()
    {
        while (true)
        {
            // Wait for a random amount of time.
            float waitTime = Random.Range(minTimeBetweenBlinks, maxTimeBetweenBlinks);
            yield return new WaitForSeconds(waitTime);

            // Start the blink.
            StartCoroutine(BlinkOnce());
        }
    }

    IEnumerator BlinkOnce()
    {
        // Animate the blend shape weight to 100 (fully blink) over half the duration.
        for (float t = 0; t < blinkDuration / 2; t += Time.deltaTime)
        {
            float normalizedTime = t / (blinkDuration / 2);
            meshRenderer.SetBlendShapeWeight(leftEyeBlendShapeIndex, normalizedTime * 100);
            meshRenderer.SetBlendShapeWeight(rightEyeBlendShapeIndex, normalizedTime * 100);
            yield return null;
        }

        // Animate the blend shape weight back to 0 (fully open) over the other half of the duration.
        for (float t = 0; t < blinkDuration / 2; t += Time.deltaTime)
        {
            float normalizedTime = t / (blinkDuration / 2);
            meshRenderer.SetBlendShapeWeight(leftEyeBlendShapeIndex, (1 - normalizedTime) * 100);
            meshRenderer.SetBlendShapeWeight(rightEyeBlendShapeIndex, (1 - normalizedTime) * 100);
            yield return null;
        }
    }

    IEnumerator RotateHeadCoroutine()
    {
        float threshold = 0.1f;  // A threshold to detect if there's significant audio input
        float scalingFactor = 1f; // Scaling factor to amplify the amplitude's effect on rotation
        float minRotationRange = 10f; // Minimum rotation range irrespective of the audio amplitude

        while (true)
        {
            // Calculate the rotation range based on currentAmplitude, scalingFactor, and minRotationRange
            float adjustedRotationRange = minRotationRange + (rotationRange * currentAmplitude * scalingFactor);

            if (currentAmplitude > threshold)  // Only rotate when there's significant audio input
            {
                // Choose a random rotation for the y and z axes, keeping x the same
                Quaternion targetRotation = Quaternion.Euler(
                    baseRotation.eulerAngles.x, // Keep the x rotation the same
                    baseRotation.eulerAngles.y + Random.Range(-adjustedRotationRange, adjustedRotationRange),
                    baseRotation.eulerAngles.z + Random.Range(-adjustedRotationRange, adjustedRotationRange)
                );

                // Gradually rotate to the target rotation over a random duration using Slerp for smoother transitions
                float duration = Random.Range(1f, 3f);
                float elapsed = 0;
                Quaternion startingRotation = transform.rotation;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float percentage = elapsed / duration;
                    transform.rotation = Quaternion.Slerp(startingRotation, targetRotation, percentage);
                    yield return null;
                }
            }
            else
            {
                yield return null;  // If there's no significant audio, just wait for the next frame
            }
        }
    }
}
