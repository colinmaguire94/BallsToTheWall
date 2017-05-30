using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WorldData : MonoBehaviour
{
 
    public MatchOptions matchOptions;
    public AudioPlaying audio;

    public GameObject obstacles;
    Obstacle[] obs;
    Booster[] boosts;
    Powerup[] pows;
    public Vector2[] obstacleTimes;
    public float obsFlashTimer;
    public int obsFlash;
    int curObs;
    bool obsUp;
    [HideInInspector]
    public bool obsActivity = true;


    public Vector2[] powerUpTimes;
    public float powFlashTimer;
    public int powFlash;
    int curPow;
    bool powUp;
    [HideInInspector]
    public bool powUpActivity = false;

    [HideInInspector]
    public int[] powUpAvailable;
    [HideInInspector]
    public int[] obsAvailable;

    public Timer time;
    float startDuration;

    float TimePassed { get { return startDuration - time.timeLeft; } }

    public Text countdownTimer;
    public Text winningTeam;
    public Text redScore;
    public Text blueScore;

    private bool obsCoRunning = false;
    private bool powCoRunning = false;

    private int redStreak, blueStreak;

    [HideInInspector]
    public bool powCheat = false, obsCheat = false;
    public bool timerPause = false;

    public void Initialize()
    {
        Random.InitState(System.DateTime.Now.Millisecond);

        obsUp = false;
        curObs = 0;
        powUp = false;
        curPow = 0;

        redStreak = 0;
        blueStreak = 0;
        saidItAlready = false;

        obs = obstacles.GetComponentsInChildren<Obstacle>();
        boosts = obstacles.GetComponentsInChildren<Booster>();
        pows = obstacles.GetComponentsInChildren<Powerup>();

        time.enabled = false;
    }

 


    // Update is called once per frame
    bool saidItAlready = false;
    void Update()
    {
        HandleObstacles();
        HandlePowerups();

        EnsureDemoMode();
        if (wasDemoMode && !isDemoMode) TurnOffDemo();

        if ((redStreak == 3) || (blueStreak == 3))
        {
            //audio.scoreStreak();
            audio.EventOccurred(AudioPlaying.AnnouncementType.SCORESTREAK, AudioPlaying.AudioPriority.HIGH, 2f);
            redStreak = 0;
            blueStreak = 0;
        }

        if (Mathf.Abs(scoreBlue - scoreRed) == 3 && !saidItAlready)
        {
            //audio.largeScore();
            audio.EventOccurred(AudioPlaying.AnnouncementType.LARGESCORE, AudioPlaying.AudioPriority.HIGH, 2f);
            saidItAlready = true;
        }
    }


    int activateObIndexRed, activateObIndexBlue;
    void HandleObstacles()
    {
        if (obsActivity)
        {
            if (obstacleTimes.Length > 0 && curObs < obstacleTimes.Length || obsCheat)
            {
                if (obsUp)
                {
                    if (TimePassed > GetCurrentObstacleEndTime())
                    {
                        obs[activateObIndexRed].changeToType = Obstacle.ObstacleType.NULL;
                        obstacles.transform.GetChild(activateObIndexRed).GetComponent<Rotate>().enabled = true;
                        boosts[activateObIndexRed].enabled = false;

                        obs[activateObIndexBlue].changeToType = Obstacle.ObstacleType.NULL;
                        obstacles.transform.GetChild(activateObIndexBlue).GetComponent<Rotate>().enabled = true;
                        boosts[activateObIndexBlue].enabled = false;

                        //for (int i = 0; i < obs.Length; i++)
                        //{
                        //    obs[i].changeToType = Obstacle.ObstacleType.NULL;
                        //    obstacles.transform.GetChild(i).GetComponent<Rotate>().enabled = true;
                        //}
                        //foreach (Boost b in boosts)
                        //{
                        //    //b.GetComponent<Rotate>().enabled = boostersEnabled;
                        //    b.enabled = boostersEnabled;
                        //}
                        curObs++;
                        obsUp = false;
                    }
                }
                else
                {
                    if (TimePassed > GetNextObstacleStartTime(false) || obsCheat)
                    {
                        if (obsCoRunning == false)
                            StartCoroutine(popupObs(obsFlash, obsFlashTimer));
                    }
                }
            }
        }
    }

    IEnumerator popupObs(int cycles, float time)
    {
        obsCoRunning = true;
        activateObIndexRed = Random.Range(0, 2);
        boosts[activateObIndexRed].GetComponent<Flash>().FlashMeshes(Color.red, Color.red, time / ((float)cycles * 2f), time / ((float)cycles * 2f), cycles);
        activateObIndexBlue = Random.Range(2, 4);
        boosts[activateObIndexBlue].GetComponent<Flash>().FlashMeshes(Color.red, Color.red, time / ((float)cycles * 2f), time / ((float)cycles * 2f), cycles);

        yield return new WaitForSeconds(time);

        boosts[activateObIndexRed].enabled = false;
        boosts[activateObIndexBlue].enabled = false;

        int rand = Random.Range(1, 3);
        obs[activateObIndexRed].changeToType = (Obstacle.ObstacleType)rand;
        obs[activateObIndexBlue].changeToType = (Obstacle.ObstacleType)rand;
        //    obstacles.transform.GetChild(i).GetComponent<Rotate>().enabled = false;
        //for(int i = 0; i < obs.Length; i++)
        //{
        //    obs[i].changeToType = (Obstacle.ObstacleType)rand;
        //    obstacles.transform.GetChild(i).GetComponent<Rotate>().enabled = false;
        //}
        obsUp = true;
        obsCoRunning = false;
    }

    int activatePowIndexRed, activatePowIndexBlue;
    void HandlePowerups()
    {
        if (powUpActivity)
        {
            if (powerUpTimes.Length > 0 && curPow < powerUpTimes.Length || (powCheat))
            {
                if (powUp)
                {
                    if (TimePassed > GetCurrentPowerupEndTime())
                    {
                        foreach (Powerup p in pows)
                        {
                            p.nextType = Powerup.PowerupType.NULL;
                        }

                        if (boostersEnabled)
                        {
                            SetBoosters(true);
                        }

                        curPow++;
                        powUp = false;
                        powCheat = false;
                    }
                }
                else
                {
                    if (TimePassed > GetNextPowerupStartTime(false) || powCheat)
                    {
                        if (!powCoRunning)
                            StartCoroutine(popupPow(powFlash, powFlashTimer));
                    }
                }
            }
        }
    }

    IEnumerator popupPow(int cycles, float time)
    {
        //audio.pickupSpawn();
        audio.EventOccurred(AudioPlaying.AnnouncementType.PICKUPSPAWN, AudioPlaying.AudioPriority.REQUIRED, 2f);

        powCoRunning = true;

        activatePowIndexRed = Random.Range(0, 2);
        activatePowIndexBlue = Random.Range(2, 4);

        //foreach (Boost b in boosts)
        //{
        //    Flash f = b.GetComponent<Flash>();
        //    f.FlashMeshes(Color.green, Color.green, time / ((float)cycles * 2f), time / ((float)cycles * 2f), cycles);
        //}

        //yield return new WaitForSeconds(time);

        //foreach (Powerup p in pows)
        //{
        //    int rand = Random.Range(1, 3);
        //    p.nextType = (Powerup.PowerupType)rand;
        //}

        //if (boostersEnabled)
        //{
        //    SetBoosters(false);
        //}

        boosts[activatePowIndexRed].GetComponent<Flash>().FlashMeshes(Color.green, Color.green, time / ((float)cycles * 2f), time / ((float)cycles * 2f), cycles);
        boosts[activatePowIndexBlue].GetComponent<Flash>().FlashMeshes(Color.green, Color.green, time / ((float)cycles * 2f), time / ((float)cycles * 2f), cycles);

        yield return new WaitForSeconds(time);

        boosts[activatePowIndexRed].enabled = false;
        boosts[activatePowIndexBlue].enabled = false;

        int rand = Random.Range(1, (int)Powerup.PowerupType.MAX);
        pows[activatePowIndexRed].nextType = (Powerup.PowerupType)rand;
        rand = Random.Range(1, (int)Powerup.PowerupType.MAX);
        pows[activatePowIndexBlue].nextType = (Powerup.PowerupType)rand;

        powUp = true;

        powCoRunning = false;
    }

    public IEnumerator StartGame()
    {
        
        //audio.matchStart();
        audio.EventOccurred(AudioPlaying.AnnouncementType.START, AudioPlaying.AudioPriority.REQUIRED, 3f);
        // countdown
        

        Music.Instance.NextSong();

    }

   

    IEnumerator EndGame()
    {

        audio.PlayMatchEffect(AudioPlaying.MatchEffect.HORN);



    public void PointScored(TeamManagement.Team team)
    {
        //audio.score();
        audio.EventOccurred(AudioPlaying.AnnouncementType.SCORE, AudioPlaying.AudioPriority.HIGH, 3f);
        switch (team)
        {
            case TeamManagement.Team.RED:
                scoreRed++;
                redStreak++;
                blueStreak = 0;
                redScore.text = scoreRed.ToString();
                break;
            case TeamManagement.Team.BLUE:
                scoreBlue++;
                blueStreak++;
                redStreak = 0;
                blueScore.text = scoreBlue.ToString();
                break;
        }
    }

	[HideInInspector]
	public bool counting = false;
    public IEnumerator Countdown()
    {       
		counting = true;
        // countdown
        int count = 0;
        countdownSprite.sprite = countdown[count];
        countdownSprite.enabled = true;
		
        while (count < 3)
        {
            //countdownTimer.text = count.ToString();
            countdownSprite.sprite = countdown[count];
            audio.PlayMatchEffect(AudioPlaying.MatchEffect.BEEP_HIGH);
            //countdown.sprite = countdown.sprite.texture.;
            yield return new WaitForSeconds(1f);
            count++;
        }
        audio.PlayMatchEffect(AudioPlaying.MatchEffect.BEEP_LOW);
        countdownSprite.sprite = countdown[count];

        //countdownTimer.text = "GO!";
        yield return new WaitForSeconds(1f);

        // clear screen
        //countdownTimer.text = "";
        countdownSprite.enabled = false;
		counting = false;
    }

    public void changeObsTypes(Obstacle.ObstacleType ot)
    {
        foreach (Obstacle o in obs)
        {
            o.changeToType = ot;
        }
    }

    public void chagePowTypes(Powerup.PowerupType pt)
    {
        foreach (Powerup p in pows)
        {
            p.changeType(pt);
        }
    }

    public void nextPow()
    {
        foreach (Powerup p in pows)
        {
            p.type = p.type + 1;
            p.nextType = p.nextType + 1;

            if (p.type == Powerup.PowerupType.MAX)
            {
                p.type = Powerup.PowerupType.NULL;
                p.nextType = Powerup.PowerupType.NULL;
            }
        }
    }

    
    public void SetBoosters(bool active)
    {
        foreach (Booster b in boosts)
        {
            b.enabled = active;
        }
    }

    float powUpFrequency = 30f;
    float GetNextPowerupStartTime(bool increment)
    {
        if (increment)
        {
            curPow++;
        }

        float nextTime = 0f;

        if (powerUpTimes.Length == 0)
        {
            nextTime = TimePassed;
        }
        else if (powerUpTimes.Length == 1)
        {
            nextTime = powerUpTimes[0].x + powUpFrequency * curPow;
        }
        else
        {
            nextTime = powerUpTimes[curPow].x;
        }

        return nextTime;
    }

    float GetCurrentPowerupEndTime()
    {
        float nextTime = 0f;

        if (powerUpTimes.Length == 0)
        {
            nextTime = TimePassed;
        }
        else if (powerUpTimes.Length == 1)
        {
            nextTime = powerUpTimes[0].y + powUpFrequency * curPow;
        }
        else
        {
            nextTime = powerUpTimes[curPow].y;
        }

        return nextTime;
    }

    float obsFrequency = 30f;
    float GetNextObstacleStartTime(bool increment)
    {
        if (increment)
        {
            curObs++;
        }

        float nextTime = 0f;

        if (obstacleTimes.Length == 0)
        {
            nextTime = TimePassed;
        }
        else if (obstacleTimes.Length == 1)
        {
            nextTime = obstacleTimes[0].x + obsFrequency * curObs;
        }
        else
        {
            nextTime = obstacleTimes[curObs].x;
        }

        return nextTime;
    }

    float GetCurrentObstacleEndTime()
    {
        float nextTime = 0f;

        if (obstacleTimes.Length == 0)
        {
            nextTime = TimePassed;
        }
        else if (obstacleTimes.Length == 1)
        {
            nextTime = obstacleTimes[0].y + obsFrequency * curObs;
        }
        else
        {
            nextTime = obstacleTimes[curPow].y;
        }

        return nextTime;
    }
}
