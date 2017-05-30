using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class AudioPlaying : MonoBehaviour
{

    private static AudioPlaying instance;

    public static AudioPlaying Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioPlaying>();
                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(AudioPlaying).Name;
                    instance = obj.AddComponent<AudioPlaying>();
                }
            }
            return instance;
        }
    }

    #region helper classes

    [Serializable]
    public enum ClipProgrssionType { NONE, LINEAR, RANDOM, MAX }
    public class AudioClipManager<T>
    {
        public string name;

        public AudioClipManager()
        {

        }      

        public float playChance;
        public AudioClip silence;
        public AudioClip[] clips;
        public float[] clipPlayChance;
        public ClipProgrssionType progression;
        //[HideInInspector]
        public int[] playCount;

        public T type;
        //public PlayerReactionType playerType;

        public int Count { get { int c = 0; foreach (int i in playCount) c += i; return c; } }

        int prevClipIndex = 0;
        public int PrevClipIndex
        {
            get { return prevClipIndex; }
            set
            {
                if (value < 0) prevClipIndex = clips.Length - 1;
                else if (value >= clips.Length) prevClipIndex = 0;
                else prevClipIndex = value;
            }
        }

        // change comparative play ratios into probabilities for use with random selection
        public void NormalizeClipPlayChance()
        {
            // determine sum
            float sum = 1;
            foreach (float c in clipPlayChance)
            {
                sum += c;
            }
            // normalize
            for (int i = 0; i < clipPlayChance.Length; i++)
            {
                float c = 0f;
                clipPlayChance[i] /= sum;
                if (i > 0)
                {
                    clipPlayChance[i] += clipPlayChance[i - 1];
                    // if something becomes impossible to play, give it at a least a 1% chance to play
                    if (clipPlayChance[i] >= 1f)
                    {
                        clipPlayChance[i] = (1f - (float)(clipPlayChance.Length - i) / 100f);
                    }
                }
            }
        }

        public int GetNextClipIndex()
        {
            // check if we should play anything at all
            if (clips.Length > 0)
            {
                float rand = UnityEngine.Random.Range(0f, 1f);
                if (rand < playChance)
                {
                    // check if we want the next clip or a random clip
                    switch (progression)
                    {
                        case ClipProgrssionType.LINEAR:
                            int x = PrevClipIndex++;
                            return x;
                            break;
                        case ClipProgrssionType.RANDOM:
                            rand = UnityEngine.Random.Range(0f, 1f);
                            for (int i = clips.Length - 1; i >= 0; i--)
                            {
                                float skipMod = 0f;
                                if (Count > 0)
                                    skipMod = (float)playCount[i] / (float)Count;

                                if (rand > clipPlayChance[i] - skipMod)
                                {
                                    playCount[i]++;
                                    return i;
                                }
                            }
                            break;
                        case ClipProgrssionType.NONE:
                            return prevClipIndex;
                            break;
                    }
                }
            }
            return -1;
        }

        //public void PlayNextClip(AudioSource source)
        //{

        //}
    }

    public enum MatchEffect { NONE, HORN, BELL, URGENT, BEEP_LOW, BEEP_HIGH, MAX }
    [Serializable]
    public class MatchProgressionEffects : AudioClipManager<MatchEffect>
    {
    }

    #endregion

    public AudioClipManager[] audioClipManager;
    public MatchProgressionEffects[] matchClipManager;
    Dictionary<AnnouncementType, AudioClipManager> announcerClips;
    Dictionary<MatchEffect, MatchProgressionEffects> matchClips;
    //Dictionary<PlayerReactionType, AudioClipManager> playerAudioClips;
    public AudioClip silence;
    float prevClipPlayTime;
    public float minTimeBetweenClips;
    public float maxTimeBetweenClips;

    #region audioClip arrays .
    //public AudioClip[] matchStartClips;
    //public AudioClip[] matchEndClips;
    //public AudioClip[] saveClips;
    //public AudioClip[] scoreClips;
    ////public AudioClip[] kickClips;
    //public AudioClip[] largeScoreClips;
    //public AudioClip[] downtimeClips;
    //public AudioClip[] pickupSpawnClips;
    //public AudioClip[] deathClips;
    //public AudioClip[] possessionClips;
    //public AudioClip[] scoreStreakClips;
    //public AudioClip[] ballDestroyedClips;
    //public AudioClip[] gruntClips;
    //public AudioClip[] cheatClips;
    //public AudioClip[] pauseClips;
    //public AudioClip[] unpauseClips;
    #endregion

    #region audioClip singles
    //public AudioClip ballBust;
    //public AudioClip selectClip;
    //public AudioClip rampUseClip;
    #endregion

    public AudioSource announcerSource;
    public AudioSource sfxSource;
    public Music musicAudio;

    public enum AudioPriority { NONE, LOW, NORMAL, HIGH, REQUIRED };
    //public enum AudioType { NONE, ANNOUNCER, PLAYER, SFX, MAX};

    //public AudioType type = AudioType.NONE;
    public enum AnnouncementType { NONE, CHEAT, PAUSE, UNPAUSE, START, END, BALLDESTROYED, BALLBUSTED, DEATH, SCORE, PICKUPSPAWN, LARGESCORE, SCORESTREAK, SAVE, POSSESSIONCHANGE, DOWNTIME, KICK };
    //private AnnouncementType type = AnnouncementType.NONE;
    //public enum PlayerReactionType { NONE, GRUNT1, GRUNT2, GRUNT3, GRUNT4, HAPPY, MAX};

    public class AudioEvent
    {
        public AudioEvent(AnnouncementType aType, AudioPriority aPrior, float t, float et)
        {
            type = aType;
            priority = aPrior;
            timeOccurred = t;
            timeToExpire = et;
        }
        public AnnouncementType type;
        public AudioPriority priority;
        public float timeOccurred, timeToExpire;
    }
    private List<AudioEvent> waitingClips;

    // Use this for initialization
    void Start()
    {
        prevClipPlayTime = 0f;

        waitingClips = new List<AudioEvent>();

        // normalize
        foreach (AudioClipManager acm in audioClipManager)
        {
            acm.NormalizeClipPlayChance();
        }

        // put into more useful format
        announcerClips = new Dictionary<AnnouncementType, AudioClipManager>();
        for (int i = 0; i < audioClipManager.Length; i++)
        {
            AnnouncementType t = audioClipManager[i].announcementType;
            //PlayerReactionType pt = audioClipManager[i].playerType;
            if (!announcerClips.ContainsKey(t))
                announcerClips.Add(t, audioClipManager[i]);
        }

        matchClips = new Dictionary<MatchEffect, MatchProgressionEffects>();
        for (int i = 0; i < matchClipManager.Length; i++)
        {
            MatchEffect t = matchClipManager[i].type;
            //PlayerReactionType pt = audioClipManager[i].playerType;
            if (!matchClips.ContainsKey(t))
                matchClips.Add(t, matchClipManager[i]);
        }
    }

    void Update()
    {
        if (Time.time - prevClipPlayTime > maxTimeBetweenClips)
        {
            EventOccurred(AnnouncementType.DOWNTIME, AudioPriority.LOW, 0.5f);
        }

        if (WorldData.Instance.matchProgress >= WorldData.GameState.STARTING)
            PlayNextAnnouncerClip();

        MuffleMusic();
    }

    void MuffleMusic()
    {
        if (announcerSource.isPlaying)
        {
            musicAudio.SetVolume(0.5f);
        }
        else
        {
            musicAudio.SetVolume(1f);
        }
    }

    public void EventOccurred(AnnouncementType type, AudioPriority priority, float expirationTime)
    {
        if (announcerClips.ContainsKey(type))
        {
            AudioEvent ae = new AudioEvent(type, priority, Time.time, expirationTime);
            waitingClips.Add(ae);
        }
    }

    public void PlayNextAnnouncerClip()
    {
        float timeSinceLastClip = Time.time - prevClipPlayTime;

        if (timeSinceLastClip > minTimeBetweenClips)
        {
            if (waitingClips.Count > 0)
            {
                AudioPriority maxPriority = AudioPriority.NONE;

                int maxIndex = -1;
                for (int i = 0; i < waitingClips.Count; i++)
                {
                    if (waitingClips[i].priority > maxPriority)
                    {
                        maxPriority = waitingClips[i].priority;
                        maxIndex = i;
                    }
                }

                if (maxIndex >= 0)
                {
                    AnnouncementType type = waitingClips[maxIndex].type;
                    PlayAnnouncerClip(type);
                    waitingClips.RemoveAt(maxIndex);
                }

                for (int i = 0; i < waitingClips.Count; i++)
                {
                    if (Time.time > waitingClips[i].timeToExpire + waitingClips[i].timeOccurred)
                    {
                        waitingClips.RemoveAt(maxIndex);
                    }
                }
            }
        }
    }

    void PlayAnnouncerClip(AnnouncementType type)
    {
        if (!announcerSource.isPlaying)
        {
            int i = announcerClips[type].GetNextClipIndex();

            if (i >= 0 && i < announcerClips[type].clips.Length)
            {
                announcerSource.clip = announcerClips[type].clips[i];
                announcerSource.Play();

                prevClipPlayTime = Time.time + announcerSource.clip.length;
                //audioClips[type].playCount[i]++;
            }
            else
            {
                announcerSource.clip = silence;
            }
        }
    }

    public void PlayMatchEffect(MatchEffect type)
    {
            if (!sfxSource.isPlaying)
        {
            int i = matchClips[type].GetNextClipIndex();

            if (i >= 0 && i < matchClips[type].clips.Length)
            {
                sfxSource.clip = matchClips[type].clips[i];
                sfxSource.Play();
            }
            else
            {
                sfxSource.clip = silence;
            }
        }
    }

    [Serializable]
    public class AudioClipManager
    {
        public string name;

        public AudioClipManager()
        {

        }

        public float playChance;
        public AudioClip silence;
        public AudioClip[] clips;
        public float[] clipPlayChance;
        public ClipProgrssionType progression;
        //[HideInInspector]
        public int[] playCount;

        public AnnouncementType announcementType;
        //public PlayerReactionType playerType;

        public int Count { get { int c = 0; foreach (int i in playCount) c += i; return c; } }

        int prevClipIndex = 0;
        public int PrevClipIndex
        {
            get { return prevClipIndex; }
            set
            {
                if (value < 0) { prevClipIndex = clips.Length - 1; if (clips.Length < 1) prevClipIndex = 0; }
                else if (value >= clips.Length) prevClipIndex = 0;
                else prevClipIndex = value;
            }
        }

        // change comparative play ratios into probabilities for use with random selection
        public void NormalizeClipPlayChance()
        {
            // determine sum
            float sum = 0;
            foreach (float c in clipPlayChance)
            {
                sum += c;
            }
            if (sum == clips.Length)
                sum++;
            // normalize
            for (int i = 0; i < clipPlayChance.Length; i++)
            {
                float c = 0f;
                clipPlayChance[i] /= sum;
                if (i > 0)
                {
                    clipPlayChance[i] += clipPlayChance[i - 1];
                    // if something becomes impossible to play, give it at a least a 1% chance to play
                    if (clipPlayChance[i] >= 1f)
                    {
                        clipPlayChance[i] = (1f - (float)(clipPlayChance.Length - i) / 100f);
                    }
                }
            }
        }

        public int GetNextClipIndex()
        {
            // check if we should play anything at all
            if (clips.Length > 0)
            {
                float rand = UnityEngine.Random.Range(0f, 1f);
                if (rand < playChance)
                {
                    // check if we want the next clip or a random clip
                    switch (progression)
                    {
                        case ClipProgrssionType.LINEAR:
                            int x = PrevClipIndex++;
                            return x;
                            break;
                        case ClipProgrssionType.RANDOM:
                            rand = UnityEngine.Random.Range(0f, 1f);
                            for (int i = clips.Length - 1; i >= 0; i--)
                            {
                                float skipMod = 0f;
                                if (Count > 0)
                                    skipMod = (float)playCount[i] / (float)Count;

                                if (rand > clipPlayChance[i] - skipMod)
                                {
                                    playCount[i]++;
                                    return i;
                                }
                            }
                            break;
                    }
                }
            }
            return -1;
        }
    }
}
