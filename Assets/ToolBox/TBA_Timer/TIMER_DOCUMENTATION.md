# Système de Timer - Documentation

## Vue d'ensemble

Un système de timer flexible et réutilisable pour Unity avec deux modes d'utilisation :
- **Manuel** : Tu gères le `Tick()` dans ton `Update()`
- **Automatique** : Le `TimerManager` s'occupe de tout

## Installation

Placer les fichiers dans votre projet Unity :
- `Timer.cs` - Classe principale du timer
- `TimerManager.cs` - Manager optionnel pour gestion automatique
- `TimerExamples.cs` - Exemples d'utilisation (optionnel)

## Utilisation rapide

### Méthode 1 : Timer manuel

```csharp
public class MyScript : MonoBehaviour
{
    private Timer timer;
    
    private void Start()
    {
        // Créer et démarrer un timer de 3 secondes
        timer = new Timer(3f)
            .OnComplete(() => Debug.Log("Terminé !"))
            .Start();
    }
    
    private void Update()
    {
        // Mettre à jour le timer
        timer.Tick();
    }
}
```

### Méthode 2 : Timer automatique (recommandé)

```csharp
public class MyScript : MonoBehaviour
{
    private void Start()
    {
        // Le TimerManager gère automatiquement le Tick()
        TimerManager.CreateTimer(3f)
            .OnComplete(() => Debug.Log("Terminé !"))
            .Start();
    }
}
```

## API Complète

### Création

```csharp
// Constructeur
Timer timer = new Timer(5f);

// Créer et démarrer immédiatement
Timer timer = Timer.StartNew(5f);

// Créer un timer en loop
Timer timer = Timer.StartNewLoop(2f);

// Avec TimerManager (automatique)
Timer timer = TimerManager.CreateTimer(5f);
```

### Contrôle

```csharp
timer.Start();          // Démarre/redémarre le timer
timer.Pause();          // Met en pause
timer.Resume();         // Reprend après pause
timer.Stop();           // Arrête et réinitialise
timer.Reset();          // Réinitialise sans arrêter
timer.ForceComplete();  // Force la complétion immédiate
```

### Configuration

```csharp
// Chaînage de méthodes (fluent API)
timer.SetDuration(10f)
     .SetLoop(true)
     .SetUnscaledTime(true)
     .OnComplete(() => Debug.Log("Done!"))
     .OnTick(time => Debug.Log(time))
     .Start();
```

### Callbacks

```csharp
// Appelé quand le timer se termine
timer.OnComplete(() => {
    Debug.Log("Timer terminé !");
});

// Appelé chaque frame avec le temps actuel
timer.OnTick(currentTime => {
    Debug.Log($"Temps: {currentTime}s");
});
```

### Propriétés

```csharp
timer.Duration        // Durée totale (float)
timer.CurrentTime     // Temps écoulé (float)
timer.RemainingTime   // Temps restant (float)
timer.Progress        // Progression 0-1 (float)
timer.IsRunning       // Est en cours ? (bool)
timer.IsFinished      // Est terminé ? (bool)
```

### Options

```csharp
// Loop : Le timer se relance automatiquement
timer.SetLoop(true);

// Unscaled Time : Ignore Time.timeScale (continue pendant la pause)
timer.SetUnscaledTime(true);

// Ajouter du temps au timer
timer.AddTime(2f);
```

## Exemples pratiques

### 1. Cooldown de compétence

```csharp
private Timer dashCooldown = new Timer(2f);

private void Update()
{
    dashCooldown.Tick();
    
    if (Input.GetKeyDown(KeyCode.Space) && !dashCooldown.IsRunning)
    {
        PerformDash();
        dashCooldown.Start();
    }
}

private void PerformDash()
{
    Debug.Log("DASH !");
    // Logique de dash...
}
```

### 2. Spawn périodique

```csharp
private void Start()
{
    // Spawn un ennemi toutes les 3 secondes
    TimerManager.CreateTimer(3f)
        .SetLoop(true)
        .OnComplete(() => SpawnEnemy())
        .Start();
}
```

### 3. Buff temporaire

```csharp
private float damageMultiplier = 1f;

private void ActivateDamageBuff()
{
    damageMultiplier = 2f;
    Debug.Log("Dégâts x2 activés !");
    
    TimerManager.CreateTimer(10f)
        .OnComplete(() => {
            damageMultiplier = 1f;
            Debug.Log("Buff terminé");
        })
        .Start();
}
```

### 4. Compte à rebours

```csharp
private void StartCountdown()
{
    float duration = 60f;
    
    TimerManager.CreateTimer(duration)
        .OnTick(time => {
            float remaining = duration - time;
            countdownText.text = $"{Mathf.CeilToInt(remaining)}";
            
            if (remaining <= 10f)
            {
                // Effet d'urgence
                countdownText.color = Color.red;
            }
        })
        .OnComplete(() => {
            Debug.Log("Temps écoulé !");
            GameOver();
        })
        .Start();
}
```

### 5. Barre de progression

```csharp
private void ShowLoadingBar()
{
    TimerManager.CreateTimer(5f)
        .OnTick(time => {
            float progress = time / 5f;
            loadingBar.fillAmount = progress;
            loadingText.text = $"{Mathf.RoundToInt(progress * 100)}%";
        })
        .OnComplete(() => {
            Debug.Log("Chargement terminé !");
            HideLoadingScreen();
        })
        .Start();
}
```

### 6. Invincibilité avec effet clignotant

```csharp
private bool isInvincible = false;

private void ActivateInvincibility()
{
    isInvincible = true;
    
    // Effet clignotant
    var blinkTimer = Timer.StartNewLoop(0.15f)
        .OnComplete(() => {
            spriteRenderer.enabled = !spriteRenderer.enabled;
        });
    TimerManager.Register(blinkTimer);
    
    // Fin après 3 secondes
    TimerManager.CreateTimer(3f)
        .OnComplete(() => {
            isInvincible = false;
            spriteRenderer.enabled = true;
            TimerManager.Unregister(blinkTimer);
        })
        .Start();
}
```

### 7. Combo system

```csharp
private int comboCount = 0;
private Timer comboTimer;

private void OnAttackHit()
{
    comboCount++;
    Debug.Log($"Combo x{comboCount}");
    
    // Reset le timer de combo ou en crée un nouveau
    if (comboTimer != null && comboTimer.IsRunning)
    {
        comboTimer.Reset();
    }
    else
    {
        comboTimer = TimerManager.CreateTimer(2f)
            .OnComplete(() => {
                Debug.Log("Combo perdu !");
                comboCount = 0;
            })
            .Start();
    }
}
```

### 8. Pattern d'attaque de boss

```csharp
private void StartBossPhase2()
{
    // Attaque 1 après 1s
    TimerManager.CreateTimer(1f)
        .OnComplete(() => FireProjectiles())
        .Start();
    
    // Attaque 2 après 3s
    TimerManager.CreateTimer(3f)
        .OnComplete(() => GroundSlam())
        .Start();
    
    // Attaque 3 après 5s puis répète
    TimerManager.CreateTimer(5f)
        .SetLoop(true)
        .OnComplete(() => LaserBeam())
        .Start();
}
```

### 9. Timer pause-proof (UI)

```csharp
// Timer qui continue même si le jeu est en pause
private void ShowTimedNotification()
{
    notificationPanel.SetActive(true);
    
    TimerManager.CreateTimer(3f)
        .SetUnscaledTime(true)  // Ignore Time.timeScale
        .OnComplete(() => {
            notificationPanel.SetActive(false);
        })
        .Start();
}
```

### 10. Combo de plusieurs timers

```csharp
private void SequentialActions()
{
    // Action 1 immédiatement
    Action1();
    
    // Action 2 après 1 seconde
    TimerManager.CreateTimer(1f)
        .OnComplete(() => {
            Action2();
            
            // Action 3 après 2 secondes de plus
            TimerManager.CreateTimer(2f)
                .OnComplete(() => Action3())
                .Start();
        })
        .Start();
}
```

## TimerManager

### Fonctions utiles

```csharp
// Créer un timer automatique
Timer timer = TimerManager.CreateTimer(5f);

// Enregistrer un timer manuel pour qu'il soit auto-géré
TimerManager.Register(myTimer);

// Désenregistrer
TimerManager.Unregister(myTimer);

// Arrêter tous les timers
TimerManager.StopAll();

// Nettoyer tous les timers
TimerManager.Clear();
```

### Notes

- Le `TimerManager` se crée automatiquement au premier usage
- Il persiste entre les scènes (`DontDestroyOnLoad`)
- Les timers terminés sont automatiquement nettoyés
- Les timers en loop restent enregistrés jusqu'à ce que tu les arrêtes

## Intégration avec d'autres systèmes

### Avec votre SaveSystem

```csharp
public class PlayerController : MonoBehaviour, ISaveable
{
    private Timer cooldownTimer;
    
    public Dictionary<string, object> CaptureState()
    {
        return new Dictionary<string, object>
        {
            { "cooldownRemaining", cooldownTimer.RemainingTime }
        };
    }
    
    public void RestoreState(Dictionary<string, object> data)
    {
        float remaining = (float)data["cooldownRemaining"];
        if (remaining > 0)
        {
            cooldownTimer.SetDuration(remaining).Start();
        }
    }
}
```

### Avec votre FSM

```csharp
public class PlayerJumpState : IState
{
    private Timer jumpDurationTimer;
    
    public void Enter()
    {
        jumpDurationTimer = new Timer(0.5f)
            .OnComplete(() => {
                controller.ChangeState(controller.fallState);
            })
            .Start();
    }
    
    public void Execute()
    {
        jumpDurationTimer.Tick();
    }
}
```

## Bonnes pratiques

### ✅ À FAIRE

- Utiliser `TimerManager` pour les timers automatiques
- Nettoyer les timers dans `OnDestroy()` si nécessaire
- Utiliser `SetUnscaledTime(true)` pour les UI/menus
- Utiliser des noms de variables descriptifs

### ❌ À ÉVITER

- Oublier d'appeler `Tick()` sur les timers manuels
- Créer des dizaines de timers sans les gérer
- Utiliser des timers pour des animations (préférer Animator ou DOTween)

## Performance

- Les timers sont très légers (quelques bytes)
- Le `TimerManager` gère efficacement des centaines de timers
- Pas d'allocation mémoire pendant l'exécution

## Comparaison avec d'autres solutions

### vs Coroutines

**Avantages des Timers :**
- Plus facile à mettre en pause/reprendre
- Pas de problème avec le garbage collector
- Plus facile à debugger
- Accès direct à la progression

**Avantages des Coroutines :**
- Mieux pour les séquences complexes
- Intégration native Unity

### vs Invoke/InvokeRepeating

**Avantages des Timers :**
- Plus de contrôle (pause, reset, progression)
- Pas de string pour les noms de méthodes
- Callbacks modernes (Action, Lambda)
- Meilleure performance

## Troubleshooting

**Le timer ne se met pas à jour**
- Avez-vous appelé `Tick()` dans `Update()` ?
- Ou utilisez-vous `TimerManager.Register()` ?

**Le timer continue pendant la pause**
- Utilisez `.SetUnscaledTime(false)` (défaut)

**Le timer ne loop pas**
- Avez-vous appelé `.SetLoop(true)` ?

**Callbacks non appelés**
- Vérifiez que les callbacks sont définis avant `Start()`

## Extensions possibles

Vous pouvez facilement étendre le système :

```csharp
// Timer avec easing
public Timer SetEasing(AnimationCurve curve)
{
    // Implémenter l'easing
    return this;
}

// Timer avec événements Unity
public Timer OnCompleteEvent(UnityEvent onComplete)
{
    // Support des UnityEvents
    return this;
}
```
