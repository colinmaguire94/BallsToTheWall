using UnityEngine;
using System.Collections;

public class Powerup : MonoBehaviour {
    public enum PowerupType { NULL, SPEED, STRENGTH, BALLS2WALL, BUSTER, STICKYFEET, SHIELD, CLONE, MAX }
    public PowerupType type;
    public PowerupType nextType;

    public float spawnTime = 0.5f;

    public GameObject pUpShell;
    public GameObject[] pUpOptions;
    public float[] durations;
    public GameObject triggers;

    // Use this for initialization
    void Start () {
        pUpShell.SetActive(false);
        triggers.SetActive(false);

        foreach (GameObject go in pUpOptions)
        {
            go.gameObject.SetActive(false);
        }
	}
	
	// Update is called once per frame
	void Update () {
        if(nextType == PowerupType.NULL && type != PowerupType.NULL)
        {
            Deactivate();
        }
        else if (nextType != type)
        {
            Activate(nextType);
        }
    }

    //Transform GetPowerup(PowerupType t)
    //{
    //    return options[(int)t];
    //}

    public void Deactivate()
    {
        int pow = (int)type;
        if (type == PowerupType.MAX)
            pow = 0;

        pUpShell.SetActive(false);
        triggers.SetActive(false);
        pUpOptions[pow].SetActive(false);
        nextType = PowerupType.NULL;
        type = PowerupType.NULL;
    }

    public void Activate(PowerupType t)
    {
        Deactivate();
        nextType = t;
        StartCoroutine(Activate());
    }

    IEnumerator Activate()
    {
        type = nextType;

        pUpShell.SetActive(true);

        yield return new WaitForSeconds(spawnTime);

        int pow = (int)type;
        if (type == PowerupType.MAX)
            pow = 0;

        pUpOptions[pow].SetActive(true);
        triggers.SetActive(true);

        yield return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyPowerup(other.GetComponentInParent<PlayerControl>());
            Deactivate();
        }
    }

    void ApplyPowerup(PlayerControl player)
    {
        //player.activePowerup = type;
        //switch (type)
        //{
        //    case PowerupType.NULL:
        //        break;
        //    case PowerupType.SPEED:
        //    case PowerupType.STRENGTH:
        //        break;
        //}
        player.ActivatePowerup(type, 10f);
    }

    public void changeType(PowerupType pt)
    {
        type = pt;
        nextType = pt;
    }
}
