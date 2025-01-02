using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using System.Text.RegularExpressions;
using System.Collections;

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
    public static bool PlayerObjectActive => PlayerObject != null ? PlayerObject.activeSelf : false;


    private Vector3 cameraPosition;
    private Quaternion cameraRotation;
    private int Score = 0;

    public GameObject Camera;
    public GameObject Falcon9;
    public List<GameObject> LandingZones = new List<GameObject>();
    public GameObject RocketUI;
    public GameObject OutcomeUI;
    public GameObject MainMenuUI;
    public TextMeshProUGUI LevelText;

    private TextMeshProUGUI OutcomeText;
    private TextMeshProUGUI OutcomeDist;
    private TextMeshProUGUI OutcomeVel;
    private TextMeshProUGUI OutcomeAngle;
    private TextMeshProUGUI OutcomeAngleSign;
    private TextMeshProUGUI OutcomeScore;
    private TextMeshProUGUI MenuHighScore;
    private Coroutine StartLevelRoutine;

    private void Start()
    {
        cameraPosition = Camera.transform.position;
        cameraRotation = Camera.transform.rotation;
        StartLevelRoutine = StartCoroutine(StartLevel(LevelType.MainMenu));
    }

    private void Update()
    {
        if (PlayerObject != null && !OutcomeUI.activeSelf && (!PlayerObjectActive || PlayerState.IsStable) && Time.time >= PlayerState.TimeGrounded + GameSettings.TimeBetweenLevels / 4)
        {
            PopulateOutcomeUI();
            OutcomeUI.SetActive(true);
        }
        else if (PlayerObject != null && PlayerState.IsStable && Time.time >= PlayerState.TimeGrounded + GameSettings.TimeBetweenLevels)
        {
            StartNextLevel();
        }
        else if (PlayerObject == null && CurrentLevel != LevelType.MainMenu && StartLevelRoutine == null)
        {
            ClearLevel();
            StartLevelRoutine = StartCoroutine(StartLevel(LevelType.MainMenu));
        }
    }
    [ContextMenu("Next Level")]
    public void StartNextLevel()
    {
        ClearLevel();
        if (Enum.IsDefined(typeof(LevelType), CurrentLevel + 1))
            StartLevelRoutine = StartCoroutine(StartLevel(CurrentLevel + 1));
        else
            StartLevelRoutine = StartCoroutine(StartLevel(LevelType.MainMenu));
    }

    private IEnumerator StartLevel(LevelType level)
    {
        yield return new WaitForEndOfFrame();
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
                                          , targetLZ: LandingZones[2]
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
                                          , targetLZ: LandingZones[0]
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
            SetHighScore(Score);
            Score = 0;
            GetHighScore();
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
        StartLevelRoutine = null;
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
                                  , GameObject targetLZ = null
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
        state.TargetLandingZone = targetLZ;

        return tmpObj;
    }

    private void PopulateOutcomeUI()
    {
        var succesfulLanding = PlayerObjectActive;
        if (OutcomeText == null) OutcomeText = OutcomeUI.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        OutcomeText.text = succesfulLanding ? "Succesful Landing" : "Catastrophic Failure";
        OutcomeText.color = succesfulLanding ? Color.white : Color.red;

        if (OutcomeDist == null) OutcomeDist = OutcomeUI.transform.GetChild(0).GetChild(3).GetComponent<TextMeshProUGUI>();
        OutcomeDist.text = $"{PlayerState.DistanceGrounded:F1}m";
        if (succesfulLanding)
        {
            var colorValue = CalculatePoints(value: PlayerState.DistanceGrounded
                                           , availablePoints: 2
                                           , valueForMaxPoints: GameSettings.idealDistance
                                           , valueForNoPoints: GameSettings.maxDistance);
            OutcomeDist.color = new Color(Mathf.Min(2 - colorValue, 1), Mathf.Min(colorValue, 1), 0, 1);
        }
        else
        {
            OutcomeDist.color = Color.white;
        }

        if (OutcomeVel == null) OutcomeVel = OutcomeUI.transform.GetChild(0).GetChild(5).GetComponent<TextMeshProUGUI>();
        OutcomeVel.text = $"{PlayerState.VelocityGrounded:F2}m/s";
        if (succesfulLanding)
        {
            var colorValue = CalculatePoints(value: PlayerState.VelocityGrounded
                                           , availablePoints: 2
                                           , valueForMaxPoints: GameSettings.idealVelocity
                                           , valueForNoPoints: GameSettings.maxVelocity);
            OutcomeVel.color = new Color(Mathf.Min(2 - colorValue, 1), Mathf.Min(colorValue, 1), 0, 1);
        }
        else
        {
            OutcomeVel.color = Color.white;
        }

        if (OutcomeAngle == null) OutcomeAngle = OutcomeUI.transform.GetChild(0).GetChild(7).GetComponent<TextMeshProUGUI>();
        if (OutcomeAngleSign == null) OutcomeAngleSign = OutcomeUI.transform.GetChild(0).GetChild(8).GetComponent<TextMeshProUGUI>();
        OutcomeAngle.text = $"{PlayerState.AngleGrounded:F2}";
        if (succesfulLanding)
        {
            var colorValue = CalculatePoints(value: PlayerState.AngleGrounded
                                           , availablePoints: 2
                                           , valueForMaxPoints: GameSettings.idealAngle
                                           , valueForNoPoints: GameSettings.maxAngle);
            var color = new Color(Mathf.Min(2 - colorValue, 1), Mathf.Min(colorValue, 1), 0, 1);
            OutcomeAngle.color = color;
            OutcomeAngleSign.color = color;
        }
        else
        {
            OutcomeAngle.color = Color.white;
            OutcomeAngleSign.color = Color.white;
        }

        if (PlayerObjectActive)
            Score += CalculateScore();
        if (OutcomeScore == null) OutcomeScore = OutcomeUI.transform.GetChild(0).GetChild(11).GetComponent<TextMeshProUGUI>();
        OutcomeScore.text = Score.ToString();
    }
    private int CalculateScore()
    {
        int tmpScore = 0;

        // Distance scoring
        tmpScore += Mathf.RoundToInt(CalculatePoints(value: PlayerState.DistanceGrounded
                                                   , availablePoints: GameSettings.maxDistancePoints
                                                   , valueForMaxPoints: GameSettings.idealDistance
                                                   , valueForNoPoints: GameSettings.maxDistance));

        // Velocity scoring
        tmpScore += Mathf.RoundToInt(CalculatePoints(value: PlayerState.VelocityGrounded
                                                   , availablePoints: GameSettings.maxVelocityPoints
                                                   , valueForMaxPoints: GameSettings.idealVelocity
                                                   , valueForNoPoints: GameSettings.maxVelocity));

        // Angle scoring
        tmpScore += Mathf.RoundToInt(CalculatePoints(value: PlayerState.AngleGrounded
                                                   , availablePoints: GameSettings.maxAnglePoints
                                                   , valueForMaxPoints: GameSettings.idealAngle
                                                   , valueForNoPoints: GameSettings.maxAngle));

        return tmpScore;
    }

    private float CalculatePoints(float? value, float availablePoints, float valueForMaxPoints, float valueForNoPoints)
    {
        float tmpScore = 0f;
        if (value != null)
        {
            if (value <= valueForNoPoints)
            {
                float angleFactor = Math.Clamp((float) (1.0f - ((value - valueForMaxPoints) / (valueForNoPoints - valueForMaxPoints))), 0.0f, 1.0f);
                tmpScore += (angleFactor * availablePoints);
            }
        }
        return tmpScore;
    }

    public static void SetHighScore(int score)
    {
        int currentHighScore = PlayerPrefs.GetInt(GameSettings.HighScoreKey, 0);
        if (score > currentHighScore)
        {
            PlayerPrefs.SetInt(GameSettings.HighScoreKey, score);
            PlayerPrefs.Save();
        }
    }

    public void GetHighScore()
    {
        var highScore = PlayerPrefs.GetInt(GameSettings.HighScoreKey, 0);
        if (MenuHighScore == null) MenuHighScore = MainMenuUI.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        MenuHighScore.text = highScore.ToString();
    }
}

public enum LevelType
{
    MainMenu = 0,
    Level1 = 1,
    Level2 = 2,
}
