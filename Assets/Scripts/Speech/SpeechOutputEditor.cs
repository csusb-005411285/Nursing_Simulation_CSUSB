﻿#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Speech
{
    [CustomEditor(typeof(SpeechOutput), true)]//TODO see if custom editor can be toggled
    public class SpeechOutputEditor : Editor
    {

        private SpeechOutput speechOutput;
        private string thisObjectName;
        /// boolean for forcing generation of all character voices instead of only current character
        public bool generateForAllCharacters = false;
        /// boolean for only generating OutputClips without generating audio from amazon polly
        public bool preventPollyAudioGeneration = false;


        void OnEnable()
        {
            speechOutput = ((SpeechOutput)target);
            SerializedObject serializedObject1 = new SerializedObject(speechOutput);
            thisObjectName = serializedObject1.FindProperty("m_Name").stringValue;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Separator();

            generateForAllCharacters = EditorGUILayout.Toggle("Generate All Characters?", generateForAllCharacters);
            preventPollyAudioGeneration = EditorGUILayout.Toggle("Prevent Polly Generation?", preventPollyAudioGeneration);

            if (GUILayout.Button("Generate Audio Clips"))
            {
                EditorUtility.DisplayProgressBar("Generating Audio Clips...", "starting...", 0f);
                if (generateForAllCharacters == true)   //generate voice clips for all characters, restore currently selected character
                {
                    int originalCharacter = CharacterConfig.currentCharacter.characterNumber;
                    for (int i = 0; i < CharacterConfig.characterList.Length; i++)
                    {
                        EditorUtility.DisplayProgressBar("Generating Audio Clips...", "character "+(i+1)+"/"+ CharacterConfig.characterList.Length, 
                            CharacterConfig.characterList.Length/(i+1));
                        CharacterConfig.currentCharacter = CharacterConfig.characterList[i];
                        generateCharacterVoiceClips();
                    }
                    CharacterConfig.currentCharacter = CharacterConfig.characterList[originalCharacter];
                }
                else
                {   //Generate voice clips only for current character
                    EditorUtility.DisplayProgressBar("Generating Audio Clips...", "character 1/1", .5f);
                    generateCharacterVoiceClips();
                }
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();
            }

        }

        private void generateCharacterVoiceClips()
        {
            speechOutput.outputAudioClipList = new OutputClip[speechOutput.outputPhrases.Length];

            string newDirectory = Tools.Directory.GetHomeDirectory() + @"Assets\SpeechIO\GeneratedOutputClips\" + thisObjectName + @"\";
            if (System.IO.Directory.Exists(newDirectory))
            {
                System.IO.Directory.Delete(newDirectory, true);
            }
            System.IO.Directory.CreateDirectory(newDirectory);
            AssetDatabase.Refresh();

            for (int i = 0; i < speechOutput.outputPhrases.Length; i++) //for each output Phrase, create OutputClip and generate Audio
            {
                if (preventPollyAudioGeneration == false)
                {
                    CloudManager.StartPollyJob(speechOutput.outputPhrases[i]);
                }

                OutputClip myClip = ScriptableObject.CreateInstance<OutputClip>();

                AssetDatabase.CreateAsset(myClip, "Assets/SpeechIO/GeneratedOutputClips/" + thisObjectName + "/" + speechOutput.outputPhrases[i] + ".asset");
                speechOutput.outputAudioClipList[i] = myClip;

                //set OutputClip parameters
                myClip.outputPhrase = speechOutput.outputPhrases[i];
            }

            while (CloudManager.WaitForPollyJobs() != true)    //wait for audio clips to download
            {
                System.Threading.Thread.Sleep(100);
            }

            AssetDatabase.Refresh();

            for (int i = 0; i < speechOutput.outputPhrases.Length; i++) //for each output Phrase, attach audio to OutputClip
            {
                //TODO attach audio files to audio objects
                AudioClip outputAudio = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Audio/GeneratedOutput/" +
                    CharacterConfig.currentCharacter.name + "/" + speechOutput.outputPhrases[i] + ".mp3", typeof(AudioClip));
                if (outputAudio == null)
                {
                    Debug.LogError("Mising Audio Clip: " + CharacterConfig.currentCharacter.name + "/" + speechOutput.outputPhrases[i]);
                }
                else
                {
                    System.Array.Resize<AudioClip>(ref speechOutput.outputAudioClipList[i].outputAudioClips, CharacterConfig.characterList.Length);
                    speechOutput.outputAudioClipList[i].outputAudioClips[CharacterConfig.currentCharacter.characterNumber] = outputAudio;
                }
            }
        }
    }
}
#endif