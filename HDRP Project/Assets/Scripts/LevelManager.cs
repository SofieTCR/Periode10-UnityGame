using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using System.Text.RegularExpressions;
using static UnityEngine.Rendering.DebugUI;

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
            if (PlayerObject == null) return null;
            else if (_playerState == null)
            {
                _playerState = PlayerObject.GetComponent<VehicleState>();
            }
            return _playerState;
        }
    }
    public static bool PlayerObjectActive => PlayerObject?.activeSelf ?? false;


    private Vector3 cameraPosition;
    private Quaternion cameraRotation;
    private int Score = 0;

    public GameObject Camera;
    public GameObject Falcon9;
    public GameObject RocketUI;
    public GameObject OutcomeUI;
    public GameObject MainMenuUI;
    public TextMeshProUGUI LevelText;

    private TextMeshProUGUI OutcomeText;
    private TextMeshProUGUI OutcomeDist;
    private TextMeshProUGUI OutcomeVel;
    private TextMeshProUGUI OutcomeAngle;
    private TextMeshProUGUI OutcomeScore;

    private void Start()
    {
        cameraPosition = Camera.transform.position;
        cameraRotation = Camera.transform.rotation;
        StartLevel(LevelType.MainMenu);
    }

    private void Update()
    {
        
        if (PlayerObject != null && !OutcomeUI.activeSelf && PlayerState.IsGrounded && Time.time >= PlayerState.TimeGrounded + GameSettings.TimeBetweenLevels / 4)
        {
            PopulateOutcomeUI();
            OutcomeUI.SetActive(true);
        }
        else if (PlayerObject != null && PlayerState.IsStable && Time.time >= PlayerState.TimeGrounded + GameSettings.TimeBetweenLevels)
        {
            StartNextLevel();
        }
        else if (PlayerObject == null && CurrentLevel != LevelType.MainMenu)
        {
            // TODO: save score
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
                LevelObjects.Add(SpawnFalcon9(position: new Vector3(-350, 250, 100)
                                            , rotation: Quaternion.identity
                                            , velocity: new Vector3(0, -45, 0)
                                            , angularVelocity: new Vector3(0, 0, 0)
                                            , isPlayer: false
                                            , isAI: true
                                            , legsDeployed: false
                                            , finsDeployed: true));
                LevelObjects.Add(SpawnFalcon9(position: new Vector3(-350, 120, -50)
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

                LevelObjects.Add(SpawnFalcon9(position: new Vector3(-350, 120, -50)
                                            , rotation: Quaternion.identity
                                            , velocity: new Vector3(0, 0, 0)
                                            , angularVelocity: new Vector3(0, 0, 0)
                                            , isPlayer: false
                                            , legsDeployed: true
                                            , finsDeployed: true));
                break;
            case LevelType.Level2:
                PlayerObject = SpawnFalcon9(position: new Vector3(-350, 2500, -90)
                                          , rotation: Quaternion.Euler(-7.5f, 0, 0)
                                          , velocity: new Vector3(0, -150, 20)
                                          , angularVelocity: new Vector3(0, 0, 0)
                                          , isPlayer: true
                                          , legsDeployed: false
                                          , finsDeployed: true);

                LevelObjects.Add(SpawnFalcon9(position: new Vector3(-350, 1650, -50)
                                            , rotation: Quaternion.identity
                                            , velocity: new Vector3(0, -150, 0)
                                            , angularVelocity: new Vector3(0, 0, 0)
                                            , isPlayer: false
                                            , isAI: true
                                            , legsDeployed: false
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
            if (OutcomeUI.activeSelf) OutcomeUI.SetActive(false);
            Score = 0;
        }
        else
        {
            if (MainMenuUI.activeSelf) MainMenuUI.SetActive(false);
            if (!RocketUI.activeSelf) RocketUI.SetActive(true);
            if (OutcomeUI.activeSelf) OutcomeUI.SetActive(false);
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

    private void PopulateOutcomeUI()
    {
        if (OutcomeText == null) OutcomeText = OutcomeUI.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        OutcomeText.text = PlayerObjectActive ? "Succesful Landing" : "Catastrophic Failure";
        OutcomeText.color = PlayerObjectActive ? Color.white : Color.red;

        if (OutcomeDist == null) OutcomeDist = OutcomeUI.transform.GetChild(0).GetChild(3).GetComponent<TextMeshProUGUI>();
        OutcomeDist.text = $"{PlayerState.DistanceGrounded:F1}m";

        if (OutcomeVel == null) OutcomeVel = OutcomeUI.transform.GetChild(0).GetChild(5).GetComponent<TextMeshProUGUI>();
        OutcomeVel.text = $"{PlayerState.VelocityGrounded:F2}m/s";

        if (OutcomeAngle == null) OutcomeAngle = OutcomeUI.transform.GetChild(0).GetChild(7).GetComponent<TextMeshProUGUI>();
        OutcomeAngle.text = $"{PlayerState.AngleGrounded:F2}";

        if (PlayerObjectActive)
            Score += CalculateScore();
        if (OutcomeScore == null) OutcomeScore = OutcomeUI.transform.GetChild(0).GetChild(11).GetComponent<TextMeshProUGUI>();
        OutcomeScore.text = Score.ToString();
    }
    private int CalculateScore()
    {
        // Define scoring parameters
        const int maxDistancePoints = 15;
        const float maxDistance = 5.0f; // Distance in meters

        const int maxVelocityPoints = 10;
        const float maxVelocity = 6.0f; // Velocity in m/s
        const float idealVelocity = 2.0f; // Velocity for max score

        const int maxAnglePoints = 5;
        const float maxAngle = 5.0f; // Angle in degrees
        const float idealAngle = 2.0f; // Angle for max score
        int tmpScore = 0;

        // Distance scoring
        if (PlayerState.DistanceGrounded != null)
        {
            float distance = PlayerState.DistanceGrounded.Value;
            if (distance <= maxDistance)
            {
                float distanceFactor = 1.0f - (distance / maxDistance);
                tmpScore += (int) (distanceFactor * maxDistancePoints);
            }
        }

        // Velocity scoring
        if (PlayerState.VelocityGrounded != null)
        {
            float velocity = PlayerState.VelocityGrounded.Value;
            if (velocity <= maxVelocity)
            {
                float velocityFactor = Math.Clamp(1.0f - ((velocity - idealVelocity) / (maxVelocity - idealVelocity)), 0.0f, 1.0f);
                tmpScore += (int) (velocityFactor * maxVelocityPoints);
            }
        }

        // Angle scoring
        if (PlayerState.AngleGrounded != null)
        {
            float angle = PlayerState.AngleGrounded.Value;
            if (angle <= maxAngle)
            {
                float angleFactor = Math.Clamp(1.0f - ((angle - idealAngle) / (maxAngle - idealAngle)), 0.0f, 1.0f);
                tmpScore += (int) (angleFactor * maxAnglePoints);
            }
        }

        return Math.Max(0, tmpScore);
    }

}

public enum LevelType
{
    MainMenu = 0,
    Level1 = 1,
    Level2 = 2,
}
