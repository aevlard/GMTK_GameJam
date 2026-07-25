using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PhoneHandler : InteractiveObjectBase
{
    [Header("Slider")]
    [SerializeField] private Slider timerSlider;
    [SerializeField] private Slider baitSlider;

    [Tooltip("Coche si tu veux que le slider se vide au fil du temps plutôt que se remplir")]
    [SerializeField] private bool invertFill;

    [Header("Reset via Slider")]
    [SerializeField, Range(0f, 1f)] private float resetThreshold = .95f;
    [SerializeField] private float timeAddedOnReset;

    [Header("Bait Slider")]
    [SerializeField, Range(0f, 1f)] private float baitThreshold = .95f;
    [SerializeField] private UnityEvent onBaitTriggered;

    private bool isDragging;
    private bool isDraggingBait;
    private VerticalLayoutGroup verticalLayoutGroup;

    private int nbrIfReset = 0;

    private void Awake()
    {
        verticalLayoutGroup = timerSlider.GetComponentInParent<VerticalLayoutGroup>();
        timeAddedOnReset = initialDuration;

        if (timerSlider != null)
            timerSlider.onValueChanged.AddListener(OnSliderValueChanged);

        if (baitSlider != null)
            baitSlider.onValueChanged.AddListener(OnBaitSliderValueChanged);
    }

    private void OnDestroy()
    {
        if (timerSlider != null)
            timerSlider.onValueChanged.RemoveListener(OnSliderValueChanged);

        if (baitSlider != null)
            baitSlider.onValueChanged.RemoveListener(OnBaitSliderValueChanged);
    }

    public void SetDragging(bool dragging, bool isBait = false)
    {
        if (isBait)
            isDraggingBait = dragging;
        else
            isDragging = dragging;
    }

    private void OnSliderValueChanged(float value)
    {
        if (!isDragging) return;

        bool reached = invertFill ? value <= (1f - resetThreshold) : value >= resetThreshold;

        if (reached)
        {
            AddTime(timeAddedOnReset);
            nbrIfReset++;
            isDragging = false;
        }
    }

    public override void MoveToPlayer(Transform playerHand)
    {
        base.MoveToPlayer(playerHand);
        ChangeSort();
    }

    private void OnBaitSliderValueChanged(float value)
    {
        if (!isDraggingBait) return;

        bool reached = invertFill ? value <= (1f - baitThreshold) : value >= baitThreshold;

        if (reached)
        {
            TriggerBait();
            isDraggingBait = false;
        }
    }

    protected virtual void TriggerBait()
    {
        onBaitTriggered?.Invoke();
        _timer.Reset();
    }

    protected override void DisplayTimer(float timerRemainingTime)
    {
        base.DisplayTimer(timerRemainingTime);

        if (initialDuration <= 0f) return;

        float ratio = Mathf.Clamp01(timerRemainingTime / initialDuration);
        float displayValue = invertFill ? 1f - ratio : ratio;

        if (timerSlider != null && !isDragging)
            timerSlider.value = displayValue;

        if (baitSlider != null && !isDraggingBait)
            baitSlider.value = displayValue;
    }

    private void ChangeSort()
    {
        if (nbrIfReset < 3)
        {
            verticalLayoutGroup.reverseArrangement = false;
        }
        else if(nbrIfReset == 3)
        {
            verticalLayoutGroup.reverseArrangement = true;
        }
        else
        {
            int random = UnityEngine.Random.Range(0, 2);

            if (random == 0)
            {
                verticalLayoutGroup.reverseArrangement = true;
            }
            else
            {
                verticalLayoutGroup.reverseArrangement = false;
            }
        }
    }
}