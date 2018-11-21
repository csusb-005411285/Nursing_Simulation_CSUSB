﻿using System.Threading;
using AWS;
using UnityEngine;
using System.Collections;

public static class CloudManager	//TODO inactivity timeout
{
    //TODO implement AWS comprehend, S3, Transcribe

    private static int maxPollyThreads = 9;
    private static int runningPollyThreads = 0;
    private static Thread pollyThread = new Thread(PollyJob);
    /// holds queue of strings to be run in polly threads
    private static Queue pollyJobQueue = new Queue();

    ///helper class for safely starting polly threads
    private static void PollyJob(object pollyInputText)
    {
        Polly.runPolly((string)pollyInputText);
        runningPollyThreads--;
    }

    /// queues a thread to request text-to-speech audio from AWSPolly.
    /// <param name="textToSpeechInput">text that will be converted into audio</param>
    public static void StartPollyJob(string textToSpeechInput)
    {
        pollyJobQueue.Enqueue(textToSpeechInput);
        if (runningPollyThreads < maxPollyThreads)  //if there are threads avalible
        {
            while (runningPollyThreads < maxPollyThreads && pollyJobQueue.Count != 0) //try to empty queue into threads
            {
                runningPollyThreads++;
                pollyThread = new Thread(() => PollyJob(pollyJobQueue.Dequeue()));
                //TODO test this
            }
        }
    }

    /// runs jobs from pollyqueue and returns true when all jobs have completed
    /// <returns>returns true when all bobs have been completed</returns>
    public static bool JoinPollyJobs()
    {
        if (pollyJobQueue.Count == 0)
        {
            return true;
        }
        if (runningPollyThreads < maxPollyThreads)
        {
            runningPollyThreads++;
            pollyThread = new Thread(() => PollyJob(pollyJobQueue.Dequeue()));
        }
        return false;
    }
}