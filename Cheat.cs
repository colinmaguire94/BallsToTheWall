using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//---------------------------------------------------------
// Cheat
// A cheat system that allows the user to alter the gameplay, mostly used for demos/debugging in game.
// Colin Maguire    2017-02-07

public class Cheat : MonoBehaviour {

    public InputField cheatInput;
    public Text cheatInputTxt;
    public Text cheatOutput;
    public GameObject cheatOutputObj;
    public CameraControl camera;
    public Text txtBlue, txtRed;

    private string[] inputArray = new string[3];
    private WorldData worldData;
    private bool isActive = false;
    private PlayerControl[] pc = new PlayerControl[4];
    private AudioPlaying audio;

    void Awake()
    {
        worldData = GameObject.Find("WorldData").GetComponent<WorldData>();
        for (int i = 0; i < worldData.players.Length; i++)
        {
            pc[i] = worldData.players[i].GetComponent<PlayerControl>();
        }

        audio = GameObject.Find("Audios").GetComponent<AudioPlaying>();
    }
    
    //-----------------------------------------
    // Update
    // Enables the cheat meanu when `/~ is pressed, and takes the user input if they press the return key.
    // Created:     Colin Maguire 2017-02-07
    // Edited:      Colin Maguire 2017-02-07
    // Created class and did inital coding.
    void Update () {
	    if(Input.GetKeyDown(KeyCode.BackQuote))
        {
            if (isActive == false)
            {
                cheatInput.gameObject.active = true;
                cheatOutputObj.active = true;
                cheatInput.Select();
                isActive = true;
                //Time.timeScale = 0f;
                WorldData.Instance.cameraControl.setCameraStates[(int)WorldData.CameraPositions.MENU].enabled = true;
                worldData.isPaused = true;
                worldData.isPlayerControlActive = false;
            }
            else
            {
                cheatInput.gameObject.active = false;
                cheatOutputObj.active = false;
                isActive = false;
                //Time.timeScale = 1f;
                WorldData.Instance.cameraControl.setCameraStates[(int)WorldData.CameraPositions.MENU].enabled = false;
                worldData.isPaused = false;
                worldData.isPlayerControlActive = true;
            }
        }

        if (!isActive)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log(checkCommand("kill all players"));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log(checkCommand("respawn ball"));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Debug.Log(checkCommand("toggle obs active"));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Debug.Log(checkCommand("toggle obs push"));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                Debug.Log(checkCommand("toggle obs pull"));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                Debug.Log(checkCommand("toggle powup active"));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                worldData.nextPow();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                Debug.Log(checkCommand("score add red"));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                Debug.Log(checkCommand("score add blue"));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                Debug.Log(checkCommand("set time overtime"));
            }
            else if (Input.GetKeyDown(KeyCode.Equals))
            {
                Debug.Log(checkCommand("add time 15"));
            }
            else if (Input.GetKeyDown(KeyCode.Minus))
            {
                Debug.Log(checkCommand("minus time 15"));
            }
            else if (Input.GetKeyDown(KeyCode.F1))
            {
                worldData.isDemoMode = !worldData.isDemoMode;
            }
        }
    }

    //-----------------------------------------
    // enteringInput
    // Is called when OnEndEdit is done in unity.
    // Created:     Colin Maguire 2017-02-07
    // Edited:      Colin Maguire 2017-02-07
    // Created class and did inital coding.
    public void enteringInput(Text input)
    {
        //Checks to see if anything has been inputed, if not returns.
        if(input.text != "")
        {
            //Checks to see if the command is true or not, if it is, adds the command to the messagebox for cheat, if not, it adds command not recoginzed to the messagebox.
            //cheatOutput.text += checkCommand(input.text.ToLower());
            string chk = checkCommand(input.text.ToLower());
            if (chk != "Command not recoginzed.")
            {
                //audio.cheat();
                audio.EventOccurred(AudioPlaying.AnnouncementType.CHEAT, AudioPlaying.AudioPriority.HIGH, 1f);
            }
            cheatOutput.text += chk;

            cheatOutput.text += "\n";

            Debug.Log(input.text.ToLower());
        }
        cheatInput.Select();
    }

    //-----------------------------------------
    // checkCommand
    // Checks to see if the command is vaild or not. If not vaild returns false, if valid returns true.
    // Created:     Colin Maguire 2017-02-07
    // Edited:      Colin Maguire 2017-02-17
    // Updated the cheats more.
    string checkCommand(string txt)
    {
        inputArray = txt.Split(' ');
        //Debug.Log(camera.rigBoundingBox.size);
        if (inputArray.Length == 3)
        {
            string txt2 = inputArray[0] + " " + inputArray[1];
            #region Obstacles
            if (txt2 == "toggle obs")
            {
                if (inputArray[2] == "pull")
                {
                    worldData.changeObsTypes(Obstacle.ObstacleType.PULLER);
                    return "Setting objects to pullers.";
                }
                else if (inputArray[2] == "push")
                {
                    worldData.changeObsTypes(Obstacle.ObstacleType.PUSHER);
                    return "Setting objects to pusher.";
                }
                else if (inputArray[2] == "active")
                {
                    worldData.obsActivity = !worldData.obsActivity;
                    worldData.obsCheat = !worldData.obsCheat;
                    return "Toggling the objects activity.";
                }
            }
            #endregion
            #region Players
            else if (txt2 == "toggle p1")
            {
                if (inputArray[2] == "inv")
                {
                    pc[0].invulnerable = !pc[0].invulnerable;
                    return "Toggling p1 invulnerability.";
                }
                else if (inputArray[2] == "unstop")
                {
                    pc[0].unstoppable = !pc[0].unstoppable;
                    return "Toggling p1 unstoppability.";
                }
                else if(inputArray[2] == "destruct")
                {
                    pc[0].destructive = !pc[0].destructive;
                    return "Toggling p1 destructive.";
                }
                else if(inputArray[2] == "maxspeed")
                {
                    pc[0].ignoreMaxSpeed = !pc[0].ignoreMaxSpeed;
                    return "Toggling p1 ignoring max speed.";
                }
                else
                    return "Command not recoginzed.";
            }
            else if (txt2 == "toggle p2")
            {
                if (inputArray[2] == "inv")
                {
                    pc[1].invulnerable = !pc[1].invulnerable;
                    return "Toggling p2 invulnerability.";
                }
                else if (inputArray[2] == "unstop")
                {
                    pc[1].unstoppable = !pc[1].unstoppable;
                    return "Toggling p2 unstoppability.";
                }
                else if (inputArray[2] == "destruct")
                {
                    pc[1].destructive = !pc[1].destructive;
                    return "Toggling p1 destructive.";
                }
                else if (inputArray[2] == "maxspeed")
                {
                    pc[1].ignoreMaxSpeed = !pc[1].ignoreMaxSpeed;
                    return "Toggling p1 ignoring max speed.";
                }
                else
                    return "Command not recoginzed.";
            }
            else if (txt2 == "toggle p3")
            {
                if (worldData.players.Length == 4)
                    if (inputArray[2] == "inv")
                    {
                        pc[2].invulnerable = !pc[2].invulnerable;
                        return "Toggling p3 invulnerability.3";
                    }
                    else if (inputArray[2] == "unstop")
                    {
                        pc[2].unstoppable = !pc[2].unstoppable;
                        return "Toggling p3 unstoppability.";
                    }
                    else if (inputArray[2] == "destruct")
                    {
                        pc[2].destructive = !pc[2].destructive;
                        return "Toggling p1 destructive.";
                    }
                    else if (inputArray[2] == "maxspeed")
                    {
                        pc[2].ignoreMaxSpeed = !pc[2].ignoreMaxSpeed;
                        return "Toggling p1 ignoring max speed.";
                    }
                    else
                        return "Command not recoginzed.";
                else
                    return "Sorry, " + inputArray[1] + " is not active";
            }
            else if (txt2 == "toggle p4")
            {
                if (worldData.players.Length == 4)
                    if (inputArray[2] == "inv")
                    {
                        pc[3].invulnerable = !pc[3].invulnerable;
                        return "Toggling p4 invulnerability.";
                    }
                    else if (inputArray[2] == "unstop")
                    {
                        pc[3].unstoppable = !pc[3].unstoppable;
                        return "Toggling p4 unstoppability.";
                    }
                    else if (inputArray[2] == "destruct")
                    {
                        pc[3].destructive = !pc[3].destructive;
                        return "Toggling p4 destructive.";
                    }
                    else if (inputArray[2] == "maxspeed")
                    {
                        pc[3].ignoreMaxSpeed = !pc[3].ignoreMaxSpeed;
                        return "Toggling p4 ignoring max speed.";
                    }
                    else
                        return "Command not recoginzed.";
                else
                    return "Sorry, " + inputArray[1] + " is not active";
            }
            else if(txt == "kill all players")
            {
                for(int i = 0; i < worldData.players.Length; i++)
                {
                    pc[i].GetComponent<Respawn>().Spawn();
                }
                return "Killing all players";
            }
            #endregion
            #region Powerups
            else if (txt2 == "toggle powup")
            {
                if (inputArray[2] == "active")
                {
                    worldData.powUpActivity = !worldData.powUpActivity;
                    worldData.powCheat = !worldData.powCheat;
                    return "Toggling the powerups activity.";
                }
                else if (inputArray[2] == "speed")
                {
                    worldData.chagePowTypes(Powerup.PowerupType.SPEED);
                    return "Setting powerups to speed.";
                }
                else if (inputArray[2] == "strength")
                {
                    worldData.chagePowTypes(Powerup.PowerupType.STRENGTH);
                    return "Setting powerups to strength.";
                }
                else if (inputArray[2] == "balls2wall")
                {
                    worldData.chagePowTypes(Powerup.PowerupType.BALLS2WALL);
                    return "Setting powerups to ball2wall.";
                }
                else if (inputArray[2] == "buster")
                {
                    worldData.chagePowTypes(Powerup.PowerupType.BUSTER);
                    return "Setting powerups to buster.";
                }
                else if (inputArray[2] == "stickyfeet")
                {
                    worldData.chagePowTypes(Powerup.PowerupType.STICKYFEET);
                    return "Setting powerups to stickyfeet.";
                }
                else if (inputArray[2] == "shield")
                {
                    worldData.chagePowTypes(Powerup.PowerupType.SHIELD);
                    return "Setting powerups to shield.";
                }
                else if (inputArray[2] == "clone")
                {
                    worldData.chagePowTypes(Powerup.PowerupType.CLONE);
                    return "Setting powerups to clone.";
                }
            }
            #endregion
            #region Team
            else if (txt2 == "score add")
            {
                if (inputArray[2] == "red")
                {
                    worldData.scoreRed += 1;
                    txtRed.text = worldData.scoreRed + "";
                    return "Adding 1 to red's score.";
                }
                else if (inputArray[2] == "blue")
                {
                    worldData.scoreBlue += 1;
                    txtBlue.text = worldData.scoreBlue + "";
                    return "Adding 1 to blue's score.";
                }
            }
            else if (txt2 == "score remove")
            {
                if (inputArray[2] == "red")
                {
                    worldData.scoreRed -= 1;
                    txtRed.text = worldData.scoreRed + "";
                    return "Removing 1 from red's score.";
                }
                else if (inputArray[2] == "blue")
                {
                    worldData.scoreBlue -= 1;
                    txtBlue.text = worldData.scoreBlue + "";
                    return "Removing 1 from blue's score.";
                }
            }
            #endregion
            #region Match
            else if (txt2 == "set time")
            {
                float x;
                if (float.TryParse(inputArray[2], out x) == true)
                {
                    worldData.time.timeLeft = float.Parse(inputArray[2]);
                    return "Setting the time to " + x + " seconds.";
                }
                else if (inputArray[2] == "overtime")
                {
                    if (worldData.scoreBlue > worldData.scoreRed)
                    {
                        worldData.scoreRed = worldData.scoreBlue;
                        txtRed.text = worldData.scoreRed + "";
                    }
                    else if (worldData.scoreBlue < worldData.scoreRed)
                    {
                        worldData.scoreBlue = worldData.scoreRed;
                        txtBlue.text = worldData.scoreBlue + "";
                    }
                    worldData.time.timeLeft = 0.0f;
                    return "Putting the game into overtime.";
                }
            }
            else if(txt2 == "add time")
            {
                float x;
                if(float.TryParse(inputArray[2], out x) == true)
                {
                    worldData.time.timeLeft += float.Parse(inputArray[2]);
                    return "Adding " + inputArray[2] + " seconds to the game.";
                }
            }
            else if(txt2 == "minus time")
            {
                float x;
                if(float.TryParse(inputArray[2], out x) == true)
                {
                    worldData.time.timeLeft -= float.Parse(inputArray[2]);
                    return "Subtracting " + inputArray[2] + " seconds from the game.";
                }
            }
            #endregion
            #region Camera
            else if (txt2 == "toggle camera")
            {
                if (inputArray[2] == "zoom")
                {
                    camera.zoom = !camera.zoom;
                    
                    return "Zooming the camera.";
                }
            }
            #endregion

        }
        else if (inputArray.Length == 2)
        {
            #region Players, length 2

            if (inputArray[0] == "kill")
            {
                if (worldData.players.Length != 4)
                {
                    if (inputArray[1] == "p3" || inputArray[1] == "p4")
                    {
                        return "Sorry, " + inputArray[1] + " is not active";
                    }
                    else
                    {
                        if (inputArray[1] == "p1")
                        {
                            pc[0].gameObject.GetComponent<PlayerSpawn>().Spawn();
                        }
                        else if (inputArray[1] == "p2")
                        {
                            pc[1].gameObject.GetComponent<PlayerSpawn>().Spawn();
                        }
                        return "Killing " + inputArray[1];
                    }
                }
                else
                {
                    if (inputArray[1] == "p1")
                    {
                        pc[0].gameObject.GetComponent<PlayerSpawn>().Spawn();
                    }
                    else if (inputArray[1] == "p2")
                    {
                        pc[1].gameObject.GetComponent<PlayerSpawn>().Spawn();
                    }
                    else if (inputArray[1] == "p3")
                    {
                        pc[2].gameObject.GetComponent<PlayerSpawn>().Spawn();
                    }
                    else if (inputArray[1] == "p4")
                    {
                        pc[3].gameObject.GetComponent<PlayerSpawn>().Spawn();
                    }
                    return "Killing " + inputArray[1];
                }
            }
            else if(inputArray[0] == "toggle")
            {
                if(inputArray[1] == "demo")
                {
                    worldData.isDemoMode = !worldData.isDemoMode;
                    worldData.SetDemoMode(worldData.isDemoMode);
                    return "Toggling demo mode: " + worldData.isDemoMode.ToString();
                }
            }
            #endregion
            #region Ball
            else if (inputArray[0] == "respawn" && inputArray[1] == "ball")
            {
                GameObject ball = GameObject.FindGameObjectWithTag("Ball");
                Respawn ballRespawn = ball.GetComponent<Respawn>();
                ballRespawn.Spawn();
                return "Respawning the ball.";
            }
            #endregion
            
        }
        return "Command not recoginzed.";
    }
}
