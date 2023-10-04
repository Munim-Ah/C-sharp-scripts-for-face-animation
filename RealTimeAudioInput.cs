using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System;

[RequireComponent(typeof(AudioSource))]
public class AudioRecorderAndLipSync : MonoBehaviour
{
    private AudioSource audioSource;
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;
    private LipSyncController lipSyncController;

    public string serverIP = "127.0.0.1";
    public int sendPort = 8000;
    public int receivePort = 8001;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        lipSyncController = GetComponent<LipSyncController>();

        udpClient = new UdpClient(receivePort);
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), sendPort);

        StartCoroutine(ReceiveVisemeData());
    }

    public void StartRecording()
    {
        audioSource.clip = Microphone.Start(null, false, 10, 16000);
    }

    public void StopRecordingAndSend()
    {
        Microphone.End(null);
        byte[] audioData = SavWav.GetWavWithoutHeader(audioSource.clip, out var _);

        int maxPacketSize = 65000; // Set to a safe value below the maximum UDP packet size
        int totalPackets = (int)Math.Ceiling((double)audioData.Length / maxPacketSize);

        for (int i = 0; i < totalPackets; i++)
        {
            int offset = i * maxPacketSize;
            int size = Math.Min(maxPacketSize, audioData.Length - offset);

            byte[] chunk = new byte[size];
            Array.Copy(audioData, offset, chunk, 0, size);

            udpClient.Send(chunk, size, remoteEndPoint);
        }

        // Play the audio for synchronization with lip syncing
        audioSource.Play();
    }


    IEnumerator ReceiveVisemeData()
    {
        // Create a nested async function
        async void ReceiveAsync()
        {
            while (true)
            {
                UdpReceiveResult result = await udpClient.ReceiveAsync();
                string visemeData = Encoding.UTF8.GetString(result.Buffer);
                lipSyncController.LipSync(visemeData);
            }
        }

        // Call the nested async function
        ReceiveAsync();

        // Let the coroutine run indefinitely (since the async function is handling the loop)
        while (true)
        {
            yield return null;
        }
    }

}
