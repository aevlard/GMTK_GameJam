using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Vie")]
    [SerializeField] private Image[] lifeImages; // assigne les 3 images dans l'ordre, dans l'inspector

    [Header("Couleur quand une vie est perdue")]
    [SerializeField] private Color darkenedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private float globalTimerTime = 600f;
    [SerializeField] private List<InteractiveObjectBase> interactiveObjectToActive = new List<InteractiveObjectBase>();
    [SerializeField] private InteractiveObjectBase firstInteractiveObjectToActive;
    [SerializeField] private float timeBetweenActivation = 10f;
    [SerializeField] private TMP_Text globalTimerText;
    [SerializeField] private PlayerInput playerInput;
    
    [Header("Post Processing")]
    [SerializeField] private Volume pps;

    private ColorAdjustments colorAdjustments;
    private ChromaticAberration chromaticAberration;
    private Vignette vignette;
    private Bloom bloom;
    private LensDistortion lensDistortion;

    private Timer _timer;
    private int nbrOfMisstakes = 0;
    private float activationTimer;
    private List<InteractiveObjectBase> remainingObjectsToActivate;


    private void Awake()
    {
        if (pps == null || pps.profile == null)
        {
            Debug.LogWarning("PPS Volume ou profile manquant !");
            return;
        }

        pps.profile.TryGet(out colorAdjustments);
        pps.profile.TryGet(out chromaticAberration);
        pps.profile.TryGet(out vignette);
        pps.profile.TryGet(out bloom);
        pps.profile.TryGet(out lensDistortion);
    }

    private void Start()
    {
        playerInput.SwitchCurrentActionMap("ObjectView");
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        remainingObjectsToActivate = new List<InteractiveObjectBase>(interactiveObjectToActive);

        _timer = new Timer(globalTimerTime)
            .OnComplete(() =>
            {
                SceneLoader.Instance.LoadLevel("Win_Scene");
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
        ApplyPPSSettings();
        DisplayNbrOfMistake();
        CheckNbrOfMistakes();
    }

    private void ApplyPPSSettings()
    {
        if (nbrOfMisstakes == 1)
        {
            SetPPSValues(contrast: 15f, saturation: 30f, chromaticAberrationIntensity: 0.25f,
                vignetteIntensity: 0.25f, bloomIntensity: 1f, lensDistortionIntensity: -.15f);
        }

        if (nbrOfMisstakes == 2)
        {
            SetPPSValues(contrast: 25f, saturation: 50f, chromaticAberrationIntensity: 0.4f,
                vignetteIntensity: 0.4f, bloomIntensity: 2f, lensDistortionIntensity: -.3f);
        }
    }

    private void SetPPSValues(float contrast, float saturation, float chromaticAberrationIntensity,
        float vignetteIntensity, float bloomIntensity, float lensDistortionIntensity)
    {
        if (colorAdjustments != null)
        {
            colorAdjustments.contrast.value = contrast;
            colorAdjustments.saturation.value = saturation;
        }

        if (chromaticAberration != null)
            chromaticAberration.intensity.value = chromaticAberrationIntensity;

        if (vignette != null)
            vignette.intensity.value = vignetteIntensity;

        if (bloom != null)
            bloom.intensity.value = bloomIntensity;

        if (lensDistortion != null)
            lensDistortion.intensity.value = lensDistortionIntensity;
    }

    private void DisplayNbrOfMistake()
    {
        int indexToDarken = nbrOfMisstakes - 1;

        if (lifeImages == null || indexToDarken < 0 || indexToDarken >= lifeImages.Length) return;

        lifeImages[indexToDarken].color = darkenedColor;
    }

    private void CheckNbrOfMistakes()
    {
        if (nbrOfMisstakes >= 3)
        {
            PlayerPrefs.SetFloat("timeLeft", globalTimerTime);
            SceneLoader.Instance.LoadLevel("GameOver_Scene");
        }
    }

    public void OnCloseTutoButtonClick(GameObject tutoWindow)
    {
        playerInput.SwitchCurrentActionMap("Player");
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        tutoWindow.SetActive(false);
        Time.timeScale = 1f;
    }
}