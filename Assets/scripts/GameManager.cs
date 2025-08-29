using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

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
    [Header("Slider Limits")]
    private const double MinPercent = 0.01;
    private const double MaxPercent = 94.99;

    [Header("UI References")]
    [SerializeField] private RectTransform deadzoneTransform;
    [SerializeField] private Image bluecircleIamge;
    [SerializeField] private Slider rotationSlider;
    [SerializeField] private TMP_Text bluepercentage_Text;
    [SerializeField] private Transform circleGameObject;
    [SerializeField] private TMP_Text bluemultiplier_Text;
    [SerializeField] private TMP_Text purplemultiplier_Text;
    [SerializeField] private TMP_Text bluewin_Text;
    [SerializeField] private TMP_Text purplewin_Text;
    [SerializeField] private TMP_Text purplepercentage_Text;
    [SerializeField] private TMP_Text rotationnumber_Text;
    [SerializeField] private GameObject pressbetui_Object;

    [Header("Buttons")]
    [SerializeField] private Button BlueBet_Button;
    [SerializeField] private Button PurpleBet_Button;
    [SerializeField] private Button plusslider_Button;
    [SerializeField] private Button minusslider_Button;
    // [SerializeField] private Button AutoBet_Button;
    // [SerializeField] private Button StopAutoBet_Button;
    private Tween rotationTween;
    private Tween spinTween;
    private Sequence stopSequence;

    [Header("Auto Bet")]
    [SerializeField] private GameObject offTogle_Object;
    [SerializeField] private GameObject onTogle_Object;
    [SerializeField] private GameObject autoplayText_Object;
    private bool isAutoToggleOn = false;
    [SerializeField] private Button autoBet_Button;
    [SerializeField] private Button autoBet_Stop;
    [SerializeField] private TMP_Text betCount_Text;
    [SerializeField] private Button blueAutoBet_Button;
    [SerializeField] private Button purpleAutoBet_Button;
    [SerializeField] private Button AutoBetCustomize_Button;
    [SerializeField] private List<Sprite> betstop_Sprites;
    [SerializeField] private List<Button> setrounds_Buttons;
    [SerializeField] private Color unselectroundColor;
    [SerializeField] private GameObject SelectplayRounds_Object;
    [SerializeField] private Button closeSelectRounds_Button;
    private bool isautoplayCountSelected = false;
    private bool isautoplayEnable = false;


    [SerializeField] private float rotationDuration = 1f;
    [SerializeField] private float stopEaseDuration = 5f;

    private double Selectedx;
    private double bluemultiplier;
    private double purplemultiplier;
    [SerializeField] private GameObject playanimation_Object;
    [Header("Winnig UI")]
    [SerializeField] private GameObject winningui_Object;
    [SerializeField] private TMP_Text winninguiAmount_Text;
    [SerializeField] private TMP_Text winninguiMultiplier_Text;

    [SerializeField] AudioManager audioManager;
    private void Start()
    {
        BetCounter = 0;
        if (rotationSlider != null)
        {
            rotationSlider.onValueChanged.AddListener(HandleSliderChanged);
        }
        if (TBetPlus_Button) TBetPlus_Button.onClick.RemoveAllListeners();
        if (TBetPlus_Button) TBetPlus_Button.onClick.AddListener(delegate { ChangeBet(true); audioManager.PlayWLAudio("plusminus"); });

        if (TBetMinus_Button) TBetMinus_Button.onClick.RemoveAllListeners();
        if (TBetMinus_Button) TBetMinus_Button.onClick.AddListener(delegate { ChangeBet(false); audioManager.PlayWLAudio("plusminus"); });

        if (BlueBet_Button) BlueBet_Button.onClick.RemoveAllListeners();
        if (BlueBet_Button) BlueBet_Button.onClick.AddListener(delegate { sendBetData(0); audioManager.PlayBetButtonAudio(); });

        if (PurpleBet_Button) PurpleBet_Button.onClick.RemoveAllListeners();
        if (PurpleBet_Button) PurpleBet_Button.onClick.AddListener(delegate { sendBetData(1); audioManager.PlayBetButtonAudio(); });

        if (autoBet_Button) autoBet_Button.onClick.RemoveAllListeners();
        if (autoBet_Button) autoBet_Button.onClick.AddListener(delegate { ToggleAutobet(); audioManager.PlayButtonAudio(); });

        if (blueAutoBet_Button) blueAutoBet_Button.onClick.RemoveAllListeners();
        if (blueAutoBet_Button) blueAutoBet_Button.onClick.AddListener(delegate { sendBetData(0); audioManager.PlayBetButtonAudio(); });

        if (purpleAutoBet_Button) purpleAutoBet_Button.onClick.RemoveAllListeners();
        if (purpleAutoBet_Button) purpleAutoBet_Button.onClick.AddListener(delegate { sendBetData(1); audioManager.PlayBetButtonAudio(); });

        if (AutoBetCustomize_Button) AutoBetCustomize_Button.onClick.RemoveAllListeners();
        if (AutoBetCustomize_Button) AutoBetCustomize_Button.onClick.AddListener(delegate { SelectBetRounds(); audioManager.PlayButtonAudio(); });

        if (closeSelectRounds_Button) closeSelectRounds_Button.onClick.RemoveAllListeners();
        if (closeSelectRounds_Button) closeSelectRounds_Button.onClick.AddListener(delegate { closeSelectBetRounds(); audioManager.PlayButtonAudio(); });



        if (plusslider_Button) plusslider_Button.onClick.RemoveAllListeners();
        if (plusslider_Button) plusslider_Button.onClick.AddListener(delegate { AdjustSlider(true); audioManager.PlayWLAudio("plusminus"); });

        if (minusslider_Button) minusslider_Button.onClick.RemoveAllListeners();
        if (minusslider_Button) minusslider_Button.onClick.AddListener(delegate { AdjustSlider(false); audioManager.PlayWLAudio("plusminus"); });



        // if (autoBet_Button) autoBet_Button.onClick.RemoveAllListeners();
        // if (autoBet_Button) autoBet_Button.onClick.AddListener(delegate { gameMode(true); });

        if (autoBet_Stop) autoBet_Stop.onClick.RemoveAllListeners();
        if (autoBet_Stop) autoBet_Stop.onClick.AddListener(delegate { stopAutoBet(); });
        SetSelectedRounds();


    }


    private void HandleSliderChanged(float sliderValue)
    {
        UpdateRotation(sliderValue);
        UpdateFill(sliderValue);

        double percentage = MapSliderToPercentage(sliderValue);

        UpdatePercentageTexts(percentage);
        UpdateMultiplierTexts(percentage);

    }
    private void UpdateRotation(float sliderValue)
    {
        if (deadzoneTransform == null) return;

        float rotationZ = Mathf.Lerp(0f, -360f, sliderValue);
        deadzoneTransform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
    }

    private void UpdateFill(float sliderValue)
    {
        if (bluecircleIamge != null)
            bluecircleIamge.fillAmount = sliderValue;
    }
    private double MapSliderToPercentage(float sliderValue)
    {
        return MinPercent + (MaxPercent - MinPercent) * sliderValue;
    }

    void UpdatePercentageTexts(double mappedValue)
    {
        bluepercentage_Text.text = mappedValue.ToString("f2") + "%";
        purplepercentage_Text.text = (95 - mappedValue).ToString("f2") + "%";
    }

    void UpdateMultiplierTexts(double mappedValue)
    {
        Selectedx = mappedValue;
        bluemultiplier = CalculateMultiplier(mappedValue, socketIoManager.initialData.targetRTP, socketIoManager.initialData.houseEdge, 0);
        purplemultiplier = CalculateMultiplier(mappedValue, socketIoManager.initialData.targetRTP, socketIoManager.initialData.houseEdge, 1);


        Debug.Log($"###### multipliers are {bluemultiplier} and {purplemultiplier} ######");
        bluemultiplier_Text.text = bluemultiplier.ToString("f2");
        purplemultiplier_Text.text = purplemultiplier.ToString("f2");
        Debug.Log($"###### current total bet is {currentTotalBet} ######");
        currentTotalBet = socketIoManager.initialData.bets[BetCounter];
        bluewin_Text.text = ("WIN " + (currentTotalBet * bluemultiplier).ToString("f2"));
        purplewin_Text.text = ("WIN " + (currentTotalBet * purplemultiplier).ToString("f2"));

        // Debug.Log($"######## mappedValue is {mappedValue} ######## purpleX is {purpleX}");
    }




    public double CalculateMultiplier(double selectedX, double targetRTP, double houseEdge, int range)
    {
        double multiplier = 0;

        if (range == 0)
        {
            multiplier = (100.0 / selectedX) * (targetRTP / 100.0);
        }
        else if (range == 1)
        {
            multiplier = (100.0 / (100.0 - (selectedX + houseEdge))) * (targetRTP / 100.0);
        }

        multiplier = Math.Round(multiplier, 2);

        Debug.Log($"Calculating multiplier for selectedX: {selectedX}, targetRTP: {targetRTP}, houseEdge: {houseEdge}, range: {range} and multiplier: {multiplier}");

        return multiplier;
    }

    public void AdjustSlider(bool isIncrement)
    {
        float current = rotationSlider.value;
        float newValue;

        if (isIncrement)
        {
            newValue = (Mathf.Floor(current * 10f) + 1f) / 10f;
        }
        else
        {
            newValue = (Mathf.Ceil(current * 10f) - 1f) / 10f;
        }
        rotationSlider.value = Mathf.Clamp(newValue, rotationSlider.minValue, rotationSlider.maxValue);
    }










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
        .SetLoops(-1).OnUpdate(UpdateTextFromRotation);      // infinite
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
        ).SetEase(Ease.Linear).OnUpdate(UpdateTextFromRotation));

        // --- 3) Then tween to targetZ, but in the **next circle**
        float finalZ = finishZ + targetZ; // offset into next rotation cycle

        stopSequence.Append(circleGameObject.DOLocalRotate(
            new Vector3(0, 180, finalZ),
            stopEaseDuration,
            RotateMode.FastBeyond360
        ).SetEase(Ease.OutCubic).OnUpdate(UpdateTextFromRotation).OnComplete(() =>
    {
        float finalValue = targetZ / 3.6f;   // make sure we land exactly
        rotationnumber_Text.text = finalValue.ToString("F2");
        AnimateFinalText();
    })
);
    }
    private void UpdateTextFromRotation()
    {
        float z = circleGameObject.localEulerAngles.z % 360f;
        float value = z / 3.6f;
        rotationnumber_Text.text = value.ToString("f2");
    }
    private void AnimateFinalText()
    {
        // reset scale before anim
        rotationnumber_Text.transform.localScale = Vector3.one;

        // punch effect: scale up to 1.3x then back
        rotationnumber_Text.transform
            .DOScale(1.2f, 0.35f)       // scale up fast
                                        //.SetEase(Ease.OutBack)     // overshoot a bit
            .OnComplete(() =>
            {
                rotationnumber_Text.transform
                    .DOScale(1f, 0.2f) // return to normal
                    .SetEase(Ease.InBack);
            });
    }

    private void ChangeBet(bool IncDec)
    {
        Debug.Log("changeBetRan");
        if (IncDec)
        {
            BetCounter++;
            if (BetCounter >= socketIoManager.initialData.bets.Count)
            {
                BetCounter = 0;
            }
        }
        else
        {
            BetCounter--;
            if (BetCounter < 0)
            {
                BetCounter = socketIoManager.initialData.bets.Count - 1;
            }
        }
        if (TotalBet_text) TotalBet_text.text = (socketIoManager.initialData.bets[BetCounter]).ToString("f2");
        currentTotalBet = socketIoManager.initialData.bets[BetCounter];
        bluewin_Text.text = ("WIN " + (currentTotalBet * bluemultiplier).ToString("f2"));
        purplewin_Text.text = ("WIN " + (currentTotalBet * purplemultiplier).ToString("f2"));

    }

    private void sendBetData(int selectedRange)
    {
        DisableWinningUI();
        toggleUI(false);
        if (!isAutoBet)
        {
            StartCoroutine(accumulateResult(selectedRange));

        }
        else
        {
            // isAutoBetPlaying = true;
            autoBetCurrentCount = autoBetTotalCount;
            autoBetCoroutine = StartCoroutine(startAutoBet(selectedRange));
        }

    }


    IEnumerator accumulateResult(int selectedRange)
    {
        currentBalance = socketIoManager.playerdata.balance;
        if (currentBalance < currentTotalBet)
        {
            lowBalance();
            yield break;
        }

        else
        {
            audioManager.PlayWLAudio("spin");
            pressbetui_Object.SetActive(false);
            winningui_Object.SetActive(false);
            rotationnumber_Text.gameObject.SetActive(true);
            winninguiAmount_Text.text = "0.00";
            playanimation_Object.SetActive(true);
            touchDisable.SetActive(true);
            updateBalance(currentTotalBet, false);
            //socketIoManager.AccumulateResult(socketIoManager.initialData.Bets[BetCounter], rowDropDown.value, riskDropDown.value);
            socketIoManager.AccumulateResult(BetCounter, Selectedx, selectedRange);
            StartRotation();
            yield return new WaitUntil(() => socketIoManager.isResultdone);
            //yield return new WaitForSeconds(1f);
            StopRotation((float)(socketIoManager.resultData.randomValue * 3.6));
            yield return new WaitForSeconds(stopEaseDuration + 0.2f);
            playanimation_Object.SetActive(false);
            if (socketIoManager.resultData.winAmount > 0)
            {
                double winmultiplier = selectedRange == 0 ? bluemultiplier : purplemultiplier;

                winninguiAmount_Text.text = socketIoManager.resultData.winAmount.ToString("f2");
                winninguiMultiplier_Text.text = winmultiplier.ToString("f2") + "X";
                winningui_Object.SetActive(true);
                audioManager.PlayWLAudio("win");
            }
           // toggleUI(true);
            // else
            // {
            //     winningui_Object.SetActive(false);
            //     audioManager.PlaySound("loseSound");

            // }
            win_text.text = socketIoManager.resultData.winAmount.ToString("f2");
            balance_text.text = socketIoManager.playerdata.balance.ToString("f2");

            yield return new WaitForSeconds(1f);
            isAutobetInstanceDone = true;
            if (!isAutoBetPlaying)
            {
                touchDisable.SetActive(false);
                toggleUI(true);
            }
            // Invoke("DisableWinningUI", 3f);

        }

    }
    void DisableWinningUI()
    {
        winningui_Object.SetActive(false);
    }


    public void ToggleAutobet()
    {
        isAutoToggleOn = !isAutoToggleOn;
        onTogle_Object.SetActive(isAutoToggleOn);
        offTogle_Object.SetActive(!isAutoToggleOn);
        if (isAutoToggleOn)
        {
           // autoplayText_Object.SetActive(false);
           // AutoBetCustomize_Button.gameObject.SetActive(true);
            purpleAutoBet_Button.gameObject.SetActive(true);
            blueAutoBet_Button.gameObject.SetActive(true);
            isAutoBet = true;
            isautoplayEnable = true;
            BlueBet_Button.gameObject.SetActive(false);
            PurpleBet_Button.gameObject.SetActive(false);
        }
        else
        {
            isAutoBet = false;
            isautoplayEnable = false;
          //  autoplayText_Object.SetActive(true);
           // AutoBetCustomize_Button.gameObject.SetActive(false);
            purpleAutoBet_Button.gameObject.SetActive(false);
            blueAutoBet_Button.gameObject.SetActive(false);
            BlueBet_Button.gameObject.SetActive(true);
            PurpleBet_Button.gameObject.SetActive(true);
            toggleUI(true);
        }
    }

    void SelectBetRounds()
    {
        foreach (Button btn in setrounds_Buttons)
        {
            btn.image.color = Color.white;
        }
        autoBetTotalCount = 0;

        uiManager.OpenPopup(SelectplayRounds_Object);
    }
    void closeSelectBetRounds()
    {
        uiManager.ClosePopup(SelectplayRounds_Object);
    }

    private void SelectAutoBetRounds(int rounds)
    {
        isautoplayCountSelected = true;
        autoBetTotalCount = rounds;
        autoBetCurrentCount = rounds;
        autoBetCount_text.text = $"STOP (<size=28><color=#9CA3AF>{rounds}</color></size>)";
    }

    void SetSelectedRounds()
    {
        foreach (Button btn in setrounds_Buttons)
        {
            int rounds = int.Parse(btn.GetComponentInChildren<TMPro.TMP_Text>().text);
            btn.onClick.AddListener(() => OnSelectedroundsButtonClicked(btn, rounds));
        }
    }

    private void OnSelectedroundsButtonClicked(Button clickedButton, int rounds)
    {
        // Reset all button images to default color
        foreach (Button btn in setrounds_Buttons)
        {
            btn.image.color = unselectroundColor;
        }

        // Highlight clicked button's image
        clickedButton.image.color = Color.white;

        // Call your existing method
        SelectAutoBetRounds(rounds);
    }





    #endregion




    internal void setInitialUI()
    {

        currentBalance = socketIoManager.playerdata.balance;
        balance_text.text = socketIoManager.playerdata.balance.ToString("f2");
        currentTotalBet = socketIoManager.initialData.bets[0];
        if (TotalBet_text) TotalBet_text.text = currentTotalBet.ToString("f2");
        currentTotalBet = socketIoManager.initialData.bets[BetCounter];
        rotationSlider.value = 0.5f;

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
        uiManager.LowBalPopup();
    }

    internal void updateBalance(double amount, bool add)
    {
        if (add)
        {

            currentBalance += amount;
            balance_text.text = currentBalance.ToString("f2");
        }
        else
        {
            currentBalance -= amount;
            balance_text.text = currentBalance.ToString("f2");

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







    private void changeRiskFactor(string risk)
    {

        // Debug.Log(risk);
        // for (int i = 0; i < riskContainers.Count; i++)
        // {
        //     riskContainers[i].transform.parent.gameObject.SetActive(false);
        // }
        // switch (risk)
        // {
        //     case "low":
        //         {
        //             riskfactor = 0;
        //             int containerMultiplier = 3;
        //             for (int i = 0; i < socketIoManager.initialData.multiplier[0].Count; i++)
        //             {
        //                 riskContainers[i].text = containerMultiplier + "\n" + socketIoManager.initialData.multiplier[0][i].ToString() + "X";
        //                 riskContainers[i].transform.parent.gameObject.SetActive(true);
        //                 containerMultiplier++;
        //             }
        //             mediumRisk_Button.image.color = hideButtonCol;
        //             highRisk_Button.image.color = hideButtonCol;
        //             lowRisk_Button.image.color = highlightButtonCol;
        //             break;

        //         }
        //     case "medium":
        //         {
        //             riskfactor = 1;
        //             int containerMultiplier = 4;
        //             for (int i = 0; i < socketIoManager.initialData.multiplier[1].Count; i++)
        //             {
        //                 riskContainers[i].text = containerMultiplier + "\n" + socketIoManager.initialData.multiplier[1][i].ToString() + "X";
        //                 riskContainers[i].transform.parent.gameObject.SetActive(true);
        //                 containerMultiplier++;
        //             }
        //             mediumRisk_Button.image.color = highlightButtonCol;
        //             highRisk_Button.image.color = hideButtonCol;
        //             lowRisk_Button.image.color = hideButtonCol;
        //             break;
        //         }
        //     case "high":
        //         {
        //             riskfactor = 2;
        //             int containerMultiplier = 5;
        //             for (int i = 0; i < socketIoManager.initialData.multiplier[2].Count; i++)
        //             {
        //                 riskContainers[i].text = containerMultiplier + "\n" + socketIoManager.initialData.multiplier[2][i].ToString() + "X";
        //                 riskContainers[i].transform.parent.gameObject.SetActive(true);
        //                 containerMultiplier++;
        //             }
        //             mediumRisk_Button.image.color = hideButtonCol;
        //             highRisk_Button.image.color = highlightButtonCol;
        //             lowRisk_Button.image.color = hideButtonCol;
        //             break;
        //         }
        // }
    }



    IEnumerator startAutoBet(int selectedRange)
    {
        currentBalance = socketIoManager.playerdata.balance;
        if (currentBalance < currentTotalBet)
        {
            lowBalance();
            yield break;
        }
        isAutoBetPlaying = true;
        autoBet_Stop.image.sprite = (selectedRange == 0) ? betstop_Sprites[0] : betstop_Sprites[1];
        autoBet_Stop.gameObject.SetActive(true);
        autoBet_Button.interactable = false;
        if (!isautoplayCountSelected)
        {
            autobetRunning = true;
            while (isautoplayEnable)
            {
                autoBetInstanceDone = false;
                StartCoroutine(accumulateResult(selectedRange));
                yield return new WaitUntil(() => isAutobetInstanceDone);
                yield return new WaitForSeconds(2f);
            }
        }
        else
        {
            Debug.Log(autoBetCurrentCount);
            autobetRunning = true;
            for (int i = 0; i < autoBetTotalCount; i++)
            {
                autoBetCount_text.text = $"STOP (<size=28><color=#9CA3AF>{autoBetTotalCount - i - 1}</color></size>)";
                autoBetInstanceDone = false;
                StartCoroutine(accumulateResult(selectedRange));
                yield return new WaitUntil(() => isAutobetInstanceDone);
                yield return new WaitForSeconds(2f);
                //betCount_Text

                //$"STOP (<size=28><color=#9CA3AF>{autoBetTotalCount - i}</color></size>)";
            }
        }
        autobetRunning = false;
        touchDisable.SetActive(false);
        isAutoBetPlaying = false;
        autoBet_Stop.gameObject.SetActive(false);
        stopAutoBet();




    }

    void stopAutoBet()
    {
        isautoplayEnable = false;
        ToggleAutobet();
        autoBet_Stop.gameObject.SetActive(false);
        StopCoroutine(autoBetCoroutine);
        touchDisable.SetActive(false);
        isAutoBetPlaying = false;
        autoBet_Button.interactable = true;

    }


    private void toggleUI(bool toggle)
    {
Debug.Log($"toggleUI ran with {toggle}");
        TBetPlus_Button.interactable = toggle;
        TBetMinus_Button.interactable = toggle;
        purpleAutoBet_Button.interactable = toggle;
        PurpleBet_Button.interactable = toggle;
        blueAutoBet_Button.interactable = toggle;
        BlueBet_Button.interactable = toggle;
        plusslider_Button.interactable = toggle;
        minusslider_Button.interactable = toggle;



    }


}
