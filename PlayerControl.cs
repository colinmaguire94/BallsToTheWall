using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour
{
    
    #endregion
    public enum SpeedTierBase { NONE = -1, SLOW, MEDIUM, FAST, MAX }
    public enum SpeedTierRelative { NONE = -2, REDUCED, SLOW, MEDIUM, FAST, INCREASED, MAX }
    public enum WeightTier { NONE = -2, REDUCED, SMALL, MEDIUM, LARGE, INCREASED, MAX }


    public bool invulnerable = false;
    public bool unstoppable = false;
    public bool destructive = false;
    public bool ignoreMaxSpeed;
    public bool ignorePerfectControl;


    public Powerup.PowerupType activePowerup;
    public float powerUpTime;


    public void Initialize()
    {
        
        activePowerup = Powerup.PowerupType.NULL;

        
        powerupEffects = new GameObject[temp.Length];
        for(int i = 0; i < powerupEffects.Length; i++)
        {
            powerupEffects[i] = temp[i].gameObject;
        }

        foreach (GameObject go in powerupEffects)
        {
            go.SetActive(false);
        }

      
    }

    // Update is called once per frame
    void Update()
    {

        // powerups
        powerUpTime -= Time.deltaTime;
        if (activePowerup != Powerup.PowerupType.NULL && powerUpTime <= 0f)
        {
            DeactivatePowerup();
        } 
    }

  

    #region powerups
    public void ActivatePowerup(Powerup.PowerupType type, float duration)
    {
        DeactivatePowerup();

        activePowerup = type;
        powerUpTime = duration;

        switch (activePowerup)
        {
            case Powerup.PowerupType.NULL:
                break;
            case Powerup.PowerupType.SPEED:
                if (!p_isSpeedActive)
                    StartCoroutine(Speed());
                break;
            case Powerup.PowerupType.STRENGTH:
                if (!p_isStrengthActive)
                    StartCoroutine(Strength());
                break;
            case Powerup.PowerupType.BALLS2WALL:
                // play animation
                if (!p_isB2WActive)
                    StartCoroutine(BallsToTheWall());
                break;
            case Powerup.PowerupType.BUSTER:
                if (!p_isBusterActive)
                    StartCoroutine(BallBuster());
                break;
            case Powerup.PowerupType.STICKYFEET:
                bubble.autoAimDist = 5f;
                bubble.autoAimAccuracy = 5f;
                break;
            case Powerup.PowerupType.SHIELD:
                // TODO: turn on shield
                numShields++;
                break;
        }

        powerupEffects[(int)activePowerup].SetActive(true);
    }

    public void DeactivatePowerup()
    {
        switch (activePowerup)
        {
            case Powerup.PowerupType.NULL:
                break;
            case Powerup.PowerupType.SPEED:
                //if (isBubbleMode)
                //    bubble.speedIncrease = 1.0f;
                //else
                //    legs.speedIncrease = 1.0f;
                break;
            case Powerup.PowerupType.STRENGTH:
                break;
            case Powerup.PowerupType.SHIELD:
                numShields--;           
                break;
            case Powerup.PowerupType.STICKYFEET:
                bubble.autoAimDist = 0f;
                bubble.autoAimAccuracy = 0f;
                break;
        }

        powerupEffects[(int)activePowerup].SetActive(false);

        activePowerup = Powerup.PowerupType.NULL;
    }


    IEnumerator BallBuster()
    {
        // play activate animation
        p_isBusterActive = true;
        SetMode(true);
        // TODO: Add stomp animation and effect
        bubble.transform.position += Vector3.up * 1.5f;
        bubble.HoldPosition(0.5f);
        yield return new WaitForSeconds(0.5f);

        // wait until powerup ends
        while(activePowerup == Powerup.PowerupType.BUSTER)
        {
            yield return null;
        }

        // play deactivate animation
        SetMode(true);
        bubble.transform.position += Vector3.up * 1.5f;
        bubble.HoldPosition(0.25f);
    }

    IEnumerator Speed()
    {
        p_isSpeedActive = true;
        if (isBubbleMode)
        {
            bubble.speedIncrease = 1.5f;
            yield return new WaitForSeconds(0.25f);
            bubble.speedIncrease = 1.75f;
            yield return new WaitForSeconds(0.25f);
            bubble.speedIncrease = 2.0f;
            yield return new WaitForSeconds(0.25f);
        }
        else
        {
            legs.speedIncrease = 1.5f;
            yield return new WaitForSeconds(0.25f);
            legs.speedIncrease = 1.75f;
            yield return new WaitForSeconds(0.25f);
           legs.speedIncrease = 2.0f;
           yield return new WaitForSeconds(0.25f);
        }
        yield return null;
    }

    IEnumerator Strength()
    {
        p_isStrengthActive = true;
        destructive = true;
        yield return null;
    }
    #endregion
}
