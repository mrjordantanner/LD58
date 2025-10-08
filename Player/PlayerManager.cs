using System.Collections;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System;
using System.Linq;
using Unity.Services.CloudSave.Models.Data.Player;


public enum PlayerState { Idle, Walking, Jumping, Shooting, Hurt, Dead }

public class PlayerManager : MonoBehaviour, IInitializable
{
    #region Singleton
    public static PlayerManager Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        #endregion

    }

    public string Name { get { return "Player Manager"; } }

    [ReadOnly] public PlayerState State;

    #region Declarations
    public bool useDpad, useAnalogStick;

    public GameObject PlayerSpawnPoint;

    [Header("Player Game Data")]
    public float currentHealth, MaxHealth = 1;

    public float DamageCooldownDuration = 3;
    public float MoveSpeed = 5;
    public int currentLives, startingLives = 3;
    
    [Header("Drag Settings")]
    public bool enableDrag = true;
    public float dragForce = 10f;
    public float maxDragForce = 50f;
    public float dragDamping = 0.8f;
    public float restThreshold = 0.5f; // Distance from cursor where object stops moving
    
    [Header("Weight & Physics")]
    public float weight = 1f;
    public float massMultiplier = 1f;
    public bool usePhysicsWeight = true;
    public float physicsDrag = 2f;
    public float angularDrag = 5f;
    
    [Header("Deceleration")]
    public float decelerationForce = 15f;
    public float decelerationDamping = 0.9f;
    public bool useDeceleration = true;
    
    [Header("Mouse Settings")]
    public float mouseSensitivity = 1f;
    public float maxDragDistance = 10f;
    public LayerMask dragLayerMask = -1;
    
    [Header("Visual Feedback")]
    public bool showDragLine = true;
    public Color dragLineColor = Color.yellow;
    public float dragLineWidth = 0.1f;

    [Header("Clickable References")]
    public GameObject PlayerPrefab;
    public PlayerCharacter player;
    public GameObject PlayerGraphicsRef;
    public Transform playerSpawnPoint;
    public GameObject PlayerDeathVFX;

    public float damageRadius = 2f;
    public float respawnTime = 3f;

    [Header("Input")]
    [ReadOnly] public Vector2 directionalInput;
    [ReadOnly] public float horiz;
    [ReadOnly] public float vert;

    [Header("States")]
    public bool invulnerable;
    public bool canMove = true;
    [ReadOnly]
    public bool
        isMoving,
        facingRight,
        masterInvulnerability;

    #endregion


    public IEnumerator Init()
    {
        //
        //SetInitialState();

        yield return new WaitForSecondsRealtime(0);
    }

    public void UpdatePlayerRef(PlayerCharacter newPlayer)
    {
        player = newPlayer;
        print("PlayerRef updated");
    }

    void Update()
    {
        if (!GameManager.Instance.IsGameRunning()) return;

        // Handle Input
        if (!GameManager.Instance.inputSuspended && !GameManager.Instance.IsGamePaused() && State != PlayerState.Hurt)
        {
            HandleInput();
            HandleGamepadInput();
        }

    }

    //public void SetInitialState()
    //{
    //    //RefillHealth();

    //    //HUD.Instance.UpdateHealthbar(true, false);

    //    //State = PlayerState.Idle;
    //   // canMove = true;
    //    //facingRight = false;
    //    //invulnerable = false;

    //    //player.trails.enabled = true;
    //    //player.trails.on = true;

    //    // Subscribe to player events
    //    // EventManager.Instance.Subscribe(EventManager.PLAYER_DAMAGED, OnPlayerDamaged);
    //    // EventManager.Instance.Subscribe(EventManager.PLAYER_DIED, OnPlayerDied);
    //    // EventManager.Instance.Subscribe(EventManager.PLAYER_RESPAWNED, OnPlayerRespawned);
        
    //    // Subscribe to collectible events
    //    EventManager.Instance.Subscribe(EventManager.COLLECTIBLE_PICKED_UP, OnCollectiblePickedUp);
    //}

    public void SpawnPlayer()
    {
        Debug.Log("PlayerManager: SpawnPlayer() called");
        
        DespawnPlayer();

        if (playerSpawnPoint == null)
        {
            Debug.LogError("PlayerManager: playerSpawnPoint is null! Please assign a spawn point in the Inspector.");
            return;
        }

        if (PlayerPrefab == null)
        {
            Debug.LogError("PlayerManager: PlayerPrefab is null! Please assign a player prefab in the Inspector.");
            return;
        }

        var spawnPosition = playerSpawnPoint.transform.position;
        Debug.Log($"PlayerManager: Spawning player at position {spawnPosition}");
        
        var PlayerObject = Instantiate(PlayerPrefab, spawnPosition, Quaternion.identity);
        PlayerObject.name = "Player";
        
        // Get the PlayerCharacter component from the instantiated object
        var playerCharacter = PlayerObject.GetComponent<PlayerCharacter>();
        if (playerCharacter == null)
        {
            Debug.LogError("PlayerManager: PlayerCharacter component not found on instantiated player!");
            return;
        }

        PlayerGraphicsRef = playerCharacter.PlayerGraphics;
        UpdatePlayerRef(playerCharacter);

        // Apply current theme colors to the player sprites with smooth transition
        if (ThemeController.Instance != null)
        {
            ThemeController.Instance.ApplyThemeToPlayerWithTween(playerCharacter, 0.8f, Ease.OutQuart);
        }

        Debug.Log("PlayerManager: Player spawned successfully");
        //SetInitialState();
        //StartCoroutine(DamageCooldown());
    }

    public void DespawnPlayer()
    {
        var existingPlayer = FindObjectOfType<PlayerCharacter>();
        if (existingPlayer != null)
        {
            Destroy(existingPlayer.transform.gameObject);
        }
    }

    public void OnCollectiblePickedUp(object data)
    {
        Debug.Log($"PlayerManager: OnCollectiblePickedUp called with data: {data}");
        
        if (data is Collectible collectible)
        {
            Debug.Log($"PlayerManager: Collectible picked up: {collectible.name}");
            AudioManager.Instance.PlaySound("Hit-1");

            // TODO play VFX

            // Complete the current round
            if (Progression.Instance != null)
            {
                Debug.Log($"PlayerManager: Progression instance found. IsInRound: {Progression.Instance.IsInRound()}");
                if (Progression.Instance.IsInRound())
                {
                    // TODO add points to score based on round timer?
                    Progression.Instance.CompleteRound();
                    Debug.Log("PlayerManager: Round completed!");
                    
                    // Test alert for round success
                    HUD.Instance.ShowAlertMessage("Round Complete", 0.3f, 2f, 0.5f);
                }
                else
                {
                    Debug.Log("PlayerManager: Not in a round, cannot complete");
                }
            }
            else
            {
                Debug.LogError("PlayerManager: Progression.Instance is null!");
            }
        }
        else
        {
            Debug.LogWarning($"PlayerManager: Expected Collectible but got {data?.GetType()}");
        }
    }

    public void HandleGamepadInput()
    {
        // Dpad 
        if (useDpad)
        {
            horiz = Input.GetAxisRaw("DpadHoriz");
            vert = Input.GetAxisRaw("DpadVert");
        }
        // Analog Stick
        if (useAnalogStick)
        {
            horiz = Input.GetAxisRaw("Horizontal");
            vert = Input.GetAxisRaw("Vertical");
        }

        directionalInput = new(horiz, vert);
    }

    public void HandleInput()
    {
        if (!player
            || GameManager.Instance.inputSuspended
            || State == PlayerState.Hurt || State == PlayerState.Dead) return;

        // Get keyboard horiz input
        if (Input.GetKey(InputManager.Instance.downKey))  vert = -1;
        else if (Input.GetKey(InputManager.Instance.upKey)) vert = 1;
        else vert = 0;

        // Get keyboard vert input
        if (Input.GetKey(InputManager.Instance.leftKey)) horiz = -1;
        else if (Input.GetKey(InputManager.Instance.rightKey)) horiz = 1;
        else horiz = 0;
    }

    //public void RefillHealth()
    //{
    //    currentHealth = MaxHealth;
    //}

    //// Player takes a hit
    //public void PlayerHit(float damage)
    //{
    //    if (State == PlayerState.Dead)  return;

    //    if (player != null)
    //    {
    //        if (invulnerable) return;

    //        CameraShaker.Instance.Shake(CameraShaker.ShakeStyle.Large);
    //        TakeDamage(damage);
    //        if (AudioManager.Instance.soundBank.TakeDamage) AudioManager.Instance.soundBank.TakeDamage.Play();

    //    }
    //}

    //public void TakeDamage(float damage)
    //{
    //    if (masterInvulnerability)
    //    {
    //        print("Player avoided damage bc MasterInvulnerability is true");
    //        return;
    //    }

    //    //var healthDamage = Mathf.Min(currentHealth, damage);
    //    currentHealth -= damage;
    //    //HUD.Instance.UpdateHealthbar(true, true);

    //    print($"Player takes {damage} damage");

    //    player.HitFlash();

    //    StartCoroutine(VFX.Instance.StartDamageEffects()); 

    //    if (currentHealth <= 0)
    //    {
    //        currentHealth = 0;
    //        StartCoroutine(PlayerDeath());
    //        return;
    //    }

    //    StartCoroutine(DamageCooldown());
    //}

    //public IEnumerator PlayerDeath()
    //{
    //    if (AudioManager.Instance.soundBank.PlayerDeath) AudioManager.Instance.soundBank.PlayerDeath.Play();
    //    if (PlayerDeathVFX) Instantiate(PlayerDeathVFX, player.transform.position, Quaternion.identity, VFX.Instance.VFXContainer.transform);
    //    CameraShaker.Instance.Shake(CameraShaker.ShakeStyle.Large);

    //    State = PlayerState.Dead;
    //    canMove = false;
    //    invulnerable = true;

    //    currentLives--;
    //    HUD.Instance.UpdateLives();
    //    DespawnPlayer();
    //    Time.timeScale = 0;

    //    //LevelController.Instance.CurrentLevel.Init();

    //    if (currentLives <= 0)
    //    {
    //        currentLives = 0;
    //        HUD.Instance.UpdateLives();
    //        yield return new WaitForSecondsRealtime(3);

    //        GameManager.Instance.GameOver();
    //    }
    //    else
    //    {
    //        yield return new WaitForSecondsRealtime(1);
    //        //LevelController.Instance.CurrentLevel.StartLevel();
    //    }


    //}


    //public IEnumerator DamageCooldown(bool overrideWithDefaultValue = false)
    //{
    //    if (invulnerable) yield break;

    //    invulnerable = true;
    //    player.spriteFlicker.flicker = true;

    //    var duration = overrideWithDefaultValue ? 1.5f : DamageCooldownDuration;
    //    yield return new WaitForSeconds(duration);

    //    invulnerable = false;
    //    player.spriteFlicker.flicker = false;
    //}

}


