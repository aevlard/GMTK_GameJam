using UnityEngine;

/// <summary>
/// Exemples d'utilisation du système de Timer
/// </summary>
public class TimerExamples : MonoBehaviour
{
    // Timers manuels (tu gères le Tick)
    private Timer manualTimer;
    private Timer cooldownTimer;
    
    // Timers automatiques (gérés par TimerManager)
    private Timer autoTimer;

    private void Start()
    {
        // ===== EXEMPLE 1: Timer simple =====
        SimpleTimerExample();

        // ===== EXEMPLE 2: Timer avec callback =====
        CallbackTimerExample();

        // ===== EXEMPLE 3: Timer en loop =====
        LoopTimerExample();

        // ===== EXEMPLE 4: Timer automatique (pas besoin de Tick) =====
        AutomaticTimerExample();

        // ===== EXEMPLE 5: Cooldown =====
        CooldownExample();

        // ===== EXEMPLE 6: Timer avec progression =====
        ProgressTimerExample();

        // ===== EXEMPLE 7: Timer unscaled (pause-proof) =====
        UnscaledTimerExample();

        // ===== EXEMPLE 8: Chaînage de méthodes =====
        ChainedTimerExample();
    }

    private void Update()
    {
        // Mise à jour des timers manuels
        manualTimer?.Tick();
        cooldownTimer?.Tick();
    }

    // ==================== EXEMPLES ====================

    private void SimpleTimerExample()
    {
        // Créer un timer de 3 secondes
        manualTimer = new Timer(3f);
        manualTimer.Start();
        
        Debug.Log("Timer simple démarré pour 3 secondes");
    }

    private void CallbackTimerExample()
    {
        // Timer avec callback à la fin
        var timer = new Timer(2f)
            .OnComplete(() => Debug.Log("Timer terminé !"))
            .Start();
            
        // Enregistrer pour mise à jour auto
        TimerManager.Register(timer);
    }

    private void LoopTimerExample()
    {
        // Timer qui se répète toutes les 5 secondes
        var loopTimer = Timer.StartNewLoop(5f)
            .OnComplete(() => Debug.Log("Tick toutes les 5 secondes !"));
            
        TimerManager.Register(loopTimer);
    }

    private void AutomaticTimerExample()
    {
        // Timer géré automatiquement par le TimerManager
        autoTimer = TimerManager.CreateTimer(10f)
            .OnComplete(() => Debug.Log("Timer auto terminé !"))
            .OnTick(time => {
                if (Mathf.RoundToInt(time) != Mathf.RoundToInt(time - Time.deltaTime))
                {
                    Debug.Log($"Temps écoulé: {Mathf.RoundToInt(time)}s");
                }
            })
            .Start();
    }

    private void CooldownExample()
    {
        // Système de cooldown
        cooldownTimer = new Timer(1.5f);
        
        // Utilisation du cooldown (dans Update ou sur input)
        // if (Input.GetKeyDown(KeyCode.Space) && !cooldownTimer.IsRunning)
        // {
        //     UseAbility();
        //     cooldownTimer.Start();
        // }
    }

    private void ProgressTimerExample()
    {
        // Timer avec suivi de progression
        var progressTimer = new Timer(8f)
            .OnTick(time => {
                float progress = time / 8f;
                // Mettre à jour une barre de progression
                // progressBar.fillAmount = progress;
            })
            .OnComplete(() => Debug.Log("Chargement terminé !"))
            .Start();
            
        TimerManager.Register(progressTimer);
    }

    private void UnscaledTimerExample()
    {
        // Timer qui continue même si Time.timeScale = 0 (pause)
        var pauseProofTimer = new Timer(3f)
            .SetUnscaledTime(true)
            .OnComplete(() => Debug.Log("Ce timer ignore la pause !"))
            .Start();
            
        TimerManager.Register(pauseProofTimer);
    }

    private void ChainedTimerExample()
    {
        // Création fluide avec chaînage
        TimerManager.CreateTimer(2f)
            .SetLoop(false)
            .SetUnscaledTime(false)
            .OnComplete(() => Debug.Log("Première action"))
            .OnTick(t => Debug.Log($"Progress: {t:F2}s"))
            .Start();
    }

    // ==================== EXEMPLES DE CAS D'USAGE ====================

    // Exemple: Spawn d'ennemis périodique
    private Timer enemySpawnTimer;
    
    private void StartEnemySpawner()
    {
        enemySpawnTimer = Timer.StartNewLoop(3f)
            .OnComplete(() => {
                // SpawnEnemy();
                Debug.Log("Ennemi spawné !");
            });
            
        TimerManager.Register(enemySpawnTimer);
    }

    // Exemple: Dash avec cooldown
    private Timer dashCooldown = new Timer(2f);
    
    private void TryDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !dashCooldown.IsRunning)
        {
            PerformDash();
            dashCooldown.Start();
        }
    }

    private void PerformDash()
    {
        Debug.Log("Dash !");
        // Logique de dash...
    }

    // Exemple: Buff temporaire
    private void ApplyTemporaryBuff()
    {
        // Activer le buff
        bool isBuffActive = true;
        Debug.Log("Buff activé !");
        
        // Timer pour désactiver après 10 secondes
        TimerManager.CreateTimer(10f)
            .OnComplete(() => {
                isBuffActive = false;
                Debug.Log("Buff terminé !");
            })
            .Start();
    }

    // Exemple: Compte à rebours avant game over
    private void StartGameOverCountdown()
    {
        TimerManager.CreateTimer(60f)
            .OnTick(time => {
                float remaining = 60f - time;
                // UpdateCountdownUI(remaining);
                
                if (remaining <= 10f && Mathf.RoundToInt(remaining) != Mathf.RoundToInt(remaining - Time.deltaTime))
                {
                    Debug.Log($"Game Over dans {Mathf.CeilToInt(remaining)} secondes !");
                }
            })
            .OnComplete(() => {
                Debug.Log("GAME OVER !");
                // GameOver();
            })
            .Start();
    }

    // Exemple: Animation de chargement
    private void ShowLoadingAnimation()
    {
        float duration = 3f;
        
        TimerManager.CreateTimer(duration)
            .OnTick(time => {
                float progress = time / duration;
                // loadingBar.fillAmount = progress;
                // loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
            })
            .OnComplete(() => {
                Debug.Log("Chargement terminé !");
                // HideLoadingScreen();
            })
            .Start();
    }

    // Exemple: Invincibilité temporaire
    private bool isInvincible = false;
    
    private void ActivateInvincibility()
    {
        isInvincible = true;
        Debug.Log("Invincibilité activée !");
        
        // Effet visuel clignotant
        var blinkTimer = Timer.StartNewLoop(0.2f)
            .OnComplete(() => {
                // ToggleSpriteBlink();
            });
        TimerManager.Register(blinkTimer);
        
        // Fin de l'invincibilité après 3 secondes
        TimerManager.CreateTimer(3f)
            .OnComplete(() => {
                isInvincible = false;
                TimerManager.Unregister(blinkTimer);
                Debug.Log("Invincibilité terminée !");
            })
            .Start();
    }

    // Exemple: Gestion de patterns d'attaque
    private void StartBossAttackPattern()
    {
        // Attaque 1 après 1 seconde
        TimerManager.CreateTimer(1f)
            .OnComplete(() => Debug.Log("Boss Attack 1"))
            .Start();
            
        // Attaque 2 après 3 secondes
        TimerManager.CreateTimer(3f)
            .OnComplete(() => Debug.Log("Boss Attack 2"))
            .Start();
            
        // Attaque 3 après 5 secondes
        TimerManager.CreateTimer(5f)
            .OnComplete(() => Debug.Log("Boss Attack 3"))
            .Start();
    }

    private void OnDestroy()
    {
        // Nettoyer les timers quand l'objet est détruit
        TimerManager.Unregister(autoTimer);
    }
}
