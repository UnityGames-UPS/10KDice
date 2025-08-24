using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GameManager : MonoBehaviour
{

    [SerializeField]
    int marblesToSpawn = 2;
    [Header("Buttons")]
    [SerializeField]
    private Button TBetPlus_Button;
    [SerializeField]
    private Button TBetMinus_Button;
    [SerializeField]
    private Button sendBet_Button;
    [SerializeField]
    private Button highRisk_Button;
    [SerializeField]
    private Button mediumRisk_Button;
    [SerializeField]
    private Button lowRisk_Button;
    [SerializeField]
    private Button manualBetButton;
    [SerializeField]
    private Button autoBet_Button;
    [SerializeField]
    private Button autoBet_Stop;

    private double currentTotalBet = 0;
    private double currentBalance;
    internal int BetCounter;
    internal int BallCounter;
    [SerializeField]
    List<TextMeshProUGUI> riskContainers = new List<TextMeshProUGUI>();
    [SerializeField]
    Color highlightButtonCol;
    [SerializeField]
    Color hideButtonCol;
    [SerializeField]
    List<Vector3> cubeRotaionPoints = new List<Vector3>();
    [SerializeField]
    private TMP_Text TotalBet_text;
    [SerializeField]
    private TMP_Text balance_text;
    [SerializeField]
    private TMP_Text win_text;
    [SerializeField]
    private TMP_Text autoBetCount_text;
    [SerializeField]
    TMP_InputField autoBetField;
    internal int autoBetTotalCount, autoBetCurrentCount;
    [SerializeField]
    internal float autoBetFrequency = 0.5f;
    bool isAutoBetPlaying;
    bool autoBetInstanceDone;
    internal int totalMarbleInAction;
    Coroutine autoBetCoroutine;
    [SerializeField]
    AudioManager audioManager;
    [SerializeField]
    SocketIOManager socketIoManager;
    [SerializeField]
    internal UiManager uiManager;
    internal gameType _gameType = gameType.TWELVE;
    int currentLines = 20;
    int riskfactor = 0;
    bool isAutoBet, isAutobetInstanceDone, autobetRunning;
    [SerializeField] GameObject autoBetpanel;
    [SerializeField] GameObject touchDisable;
    public enum marbleType
    {
        RED,
        YELLOW,
        GREEN
    }

    public enum gameType
    {
        TWELVE,
        SIXTEEN
    }

    #region 10kDice

    [SerializeField] private Slider rotationSlider;
    [SerializeField] private RectTransform deadzoneTransform;
    [SerializeField] private Image bluecircleIamge;

    [SerializeField] private Transform circleGameObject;
    private Tween rotationTween;

    [SerializeField] private TMP_Text bluepercentage_Text;
    [SerializeField] private TMP_Text purplepercentage_Text;



    private void Start()
    {
        if (rotationSlider != null)
        {
            rotationSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    private void OnSliderValueChanged(float value)
    {
        // Map slider [0,1] → rotation [0,-360]
        float rotationZ = Mathf.Lerp(0f, -360f, value);
        if (deadzoneTransform != null)
        {
            deadzoneTransform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
        }
        bluecircleIamge.fillAmount = value;
        SetPercentageValue(value);
    }

    void SetPercentageValue(double slidervalue)
    {
        bluepercentage_Text.text = (slidervalue * 100).ToString("f2") + "%";
        purplepercentage_Text.text = (100-(slidervalue * 100)).ToString("f2") + "%";

    }




    public double CalculateMultiplier(double selectedX, double targetRTP, double houseEdge, int range)
    {
        Debug.Log($"selectedX is {selectedX}, targetRTP is: {targetRTP}, houseEdge is: {houseEdge}, range is: {range}");

        double multiplier = 0;

        if (range == 0)
        {
            multiplier = (100 / selectedX) * (targetRTP / 100);
        }
        else if (range == 1)
        {
            multiplier = (100 / (100 - (selectedX + houseEdge))) * (targetRTP / 100);
        }
        else
        {
            Debug.Log("Invalid selected range");
        }
        return multiplier;


    }



    // Start infinite rotation
    [SerializeField] private float rotationDuration = 1f; // time for one -360° rotation
    [SerializeField] private float stopEaseDuration = 5f; // time to ease into target

    private Tween spinTween;
    private Sequence stopSequence;



    public void StartRotation()
    {
        if (circleGameObject == null) return;

        spinTween?.Kill();
        stopSequence?.Kill();

        // Keep Y = 180 locked, only rotate Z
        circleGameObject.localEulerAngles = new Vector3(0, 180, circleGameObject.localEulerAngles.z);

        spinTween = circleGameObject.DOLocalRotate(
            new Vector3(0, 0, 360f),          // target (Z rotates)
            rotationDuration,                   // one full turn
            RotateMode.FastBeyond360
        )
        .SetRelative(true)   // apply rotation relative to current
        .SetEase(Ease.Linear)
        .SetLoops(-1);       // infinite
    }

    public void StopRotation(float targetZ)
    {
        if (circleGameObject == null) return;

        spinTween?.Kill();
        stopSequence?.Kill();

        stopSequence = DOTween.Sequence();

        float currentZ = circleGameObject.localEulerAngles.z;

        // --- 1) Remainder to finish this circle ---
        float remainder = 360f - (currentZ % 360f);
        float finishZ = currentZ + remainder; // absolute forward angle

        // --- 2) Tween remainder of spin ---
        stopSequence.Append(circleGameObject.DOLocalRotate(
            new Vector3(0, 180, finishZ),
            rotationDuration * (remainder / 360f),
            RotateMode.FastBeyond360   // ensures forward movement beyond 360
        ).SetEase(Ease.Linear));

        // --- 3) Then tween to targetZ, but in the **next circle**
        float finalZ = finishZ + targetZ; // offset into next rotation cycle

        stopSequence.Append(circleGameObject.DOLocalRotate(
            new Vector3(0, 180, finalZ),
            stopEaseDuration,
            RotateMode.FastBeyond360
        ).SetEase(Ease.OutCubic));
    }




    #endregion

    // private void Start()
    // {
    //     if (TBetPlus_Button) TBetPlus_Button.onClick.RemoveAllListeners();
    //     if (TBetPlus_Button) TBetPlus_Button.onClick.AddListener(delegate { ChangeBet(true); });

    //     if (TBetMinus_Button) TBetMinus_Button.onClick.RemoveAllListeners();
    //     if (TBetMinus_Button) TBetMinus_Button.onClick.AddListener(delegate { ChangeBet(false); });

    //     if (sendBet_Button) sendBet_Button.onClick.RemoveAllListeners();
    //     if (sendBet_Button) sendBet_Button.onClick.AddListener(delegate { sendBetData(); });

    //     if (autoBet_Button) autoBet_Button.onClick.RemoveAllListeners();
    //     if (autoBet_Button) autoBet_Button.onClick.AddListener(delegate { gameMode(true); });

    //     if (manualBetButton) manualBetButton.onClick.RemoveAllListeners();
    //     if (manualBetButton) manualBetButton.onClick.AddListener(delegate { gameMode(false); });

    //     if (autoBet_Stop) autoBet_Stop.onClick.RemoveAllListeners();
    //     if (autoBet_Stop) autoBet_Stop.onClick.AddListener(delegate { stopAutoBet(); });

    //     if (lowRisk_Button) lowRisk_Button.onClick.RemoveAllListeners();
    //     if (lowRisk_Button) lowRisk_Button.onClick.AddListener(delegate { changeRiskFactor("low"); });

    //     if (mediumRisk_Button) mediumRisk_Button.onClick.RemoveAllListeners();
    //     if (mediumRisk_Button) mediumRisk_Button.onClick.AddListener(delegate { changeRiskFactor("medium"); });

    //     if (highRisk_Button) highRisk_Button.onClick.RemoveAllListeners();
    //     if (highRisk_Button) highRisk_Button.onClick.AddListener(delegate { changeRiskFactor("high"); });


    // }




    internal void setInitialUI()
    {
        currentBalance = socketIoManager.playerdata.Balance;
        balance_text.text = socketIoManager.playerdata.Balance.ToString("f3");
        currentTotalBet = socketIoManager.initialData.Bets[0];
        if (TotalBet_text) TotalBet_text.text = (socketIoManager.initialData.Bets[BetCounter]).ToString("f2");
        currentTotalBet = socketIoManager.initialData.Bets[BetCounter];
        changeRiskFactor("low");


    }




    private void sendBetData()
    {
        toggleUI(false);
        if (!isAutoBet)
        {
            StartCoroutine(accumulateResult());

        }
        else
        {
            isAutoBetPlaying = true;
            autoBetCurrentCount = autoBetTotalCount;
            autoBetCoroutine = StartCoroutine(startAutoBet());
        }

    }


    IEnumerator accumulateResult()
    {

        if (currentBalance < currentTotalBet)
        {
            lowBalance();
            yield break;
        }
        else
        {
            touchDisable.SetActive(true);
            updateBalance(currentTotalBet, false);
            //socketIoManager.AccumulateResult(socketIoManager.initialData.Bets[BetCounter], rowDropDown.value, riskDropDown.value);
            socketIoManager.AccumulateResult(BetCounter, currentLines, 1, riskfactor);
            yield return new WaitUntil(() => socketIoManager.isResultdone);
            List<int> cubeSides = socketIoManager.ConvertListListIntToListint(socketIoManager.resultData.resultSymbolMatrix);
            win_text.text = socketIoManager.playerdata.currentWining.ToString("f2");
            balance_text.text = socketIoManager.playerdata.Balance.ToString("f2");

            yield return new WaitForSeconds(1f);
            isAutobetInstanceDone = true;
            if (isAutoBetPlaying)
            {
                touchDisable.SetActive(false);
            }

        }

    }



    void lowBalance()
    {
        toggleUI(true);
        if (isAutoBetPlaying)
        {
            StopCoroutine(autoBetCoroutine);
            autobetRunning = false;
            autoBet_Stop.gameObject.SetActive(false);
            autoBetCount_text.text = "Stop Auto Bet ";
        }
    }

    internal void updateBalance(double amount, bool add)
    {
        if (add)
        {

            currentBalance += amount;
            balance_text.text = currentBalance.ToString("f3");
        }
        else
        {
            currentBalance -= amount;
            balance_text.text = currentBalance.ToString("f3");

        }
    }

    internal void checkForFallingMarbles()
    {
        if (totalMarbleInAction == 0)
        {
            toggleUI(true);
        }
    }


    void gameMode(bool autoBet)
    {
        isAutoBet = autoBet;
        if (isAutoBet)
        {
            autoBetpanel.SetActive(true);
            manualBetButton.image.color = hideButtonCol;
            autoBet_Button.image.color = highlightButtonCol;
        }
        else
        {
            autoBetpanel.SetActive(false);
            manualBetButton.image.color = highlightButtonCol;
            autoBet_Button.image.color = hideButtonCol;
        }
    }


    private void ChangeBet(bool IncDec)
    {
        Debug.Log("changeBetRan");
        if (IncDec)
        {
            BetCounter++;
            if (BetCounter >= socketIoManager.initialData.Bets.Count)
            {
                BetCounter = 0;
            }
        }
        else
        {
            BetCounter--;
            if (BetCounter < 0)
            {
                BetCounter = socketIoManager.initialData.Bets.Count - 1;
            }
        }
        if (TotalBet_text) TotalBet_text.text = (socketIoManager.initialData.Bets[BetCounter]).ToString("f2");
        currentTotalBet = socketIoManager.initialData.Bets[BetCounter];

    }




    private void changeRiskFactor(string risk)
    {

        Debug.Log(risk);
        for (int i = 0; i < riskContainers.Count; i++)
        {
            riskContainers[i].transform.parent.gameObject.SetActive(false);
        }
        switch (risk)
        {
            case "low":
                {
                    riskfactor = 0;
                    int containerMultiplier = 3;
                    for (int i = 0; i < socketIoManager.initialData.multiplier[0].Count; i++)
                    {
                        riskContainers[i].text = containerMultiplier + "\n" + socketIoManager.initialData.multiplier[0][i].ToString() + "X";
                        riskContainers[i].transform.parent.gameObject.SetActive(true);
                        containerMultiplier++;
                    }
                    mediumRisk_Button.image.color = hideButtonCol;
                    highRisk_Button.image.color = hideButtonCol;
                    lowRisk_Button.image.color = highlightButtonCol;
                    break;

                }
            case "medium":
                {
                    riskfactor = 1;
                    int containerMultiplier = 4;
                    for (int i = 0; i < socketIoManager.initialData.multiplier[1].Count; i++)
                    {
                        riskContainers[i].text = containerMultiplier + "\n" + socketIoManager.initialData.multiplier[1][i].ToString() + "X";
                        riskContainers[i].transform.parent.gameObject.SetActive(true);
                        containerMultiplier++;
                    }
                    mediumRisk_Button.image.color = highlightButtonCol;
                    highRisk_Button.image.color = hideButtonCol;
                    lowRisk_Button.image.color = hideButtonCol;
                    break;
                }
            case "high":
                {
                    riskfactor = 2;
                    int containerMultiplier = 5;
                    for (int i = 0; i < socketIoManager.initialData.multiplier[2].Count; i++)
                    {
                        riskContainers[i].text = containerMultiplier + "\n" + socketIoManager.initialData.multiplier[2][i].ToString() + "X";
                        riskContainers[i].transform.parent.gameObject.SetActive(true);
                        containerMultiplier++;
                    }
                    mediumRisk_Button.image.color = hideButtonCol;
                    highRisk_Button.image.color = highlightButtonCol;
                    lowRisk_Button.image.color = hideButtonCol;
                    break;
                }
        }
    }



    IEnumerator startAutoBet()
    {

        autoBet_Stop.gameObject.SetActive(true);
        Debug.Log(autoBetCurrentCount);
        autobetRunning = true;
        for (int i = 0; i < autoBetTotalCount; i++)
        {
            autoBetInstanceDone = false;
            StartCoroutine(accumulateResult());
            yield return new WaitUntil(() => isAutobetInstanceDone);
            yield return new WaitForSeconds(2f);

            autoBetCount_text.text = "Stop Auto Bet " + (autoBetTotalCount - i).ToString();
        }
        autobetRunning = false;
        touchDisable.SetActive(false);
        isAutoBetPlaying = false;
        autoBet_Stop.gameObject.SetActive(false);




    }

    void stopAutoBet()
    {


        autoBet_Stop.gameObject.SetActive(false);
        StopCoroutine(autoBetCoroutine);
    }


    private void toggleUI(bool toggle)
    {

        TBetPlus_Button.interactable = toggle;
        TBetMinus_Button.interactable = toggle;


    }


}
