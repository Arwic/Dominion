using ArwicEngine.Core;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

    public class AudioManager : IEngineComponent
    {
        public Engine Engine { get; }

        public MusicPlayerState PlayerState { get; set; }
        public LinkedList<SoundEffect> MusicQueue { get; set; }
        private LinkedListNode<SoundEffect> currentQueueNode;
        private SoundEffect currentMusicEffect;
        private SoundEffectInstance currentMusic;
        private Thread musicWorker;
        private bool musicWorker_running;
        public string CurrentTrackName => currentMusicEffect.Name;
        public float MusicVolume { get; private set; }

        public event EventHandler<SoundEffectEventArgs> MusicChanged;
        public event EventHandler MusicQueueEnded;

        protected virtual void OnMusicChanged(SoundEffectEventArgs args)
        {
            if (MusicChanged != null)
                MusicChanged(this, args);
        }
        protected virtual void OnMusicQueueEnded(EventArgs args)
        {
            if (MusicQueueEnded != null)
                MusicQueueEnded(this, args);
        }

        public AudioManager(Engine engine)
        {
            Engine = engine;
            MusicQueue = new LinkedList<SoundEffect>();
            Apply();
            musicWorker_running = true;
            musicWorker = new Thread(MusicWorker_Go);
            musicWorker.Start();
        }

        public void Apply()
        {
            try
            {
                MusicVolume = Convert.ToSingle(Engine.Config.GetVar(CONFIG_MUSICVOLUME));
                if (currentMusic != null)
                    currentMusic.Volume = MusicVolume;
            }
            catch (Exception)
            {

            }
        }

        public void Shutdown()
        {
            musicWorker_running = false;
            StopMusic();
        }

        private void MusicWorker_Go()
        {
            while (musicWorker_running)
            {
                if (currentMusic == null)
                {
                    Thread.Sleep(10);
                    if (currentMusic == null)
                        NextMusicTrack();
                }
                else if (currentMusic.State == SoundState.Stopped)
                {
                    Thread.Sleep(10);
                    if (currentMusic.State == SoundState.Stopped)
                        NextMusicTrack();
                }
            }
        }

        public void NextMusicTrack()
        {
            StopMusic();
            switch (PlayerState)
            {
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
                case MusicPlayerState.RepeatOrder:
                    if (currentQueueNode.Next == null)
                        currentQueueNode = MusicQueue.First;
                    else
                        currentQueueNode = currentQueueNode.Next;
                    PlayMusic(currentQueueNode.Value);
                    break;
                case MusicPlayerState.RepeatOne:
                    PlayMusic(currentMusicEffect);
                    break;
                default:
                    break;
            }
        }

        public void PlayMusic(SoundEffect music)
        {
            StopMusic();
            currentMusicEffect = music;
            currentMusic = currentMusicEffect.CreateInstance();
            currentMusic.IsLooped = false;
            currentMusic.Volume = MusicVolume;
            currentMusic.Play();
            Engine.Console.WriteLine($"Now playing: {currentMusicEffect.Name} ({currentMusicEffect.Duration})");
        }

        public void ResumeMusic()
        {
            if (currentMusic != null)
                currentMusic.Resume();
        }

        public void PauseMusic()
        {
            if (currentMusic != null)
                currentMusic.Pause();
        }

        public void StopMusic()
        {
            if (currentMusic != null)
                currentMusic.Stop();
        }

        public SoundState GetMusicState()
        {
            if (currentMusic != null)
                return currentMusic.State;
            return SoundState.Stopped;
        }
    }
}
