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

    [Header("Player Game Data")]
    public float currentHealth, MaxHealth = 1;

    public float DamageCooldownDuration = 3;
    public float MoveSpeed = 5;
    public int currentLives, startingLives = 3;

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
        SetInitialState();

        yield return new WaitForSecondsRealtime(0);
    }

    public void UpdatePlayerRef(PlayerCharacter newPlayer)
    {
        player = newPlayer;
        print("PlayerRef updated");
    }

    void Update()
    {
        if (!GameManager.Instance.gameRunning) return;

        // Select Player Object with KeyPad 0
        //if (Input.GetKeyDown(KeyCode.Keypad0)) UnityEditor.Selection.activeGameObject = player.gameObject;

        // Handle Input
        if (!GameManager.Instance.inputSuspended && !GameManager.Instance.gamePaused && State != PlayerState.Hurt)
        {
            HandleInput();
            HandleGamepadInput();
        }

    }

    public void SetInitialState()
    {
        RefillHealth();

        //HUD.Instance.UpdateHealthbar(true, false);

        State = PlayerState.Idle;
        canMove = true;
        facingRight = false;
        invulnerable = false;

        //player.trails.enabled = true;
        //player.trails.on = true;
    }

    public void SpawnPlayer()
    {
        DespawnPlayer();

        Vector3 spawnPosition = Vector3.zero;  // TODO Fix
        //var spawnPosition = LevelController.Instance.CurrentLevel.playerSpawnPoint.transform.position;
        var PlayerObject = Instantiate(PlayerPrefab, spawnPosition, Quaternion.identity);
        PlayerObject.name = "Player";
        PlayerGraphicsRef = player.PlayerGraphics;
        UpdatePlayerRef(PlayerObject.GetComponent<PlayerCharacter>());

        SetInitialState();
        StartCoroutine(DamageCooldown());
    }

    public void DespawnPlayer()
    {
        var existingPlayer = FindObjectOfType<PlayerCharacter>();
        if (existingPlayer != null)
        {
            Destroy(existingPlayer.transform.gameObject);
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

    public void RefillHealth()
    {
        currentHealth = MaxHealth;
    }

    public void RefillLives()
    {
        currentLives = startingLives;
        HUD.Instance.UpdateLives();
    }

    // Player takes a hit
    public void PlayerHit(float damage)
    {
        if (State == PlayerState.Dead)  return;

        if (player != null)
        {
            if (invulnerable) return;

            CameraShaker.Instance.Shake(CameraShaker.ShakeStyle.Large);
            TakeDamage(damage);
            if (AudioManager.Instance.soundBank.TakeDamage) AudioManager.Instance.soundBank.TakeDamage.Play();

        }
    }

    public void TakeDamage(float damage)
    {
        if (masterInvulnerability)
        {
            print("Player avoided damage bc MasterInvulnerability is true");
            return;
        }

        //var healthDamage = Mathf.Min(currentHealth, damage);
        currentHealth -= damage;
        //HUD.Instance.UpdateHealthbar(true, true);

        print($"Player takes {damage} damage");

        player.HitFlash();

        StartCoroutine(VFX.Instance.StartDamageEffects()); 

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            StartCoroutine(PlayerDeath());
            return;
        }

        StartCoroutine(DamageCooldown());
    }

    public IEnumerator PlayerDeath()
    {
        StartCoroutine(GameManager.Instance.TimeAcceleration());

        if (AudioManager.Instance.soundBank.PlayerDeath) AudioManager.Instance.soundBank.PlayerDeath.Play();
        if (PlayerDeathVFX) Instantiate(PlayerDeathVFX, player.transform.position, Quaternion.identity, VFX.Instance.VFXContainer.transform);
        CameraShaker.Instance.Shake(CameraShaker.ShakeStyle.Large);

        State = PlayerState.Dead;
        canMove = false;
        invulnerable = true;

        currentLives--;
        HUD.Instance.UpdateLives();
        DespawnPlayer();
        Time.timeScale = 0;

        //LevelController.Instance.CurrentLevel.Init();

        if (currentLives <= 0)
        {
            currentLives = 0;
            HUD.Instance.UpdateLives();
            yield return new WaitForSecondsRealtime(3);

            GameManager.Instance.GameOver();
        }
        else
        {
            yield return new WaitForSecondsRealtime(1);
            //LevelController.Instance.CurrentLevel.StartLevel();
        }


    }


    public IEnumerator DamageCooldown(bool overrideWithDefaultValue = false)
    {
        if (invulnerable) yield break;

        invulnerable = true;
        player.spriteFlicker.flicker = true;

        var duration = overrideWithDefaultValue ? 1.5f : DamageCooldownDuration;
        yield return new WaitForSeconds(duration);

        invulnerable = false;
        player.spriteFlicker.flicker = false;
    }

}


