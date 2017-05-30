
using UnityEngine;
using System.Collections;

public class Music : MonoBehaviour
{
    private static Music instance;

    public static Music Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Music>();
                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(Music).Name;
                    instance = obj.AddComponent<Music>();
                }
            }
            return instance;
        }
    }

    public AudioClip[] menuSong;
    public AudioClip[] gameSong;
    int curPlaying;
    float baseVolume;

    public AudioSource source;

    // Use this for initialization
    void Start ()
    {
        int rand = Random.Range(0, menuSong.Length - 1);
        source.clip = menuSong[rand];
        curPlaying = -1;
        source.Play();
        baseVolume = source.volume;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (!source.isPlaying && (WorldData.Instance.matchProgress < WorldData.GameState.STARTING ||  WorldData.Instance.matchProgress >= WorldData.GameState.MAIN))
        {
            NextSong();
        }

        TogglePause();
    }

    public void NextSong()
    {
        int rand = 0;
        do
        {
            rand = Random.Range(0, gameSong.Length - 1);
        } while (rand == curPlaying);
        curPlaying = rand;

        source.clip = gameSong[curPlaying];
        source.Play();
    }

    public void TogglePause()
    {
        if (WorldData.Instance.isPaused)
        {
            source.volume = baseVolume / 2f;
        }
        else
            source.volume = baseVolume;
    }

    public void SetVolume(float percent)
    {
        source.volume = baseVolume * percent;
    }

    public void Play(bool b)
    {
        if (b)
            source.Play();
        else
            source.Pause();
    }
}
