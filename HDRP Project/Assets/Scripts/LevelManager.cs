using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using System.Text.RegularExpressions;

public class LevelManager : MonoBehaviour
{
    public static GameObject PlayerObject;
    public static List<GameObject> LevelObjects = new List<GameObject>();
    public static LevelType CurrentLevel;

    private static VehicleState _playerState;
    public static VehicleState PlayerState
    {
        get
        {
            if (PlayerObject == null || !PlayerObject.activeSelf) return null;
            else if (_playerState == null)
            {
                _playerState = PlayerObject.GetComponent<VehicleState>();
            }
            return _playerState;
        }
    }
    private Vector3 cameraPosition;
    private Quaternion cameraRotation;

    public GameObject Camera;
    public GameObject Falcon9;
    public GameObject RocketUI;
    public GameObject MainMenuUI;
    public TextMeshProUGUI LevelText;

    private void Start()
    {
        cameraPosition = Camera.transform.position;
        cameraRotation = Camera.transform.rotation;
        StartLevel(LevelType.MainMenu);
    }

    private void Update()
    {
        if (PlayerObject == null && CurrentLevel != LevelType.MainMenu)
        {
            ClearLevel();
            StartLevel(LevelType.MainMenu);
        }
    }

    public void StartNextLevel()
    {
        ClearLevel();
        if (Enum.IsDefined(typeof(LevelType), CurrentLevel + 1))
            StartLevel(CurrentLevel + 1);
        else
            StartLevel(LevelType.MainMenu);
    }

    private void StartLevel(LevelType level)
    {
        switch (level)
        {
            case LevelType.MainMenu:
                var landingFalcon = SpawnFalcon9(position: new Vector3(-350, 250, 100)
                                               , rotation: Quaternion.identity
                                               , velocity: new Vector3(0, -45, 0)
                                               , angularVelocity: new Vector3(0, 0, 0)
                                               , isPlayer: false
                                               , isAI: true
                                               , legsDeployed: false
                                               , finsDeployed: true);
                landingFalcon.GetComponent<VehicleState>().Throttle = 0.38f;
                LevelObjects.Add(landingFalcon);
                LevelObjects.Add(SpawnFalcon9(position: new Vector3(-350, 122, -50)
                                            , rotation: Quaternion.identity
                                            , velocity: new Vector3(0, 0, 0)
                                            , angularVelocity: new Vector3(0, 0, 0)
                                            , isPlayer: false
                                            , legsDeployed: true
                                            , finsDeployed: true));
                break;
            case LevelType.Level1:
                PlayerObject = SpawnFalcon9(position: new Vector3(-350, 2500, 50)
                                          , rotation: Quaternion.identity
                                          , velocity: new Vector3(0, -150, 0)
                                          , angularVelocity: new Vector3(0, 0, 0)
                                          , isPlayer: true
                                          , legsDeployed: false
                                          , finsDeployed: true);

                LevelObjects.Add(SpawnFalcon9(position: new Vector3(-350, 125, -50)
                                            , rotation: Quaternion.identity
                                            , velocity: new Vector3(0, 0, 0)
                                            , angularVelocity: new Vector3(0, 0, 0)
                                            , isPlayer: false
                                            , legsDeployed: true
                                            , finsDeployed: true));
                break;
            default:
                throw new NotImplementedException($"Geen informatie bekend over level {level.ToString()}");
        }

        if (level == LevelType.MainMenu)
        {
            Camera.transform.position = cameraPosition;
            Camera.transform.rotation = cameraRotation;
            if (!MainMenuUI.activeSelf) MainMenuUI.SetActive(true);
            if (RocketUI.activeSelf) RocketUI.SetActive(false);
        }
        else
        {
            if (MainMenuUI.activeSelf) MainMenuUI.SetActive(false);
            if (!RocketUI.activeSelf) RocketUI.SetActive(true);
        }

        Debug.Log("Starting level: " + level.ToString());
        CurrentLevel = level;
        if (LevelText != null) LevelText.text = Regex.Replace(level.ToString(), "([a-z])([A-Z0-9])" , "$1 $2");
    }

    private void ClearLevel()
    {
        if (PlayerObject != null) Destroy(PlayerObject);
        for (int i = LevelObjects.Count - 1; i >= 0; i--)
        {
            Destroy(LevelObjects[i]);
            LevelObjects.RemoveAt(i);
        }
    }

    private GameObject SpawnFalcon9(Vector3 position
                                  , Quaternion rotation
                                  , Vector3 velocity = new Vector3()
                                  , Vector3 angularVelocity = new Vector3()
                                  , bool isPlayer = false
                                  , bool isAI = false
                                  , bool legsDeployed = false
                                  , bool finsDeployed = true)
    {
        GameObject tmpObj = Instantiate(Falcon9, position, rotation);

        if (velocity.magnitude > 0 || angularVelocity.magnitude > 0)
        {
            var tmpRigidbody = tmpObj.GetComponent<Rigidbody>();
            tmpRigidbody.linearVelocity = velocity;
            tmpRigidbody.angularVelocity = angularVelocity;
        }
        
        var state = tmpObj.GetComponent<VehicleState>();
        state.isPlayer = isPlayer;
        state.isAI = isAI;
        state.LegsDeployed = legsDeployed;
        state.FinsDeployed = finsDeployed;

        return tmpObj;
    }
}

public enum LevelType
{
    MainMenu = 0,
    Level1 = 1,
}
