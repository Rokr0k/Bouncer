using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using SFB;

namespace Bouncer
{
    namespace Play
    {
        public class Loader : MonoBehaviour
        {
            public MusicManager musicManager;
            public Button load, play;

            public void Load()
            {
                load.interactable = false;
                play.interactable = false;
                ExtensionFilter[] extensions = new ExtensionFilter[] { new ExtensionFilter("Bouncer File", "bnc") };
                string[] files = StandaloneFileBrowser.OpenFilePanel("Load Bouncer", "", extensions, false);
                if (files.Length > 0)
                {
                    Parse(files[0]);
                }
                else
                {
                    load.interactable = true;
                    play.interactable = success;
                }
            }

            private Regex songPath = new Regex(@"^\s*#AUDIO\s+(.*)\s*$");
            private Regex bpmPath = new Regex(@"^\s*#BPM\s+(\d+(\.\d+)?)\s*$");
            private Regex offsetPath = new Regex(@"^\s*#OFFSET\s+(\d+(\.\d+)?)\s*$");
            private Regex bpmTransPath = new Regex(@"^\s*#BPMTRANS\s+(\d+(\.\d+)?)\s+(\d+(\.\d+)?)\s*$");
            private Regex signaturePath = new Regex(@"^\s*#SIGNATURE\s+(\d+)\s+(\d+(\.\d+)?)\s*$");
            private Regex notesPath = new Regex(@"^\s*#(\d+):([\.\*#]+)\s*$");

            private bool done;
            private bool success;

            private void Parse(string file)
            {
                done = false;
                success = false;
                musicManager.measures = new List<MusicManager.Measure>();
                musicManager.speedcore = new List<MusicManager.Speedcore>();
                musicManager.speedcore.Add(new MusicManager.Speedcore());
                musicManager.speedcore[0] = new MusicManager.Speedcore(0, 0, 130);
                string[] lines = File.ReadAllLines(file);
                float[] signatures = new float[1000];
                for (int i = 0; i < 1000; i++)
                {
                    signatures[i] = 1;
                }
                foreach (string line in lines)
                {
                    Match match;
                    if ((match = songPath.Match(line)).Success)
                    {
                        StartCoroutine(LoadAudio(Path.Combine(Directory.GetParent(file).FullName, match.Groups[1].Value)));
                    }
                    else if ((match = bpmPath.Match(line)).Success)
                    {
                        musicManager.speedcore[0] = new MusicManager.Speedcore(musicManager.speedcore[0].time, musicManager.speedcore[0].fraction, float.Parse(match.Groups[1].Value));
                    }
                    else if ((match = offsetPath.Match(line)).Success)
                    {
                        musicManager.speedcore[0] = new MusicManager.Speedcore(float.Parse(match.Groups[1].Value), musicManager.speedcore[0].fraction, musicManager.speedcore[0].bpm);
                    }
                    else if ((match = bpmTransPath.Match(line)).Success)
                    {
                        musicManager.speedcore.Add(new MusicManager.Speedcore(0, float.Parse(match.Groups[1].Value), float.Parse(match.Groups[3].Value)));
                    }
                    else if ((match = signaturePath.Match(line)).Success)
                    {
                        signatures[int.Parse(match.Groups[1].Value)] = float.Parse(match.Groups[2].Value);
                    }
                    else if ((match = notesPath.Match(line)).Success)
                    {
                        MusicManager.Measure measure = new MusicManager.Measure(new List<MusicManager.Note>(), float.Parse(match.Groups[1].Value), 0, 0);
                        musicManager.measures.Add(measure);
                        for (int i = 0; i < match.Groups[2].Value.Length; i++)
                        {
                            switch (match.Groups[2].Value[i])
                            {
                                case '*':
                                    measure.notes.Add(new MusicManager.Note(0, i * signatures[Mathf.FloorToInt(measure.signature)] / match.Groups[2].Value.Length, false));
                                    break;
                                case '#':
                                    measure.notes.Add(new MusicManager.Note(0, i * signatures[Mathf.FloorToInt(measure.signature)] / match.Groups[2].Value.Length, true));
                                    break;
                            }
                        }
                    }
                }
                musicManager.speedcore.Sort((MusicManager.Speedcore a, MusicManager.Speedcore b) => Mathf.CeilToInt(a.fraction - b.fraction));
                musicManager.measures.Sort((MusicManager.Measure a, MusicManager.Measure b) => (int)a.signature - (int)b.signature);
                foreach (MusicManager.Measure measure in musicManager.measures)
                {
                    measure.notes.Sort((MusicManager.Note a, MusicManager.Note b) => Mathf.CeilToInt(a.fraction - b.fraction));
                }
                for (int i = 1; i < musicManager.speedcore.Count; i++)
                {
                    int sig = Mathf.FloorToInt(musicManager.speedcore[i].fraction);
                    float offset = 0;
                    for (int j = 0; j < sig; j++)
                    {
                        offset += signatures[j];
                    }
                    musicManager.speedcore[i] = new MusicManager.Speedcore(musicManager.speedcore[i - 1].time + (offset - musicManager.speedcore[i - 1].fraction) * 240 / musicManager.speedcore[i - 1].bpm, offset, musicManager.speedcore[i].bpm);
                }
                for (int i = 0; i < musicManager.measures.Count; i++)
                {
                    int sig = Mathf.FloorToInt(musicManager.measures[i].signature);
                    float offset = 0;
                    for (int j = 0; j < sig; j++)
                    {
                        offset += signatures[j];
                    }
                    MusicManager.Speedcore core = musicManager.speedcore.FindLast((MusicManager.Speedcore speed) => speed.fraction <= offset);
                    musicManager.measures[i] = new MusicManager.Measure(musicManager.measures[i].notes, signatures[sig], offset, core.time + (offset - core.fraction) * 240 / core.bpm);
                    for (int j = 0; j < musicManager.measures[i].notes.Count; j++)
                    {
                        core = musicManager.speedcore.FindLast((MusicManager.Speedcore speed) => speed.fraction <= musicManager.measures[i].offset + musicManager.measures[i].notes[j].fraction);
                        musicManager.measures[i].notes[j] = new MusicManager.Note(core.time + (musicManager.measures[i].offset + musicManager.measures[i].notes[j].fraction - core.fraction) * 240 / core.bpm, musicManager.measures[i].notes[j].fraction, musicManager.measures[i].notes[j].impact);
                    }
                }
                StartCoroutine(WaitUntilSuccess());
            }

            private IEnumerator LoadAudio(string file)
            {
                using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(file, AudioType.OGGVORBIS))
                {
                    uwr.SendWebRequest();
                    while (!uwr.isDone)
                    {
                        yield return null;
                    }
                    if (uwr.result == UnityWebRequest.Result.Success)
                    {
                        musicManager.audioSource.clip = DownloadHandlerAudioClip.GetContent(uwr);
                        success = true;
                    }
                    done = true;
                }
            }

            private IEnumerator WaitUntilSuccess()
            {
                while (!done)
                {
                    yield return null;
                }
                load.interactable = true;
                play.interactable = success;
                musicManager.Initialize();
            }
        }
    }
}