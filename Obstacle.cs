//---------------------------------------------------------
// Obstacle
// Class that controls the obstacles in the game.
// Colin Maguire    2017-01-10

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Obstacle : MonoBehaviour {
    public enum ObstacleType {NULL, PUSHER, PULLER, BOOST, MAX};

    public ObstacleType currentType = ObstacleType.NULL;
    public ObstacleType changeToType = ObstacleType.NULL;
    ForceMode forceMode;
    public bool changeObType = false;
    public GameObject lid;
    public GameObject pusher;
    public GameObject puller;
    public GameObject lightsYellow;
    public GameObject lightsRed;

    public bool boostersActive = true;

    public float pushForce;
    public float pullForce;
    public float pullDelay;
    public GameObject pullEffect;
    public float pullRange = 13f;
    float delay;

    List<GameObject> pullerTargets;

    WorldData worldData;

    private void OnDrawGizmos()
    {
        if(currentType == ObstacleType.PULLER)
            Gizmos.DrawSphere(transform.position + new Vector3(0, -0.02428718f, 0), pullRange);
    }

    //-----------------------------------------
    // Awake
    // Sets up which model and collider should be used, based on the obstacletype.
    // Created:     Colin Maguire 2017-01-13
    // Edited: Colin Maguire 2017-01-13
    // Commented out push action, updated tge puller part to check if the collider is a player or not.
    void Awake()
    {
        currentType = ObstacleType.NULL;
        changeObType = false;
        pullerTargets = new List<GameObject>();

        pullRange = pullEffect.transform.lossyScale.x / 2f;
        //ChangeType(ObstacleType.PULLER);

        worldData = GameObject.Find("WorldData").GetComponent<WorldData>();
    }

    void Start()
    {
        //pusher.GetComponent<Animation>().
    }

    //-----------------------------------------
    // Update
    // Currently just here to test features.
    // Created:     Colin Maguire 2017-01-13
    // Edited: Colin Maguire 2017-01-13
    // Created function and added test code.
    void Update()
    {
        if (changeToType != currentType)
        {
            ChangeType(changeToType);
        }

        if(currentType == ObstacleType.PULLER)
        {
            
            Animation anim = puller.GetComponent<Animation>();
            if (!anim.IsPlaying("Start") && !anim.IsPlaying("End"))
            {
                if (!anim.IsPlaying("Spin"))
                {
                    anim.Play("Spin");
                }

                delay += Time.deltaTime;
                float scale = (1 - delay / pullDelay) * pullRange * 2;
                pullEffect.transform.localScale = new Vector3(scale, scale, scale);
                if (delay > pullDelay)
                {
                    delay = 0;
                    // players
                    foreach (GameObject go in worldData.players)
                    {
                        PlayerControl pc = go.GetComponent<PlayerControl>();
                        Vector3 pos = pc.ActiveRigid.transform.position;
                        if (Vector3.Distance(puller.transform.position, pos) < pullRange)
                        {
                            //Debug.Log(go.GetComponent<PlayerControl>().playerNumber + "was pulled by " + gameObject.GetInstanceID());
                            Vector3 dir = puller.transform.position - pos;
                            dir = dir.normalized;
                            Debug.DrawRay(puller.transform.position, dir * pullRange, Color.magenta);
                            pc.ActiveRigid.velocity = Vector3.zero;
                            if (!pc.ignoreMaxSpeed)
                                StartCoroutine(pc.IgnoreMaxSpeed(1f));
                            if (!pc.ignorePerfectControl)
                                StartCoroutine(pc.IgnorePerfectControl(1f));
                            pc.TakeDamage(dir, pullForce);
                        }
                    }

                    // ball
                    if(Vector3.Distance(puller.transform.position, worldData.ball.transform.position) < pullRange)
                    {
                        //Debug.Log(gameObject.GetInstanceID() + " pulled ball at distance " + Vector3.Distance(puller.transform.position, worldData.ball.transform.position));
                        Vector3 dir = puller.transform.position - worldData.ball.transform.position;
                        worldData.ball.GetComponent<Rigidbody>().AddForce(dir * pullForce / 100, forceMode);
                    }
                }
            }
        }
    }

    //-----------------------------------------
    // OnTriggerEnter
    // Does the pull action when a player enters the trigger.
    // Created:     Colin Maguire 2017-01-10
    // Edited: Colin Maguire 2017-01-17
    // Updated the code to test with the prototype posted.
    public void OnTriggerEnter(Collider other)
    {
        if (currentType == ObstacleType.NULL)
        {
            //Debug.LogError("Obstacle Type not set.");
        }
        else if (currentType == ObstacleType.PULLER)
        {
            pullerTargets.Add(other.gameObject);
        }
        else if(currentType == ObstacleType.PUSHER)
        {
            //Do Nothing.
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (currentType == ObstacleType.NULL)
        {
            //Debug.LogError("Obstacle Type not set.");
        }
        else if (currentType == ObstacleType.PULLER)
        {
            pullerTargets.Remove(other.gameObject);
        }
        else if (currentType == ObstacleType.PUSHER)
        {
            //Do Nothing.
        }
    }

    //-----------------------------------------
    // OnCollisionEnter
    // Does the pull action when a player enters the trigger.
    // Created:     Colin Maguire 2017-01-13
    // Edited:      Colin Maguire 2017-01-17
    public void OnCollisionEnter(Collision other)
    {
        if (currentType == ObstacleType.NULL)
        {
            //Debug.LogError("Obstacle Type not set.");
        }
        else if (currentType == ObstacleType.PULLER)
        {
            //Do nothing.
        }
        else if (currentType == ObstacleType.PUSHER)
        {
            Vector3 dir = other.contacts[0].point - transform.position;
            dir.Normalize();
            if (other.gameObject.tag == "Player")
            {
                //If character is in leg mode do
                //change to ball mode.
                //change back to leg mode when done.
                PlayerControl pc = other.gameObject.GetComponentInParent<PlayerControl>();
                if (!pc.isBubbleMode)
                {
                    pc.ToggleModes();
                }
                //Rigidbody actRigid = pc.ActiveRigid;
                //other.gameObject.GetComponent<Rigidbody>().AddForce(dir * pushForce , forceMode);
                //pc.ActiveRigid.velocity = Vector3.zero;
                //if (!pc.ignoreMaxSpeed)
                //    StartCoroutine(pc.IgnoreMaxSpeed(1f));
                //if (!pc.ignorePerfectControl)
                //    StartCoroutine(pc.IgnorePerfectControl(1f));
                pc.TakeDamage(dir, pushForce);
            }
            pusher.GetComponent<Animation>().Play("Hit");
        }
    }

    //-----------------------------------------
    // OnTriggerEnter
    // Changes the obstacleType.
    // Changes the type of the obstacle and sets the other side of the object not active.
    // Created:     Colin Maguire  2017-01-10
    // Edit:        Colin Maguire    2017-01-21
    // removed loops, replaced with GameObject variables
    public void ChangeType(ObstacleType newType)
    {
        if (newType == ObstacleType.NULL)
            Lower();
        else
            Raise(newType);

        currentType = newType;        
        changeObType = false;
    }

    public void Raise(ObstacleType type)
    {
        if(currentType != ObstacleType.NULL)
        {
            Lower();
        }

        lid.SetActive(false);
        switch (type)
        {
            case ObstacleType.PULLER:
                puller.SetActive(true);
                puller.GetComponent<Animation>().Play("Start");
                delay = 0;
                break;
            case ObstacleType.PUSHER:
                pusher.SetActive(true);
                pusher.GetComponent<Animation>().Play("Start");
                break;
        }
    }

    public void Lower()
    {
        switch (currentType)
        {
            case ObstacleType.PULLER:
                StartCoroutine(Disable(puller));
                break;
            case ObstacleType.PUSHER:
                StartCoroutine(Disable(pusher));
                break;
        }
    }

    public void ToggleType()
    {
        if (currentType == ObstacleType.PULLER)
        {
            ChangeType(ObstacleType.PUSHER);
        }
        else if (currentType == ObstacleType.PUSHER)
        {
            ChangeType(ObstacleType.PULLER);
        }
    }

    IEnumerator Disable(GameObject toDisable)
    {
        Animation anim = toDisable.GetComponent<Animation>();
        anim.Play("End");

        while (anim.isPlaying)
        {
            yield return null;
        }

        lid.SetActive(true);
        if (boostersActive)
            lid.GetComponent<Booster>().enabled = true;

        toDisable.SetActive(false);
    }

    public void SetBoostersActive(bool isActive)
    {
        lid.GetComponent<Booster>().enabled = isActive;
    }
}
