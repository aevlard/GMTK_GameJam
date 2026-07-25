using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float globalTimerTime = 600f;
    [SerializeField] private List<InteractiveObjectBase> interactiveObjectToActive = new List<InteractiveObjectBase>();
    [SerializeField] private InteractiveObjectBase firstInteractiveObjectToActive;
    [SerializeField] private float timeBetweenActivation = 10f;
    [SerializeField] private TMP_Text globalTimerText;
    [SerializeField] private TMP_Text nbrOfMistakesText;

    private Timer _timer;
    private int nbrOfMisstakes = 0;
    private float activationTimer;
    private List<InteractiveObjectBase> remainingObjectsToActivate;

    private void Start()
    {
        remainingObjectsToActivate = new List<InteractiveObjectBase>(interactiveObjectToActive);

        _timer = new Timer(globalTimerTime)
            .OnComplete(() =>
            {
                Debug.Log("GameWin");
            })
            .OnTick((currentTime) =>
            {
                DisplayGlobalTimer(_timer.RemainingTime);
            });

        _timer.Start();

        firstInteractiveObjectToActive.StartItem();
    }

    private void Update()
    {
        _timer.Tick();

        HandleRandomActivation();
    }

    private void HandleRandomActivation()
    {
        if (remainingObjectsToActivate == null || remainingObjectsToActivate.Count == 0) return;

        activationTimer += Time.deltaTime;

        if (activationTimer >= timeBetweenActivation)
        {
            activationTimer = 0f;
            ActivateRandomItem();
        }
    }

    private void ActivateRandomItem()
    {
        int randomIndex = UnityEngine.Random.Range(0, remainingObjectsToActivate.Count);
        InteractiveObjectBase chosen = remainingObjectsToActivate[randomIndex];

        chosen.StartItem();
        Debug.Log("activation de" + chosen.name);
        remainingObjectsToActivate.RemoveAt(randomIndex);
    }

    private void DisplayGlobalTimer(float timerRemainingTime)
    {
        int minutes = Mathf.FloorToInt(timerRemainingTime / 60f);
        int seconds = Mathf.FloorToInt(timerRemainingTime % 60f);
        int centiseconds = Mathf.FloorToInt((timerRemainingTime * 100f) % 100f);

        if (globalTimerText != null)
        {
            globalTimerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, centiseconds);
        }
    }

    public void IncreaseNbrOfMisstakes()
    {
        nbrOfMisstakes++;
        DisplayNbrOfMistake();
        CheckNbrOfMistakes();
    }

    private void DisplayNbrOfMistake()
    {
        nbrOfMistakesText.text = nbrOfMisstakes.ToString();
    }

    private void CheckNbrOfMistakes()
    {
        if (nbrOfMisstakes > 3)
        {
            Debug.Log("fin de game");
            SceneLoader.Instance.LoadGameOver();
        }
    }
}