// Dominion - Copyright (C) Timothy Ings
// AudioManager.cs
// This file defines classes that manage audio playback

using ArwicEngine.Core;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Threading;

using static ArwicEngine.Constants;

namespace ArwicEngine.Audio
{
    public enum MusicPlayerState
    {
        Ordered,
        Shuffle,
        RepeatOrder,
        RepeatOne
    }

    public class SoundEffectEventArgs : EventArgs
    {
        public SoundEffect SoundEffect { get; }

        public SoundEffectEventArgs(SoundEffect soundEffect)
        {
            SoundEffect = soundEffect;
        }
    }
    
    /// <summary>
    /// Manages audio playback
    /// </summary>
    public sealed class AudioManager
    {
        // Singleton pattern
        private static object _lock_instance = new object();
        private static readonly AudioManager _instance = new AudioManager();
        public static AudioManager Instance
        {
            get
            {
                lock (_lock_instance)
                {
                    return _instance;
                }
            }
        }

        /// <summary>
        /// Gets or sets the value the defines the state of the music player
        /// </summary>
        public MusicPlayerState PlayerState { get; set; }

        /// <summary>
        /// Gets or sets a list of sound effects to be played
        /// </summary>
        public LinkedList<SoundEffect> MusicQueue { get; set; } = new LinkedList<SoundEffect>();

        private LinkedListNode<SoundEffect> currentQueueNode;
        private SoundEffect currentMusicEffect;
        private SoundEffectInstance currentMusic;
        private Thread musicWorker;
        private bool musicWorker_running = true;

        /// <summary>
        /// Gets the name of the currently playing music track
        /// </summary>
        public string CurrentTrackName => currentMusicEffect == null ? "N/A" : currentMusicEffect.Name;

        /// <summary>
        /// Gets or sets the current music playback volume
        /// </summary>
        public float MusicVolume { get; private set; }

        /// <summary>
        /// Occurs when the music track is changed
        /// </summary>
        public event EventHandler<SoundEffectEventArgs> MusicChanged;
        /// <summary>
        /// Occurs when the music queue has ended
        /// </summary>
        public event EventHandler MusicQueueEnded;

        private void OnMusicChanged(SoundEffectEventArgs args)
        {
            if (MusicChanged != null)
                MusicChanged(this, args);
        }
        private void OnMusicQueueEnded(EventArgs args)
        {
            if (MusicQueueEnded != null)
                MusicQueueEnded(this, args);
        }

        /// <summary>
        /// Creates a new audio manager
        /// </summary>
        private AudioManager()
        {
            Apply();
            musicWorker = new Thread(MusicWorker_Go);
            musicWorker.Start();
        }

        /// <summary>
        /// Applies settings to the currently playing audio
        /// </summary>
        public void Apply()
        {
            try
            {
                // set variables
                MusicVolume = Convert.ToSingle(ConfigManager.Instance.GetVar(CONFIG_AUD_MUSIC));

                // update current music with new variables
                if (currentMusic != null)
                    currentMusic.Volume = MusicVolume;
            }
            catch (Exception)
            {
                // report error
                ConsoleManager.Instance.WriteLine("Error applying audio settings", MsgType.Failed);
            }
        }

        /// <summary>
        /// Stops the audio manager
        /// </summary>
        public void Shutdown()
        {
            // stops music worker thread
            musicWorker_running = false;
            // stops music playing
            StopMusic();
        }

        private void MusicWorker_Go()
        {
            while (musicWorker_running)
            {
                if (currentMusic == null)
                {
                    // don't run too fast
                    Thread.Sleep(10);
                    // if the current track is over, play the next track
                    if (currentMusic == null)
                        NextMusicTrack();
                }
                else if (currentMusic.State == SoundState.Stopped)
                {
                    // don't run too fast
                    Thread.Sleep(10);
                    // if the current track has been stopped, play the next track
                    if (currentMusic.State == SoundState.Stopped)
                        NextMusicTrack();
                }
            }
        }

        /// <summary>
        /// Plays the next music track in the music queue
        /// </summary>
        public void NextMusicTrack()
        {
            // stop the current track
            StopMusic();
            switch (PlayerState)
            {
                // play track queue in order
                case MusicPlayerState.Ordered:
                    if (MusicQueue != null && MusicQueue.Count > 0)
                    {
                        SoundEffect nextTrack = MusicQueue.Dequeue();
                        PlayMusic(nextTrack);
                    }
                    else
                    {
                        OnMusicQueueEnded(new SoundEffectEventArgs(null));
                    }
                    break;
                // play a random track from the queue
                case MusicPlayerState.Shuffle:
                    if (MusicQueue != null && MusicQueue.Count > 0)
                    {
                        int i = RandomHelper.Next(0, MusicQueue.Count);
                        SoundEffect nextTrack = MusicQueue.Get(i);
                        if (nextTrack != null)
                            PlayMusic(nextTrack);
                    }
                    else
                    {
                        PlayMusic(currentMusicEffect);
                    }
                    break;
                // play the music queue over and over
                case MusicPlayerState.RepeatOrder:
                    if (currentQueueNode.Next == null)
                        currentQueueNode = MusicQueue.First;
                    else
                        currentQueueNode = currentQueueNode.Next;
                    PlayMusic(currentQueueNode.Value);
                    break;
                // play the same track over and over
                case MusicPlayerState.RepeatOne:
                    PlayMusic(currentMusicEffect);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Plays the current music track
        /// </summary>
        /// <param name="music"></param>
        public void PlayMusic(SoundEffect music)
        {
            // stop the current track
            StopMusic();
            // set up new track
            currentMusicEffect = music;
            currentMusic = currentMusicEffect.CreateInstance();
            currentMusic.IsLooped = false;
            currentMusic.Volume = MusicVolume;
            // play the new track
            currentMusic.Play();
            ConsoleManager.Instance.WriteLine($"Now playing: {currentMusicEffect.Name} ({currentMusicEffect.Duration})");
        }

        /// <summary>
        /// Resumes the currently paused music track
        /// </summary>
        public void ResumeMusic()
        {
            if (currentMusic != null)
                currentMusic.Resume();
        }

        /// <summary>
        /// Pauses the currently playing music track
        /// </summary>
        public void PauseMusic()
        {
            if (currentMusic != null)
                currentMusic.Pause();
        }

        /// <summary>
        /// Stops the currently playing muci track
        /// </summary>
        public void StopMusic()
        {
            if (currentMusic != null)
                currentMusic.Stop();
        }

        /// <summary>
        /// Returns the current sound state
        /// </summary>
        /// <returns></returns>
        public SoundState GetMusicState()
        {
            if (currentMusic != null)
                return currentMusic.State;
            return SoundState.Stopped;
        }
    }
}
