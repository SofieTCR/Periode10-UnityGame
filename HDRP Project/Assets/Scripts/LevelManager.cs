using UnityEngine;
using System;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static GameObject PlayerObject;
    public static List<GameObject> LevelObjects = new List<GameObject>();
    public static LevelState CurrentLevel = LevelState.MainMenu;
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

    public GameObject Falcon9;

    private void Start()
    {
        CurrentLevel = LevelState.Level1;
    }

    private void Update()
    {
        if (PlayerObject == null && CurrentLevel != LevelState.MainMenu)
        {
            ClearLevel();
            StartLevel(CurrentLevel);
        }
    }

    private void StartLevel(LevelState level)
    {
        switch (level)
        {
            case LevelState.Level1:
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
        Debug.Log("Starting level: " + level.ToString());
    }

    private void ClearLevel()
    {
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
        state.LegsDeployed = legsDeployed;
        state.FinsDeployed = finsDeployed;

        return tmpObj;
    }
}

public enum LevelState
{
    MainMenu,
    Level1,
}
