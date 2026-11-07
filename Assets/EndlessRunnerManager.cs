using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndlessRunnerManager : MonoBehaviour
{
    [Header("Player Settings")]
    public Item playerItem;
    public SpriteRenderer playerSpriteRenderer;
    public Sprite playerIdleSprite;
    public Sprite playerJumpSprite;
    public Sprite playerRunSprite;
    public Sprite playerHitSprite;

    [Header("HP Settings")]
    public float maxHP = 100.0f;
    public float currentHP = 100.0f;
    public float hpRegenRate = 1.0f; // 1 HP / sec
    public TMP_Text hpText;

    [Header("Lane & Movement Settings")]
    public float[] laneXPositions = new float[3] { -300.0f, 0.0f, 300.0f }; // Scaled for focalLength 300
    public int currentLane = 1; // Middle lane
    public float laneChangeDuration = 0.2f;
    public float jumpHeight = 150.0f;
    public float jumpDuration = 0.6f;

    [Header("Obstacle Settings")]
    public Item obstaclePrefab;
    public Sprite[] obstacleSprites;
    public float spawnZ = 2000.0f;
    public float despawnZ = -100.0f;
    public float obstacleSpeed = 600.0f;
    public float spawnInterval = 1.0f;
    public float zHitRange = 50.0f;

    // Movement state variables
    private Vector3 startPlayerPos;
    private Vector3 targetPlayerPos;
    private float laneTimer = 0.0f;
    private bool isChangingLane = false;

    private bool isJumping = false;
    private float jumpTimer = 0.0f;
    private float currentJumpY = 0.0f;

    private float hitVisualTimer = 0.0f;
    private float spawnTimer = 0.0f;

    private List<Item> activeObstacles = new List<Item>();

    private void Start()
    {
        currentHP = maxHP;
        targetPlayerPos = new Vector3(laneXPositions[currentLane], 0, 0);
        startPlayerPos = targetPlayerPos;
        if (playerItem != null) playerItem.itemPosition = targetPlayerPos;
        UpdateHPUI();
    }

    private void Update()
    {
        HandleInput();
        UpdatePlayerMovement();
        HandleRegenAndTimers();
        SpawnObstacles();
        UpdateObstacles();
        CheckCollisions();
        UpdatePlayerSprite();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (currentLane > 0)
            {
                currentLane--;
                StartLaneChange();
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (currentLane < 2)
            {
                currentLane++;
                StartLaneChange();
            }
        }

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) && !isJumping)
        {
            isJumping = true;
            jumpTimer = 0.0f;
        }
    }

    private void StartLaneChange()
    {
        isChangingLane = true;
        laneTimer = 0.0f;
        startPlayerPos = targetPlayerPos;
    }

    private void UpdatePlayerMovement()
    {
        // 1. Lane switch easing (Cubic Smoothstep using scalar math)
        float currentX = laneXPositions[currentLane];
        if (isChangingLane)
        {
            laneTimer += Time.deltaTime;
            float t = Mathf.Clamp01(laneTimer / laneChangeDuration);
            float easedT = t * t * (3.0f - 2.0f * t);

            currentX = startPlayerPos.x + (laneXPositions[currentLane] - startPlayerPos.x) * easedT;

            if (t >= 1.0f) isChangingLane = false;
        }

        // 2. Parabolic Jump Arc
        if (isJumping)
        {
            jumpTimer += Time.deltaTime;
            float normTime = jumpTimer / jumpDuration;
            if (normTime >= 1.0f)
            {
                isJumping = false;
                currentJumpY = 0.0f;
            }
            else
            {
                currentJumpY = jumpHeight * Mathf.Sin(normTime * Mathf.PI);
            }
        }
        else
        {
            currentJumpY = 0.0f;
        }

        targetPlayerPos = new Vector3(currentX, currentJumpY, 0.0f);
        if (playerItem != null)
        {
            playerItem.itemPosition = targetPlayerPos;
        }
    }

    private void HandleRegenAndTimers()
    {
        if (currentHP < maxHP && currentHP > 0)
        {
            currentHP += hpRegenRate * Time.deltaTime;
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
            UpdateHPUI();
        }

        if (hitVisualTimer > 0.0f) hitVisualTimer -= Time.deltaTime;
    }

    private void UpdateHPUI()
    {
        if (hpText != null)
        {
            hpText.text = $"HP: {Mathf.CeilToInt(currentHP)} / {maxHP}";
        }
    }

    private void SpawnObstacles()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0.0f;
            int randomLane = Random.Range(0, 3);
            bool isHigh = Random.value > 0.6f;

            Item newObs = Instantiate(obstaclePrefab, new Vector3(0, 1000, 0), Quaternion.identity);
            newObs.lane = randomLane;
            newObs.isHighObstacle = isHigh;
            newObs.hasHitPlayer = false;
            newObs.itemPosition = new Vector3(laneXPositions[randomLane], isHigh ? 120.0f : 0.0f, spawnZ);

            if (obstacleSprites != null && obstacleSprites.Length > 0)
            {
                newObs.SetSprite(obstacleSprites[Random.Range(0, obstacleSprites.Length)]);
            }

            activeObstacles.Add(newObs);
        }
    }

    private void UpdateObstacles()
    {
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            Item obs = activeObstacles[i];
            obs.itemPosition.z -= obstacleSpeed * Time.deltaTime;

            if (obs.itemPosition.z <= despawnZ)
            {
                Destroy(obs.gameObject);
                activeObstacles.RemoveAt(i);
            }
        }
    }

    private void CheckCollisions()
    {
        foreach (var obs in activeObstacles)
        {
            if (obs.hasHitPlayer) continue;

            float zDiff = obs.itemPosition.z - targetPlayerPos.z;
            if (zDiff < 0) zDiff = -zDiff;

            if (zDiff <= zHitRange && obs.lane == currentLane)
            {
                bool safeByJumping = (!obs.isHighObstacle && currentJumpY > 80.0f);
                if (!safeByJumping)
                {
                    obs.hasHitPlayer = true;
                    ApplyDamage(5.0f);
                }
            }
        }
    }

    private void ApplyDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        hitVisualTimer = 0.35f;
        UpdateHPUI();
    }

    private void UpdatePlayerSprite()
    {
        if (playerSpriteRenderer == null) return;

        if (hitVisualTimer > 0.0f && playerHitSprite != null)
        {
            playerSpriteRenderer.sprite = playerHitSprite;
        }
        else if (isJumping && playerJumpSprite != null)
        {
            playerSpriteRenderer.sprite = playerJumpSprite;
        }
        else if (isChangingLane && playerRunSprite != null)
        {
            playerSpriteRenderer.sprite = playerRunSprite;
        }
        else if (playerIdleSprite != null)
        {
            playerSpriteRenderer.sprite = playerIdleSprite;
        }
    }
}