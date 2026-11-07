using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public ItemSpawner spawner;
    public HpBar hpBar;

    [Header("Lane Settings")]
    public float[] laneX = { -2.0f, 0.0f, 2.0f };
    public float laneEaseDuration = 0.2f;
    private int currentLane = 1;
    private float laneStartX;
    private float laneTargetX;
    private float laneElapsed = 1.0f;

    [Header("Jump Settings")]
    public float groundY = -1.0f;
    public float jumpPower = 8.0f;
    public float gravity = 16.0f;
    private float verticalVelocity;
    private bool isJumping;

    [Header("Player Depth & Collision")]
    public Vector3 playerPos = new Vector3(0.0f, -1.0f, 0.0f);
    public float zHitRange = 15.0f;
    public float hitRadius = 1.0f;
    public float hitDamage = 5.0f;

    private void Start()
    {
        currentLane = 1;
        playerPos = new Vector3(laneX[currentLane], groundY, 0.0f);
        laneStartX = playerPos.x;
        laneTargetX = playerPos.x;
    }

    private void Update()
    {
        HandleInput();
        UpdateLanePosition();
        UpdateJump();
        ApplyPerspective();
        CheckObstacleHits();
    }

    private void HandleInput()
    {
   
       // left/right lane change
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeLane(-1);

        // right lane change
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeLane(1);
        }

        // jump
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            verticalVelocity = jumpPower;
            isJumping = true;
        }
    }

    private void ChangeLane(int direction)
    {
        int nextLane = Mathf.Clamp(currentLane + direction, 0, laneX.Length - 1);
        if (nextLane == currentLane) return;

        currentLane = nextLane;
        laneStartX = playerPos.x;
        laneTargetX = laneX[currentLane];
        laneElapsed = 0.0f;
    }

    private void UpdateLanePosition()
    {
        if (laneElapsed < laneEaseDuration)
        {
            laneElapsed += Time.deltaTime;
            float rawT = Mathf.Clamp01(laneElapsed / laneEaseDuration);
            float easedT = Ease.EaseOutQuad(rawT);
            playerPos.x = Mathf.Lerp(laneStartX, laneTargetX, easedT);
        }
        else
        {
            playerPos.x = laneTargetX;
        }
    }

    private void UpdateJump()
    {
        if (!isJumping) return;

        playerPos.y += verticalVelocity * Time.deltaTime;
        verticalVelocity -= gravity * Time.deltaTime;

        if (playerPos.y <= groundY)
        {
            playerPos.y = groundY;
            verticalVelocity = 0.0f;
            isJumping = false;
        }
    }

    private void ApplyPerspective()
    {
        float denominator = CameraComponent.focalLength + playerPos.z;
        if (denominator <= 0.001f) denominator = 0.001f;

        float perspective = CameraComponent.focalLength / denominator;
        transform.localScale = Vector3.one * perspective;
        transform.position = new Vector2(playerPos.x, playerPos.y) * perspective;
    }

    private void CheckObstacleHits()
    {
        if (spawner == null) return;

        for (int i = 0; i < spawner.activeItems.Count; i++)
        {
            Item item = spawner.activeItems[i];
            if (item == null || item.hasHit) continue;

            float zDiff = Mathf.Abs(item.itemPosition.z - playerPos.z);
            if (zDiff <= zHitRange)
            {
                Vector2 toObstacle = new Vector2(item.itemPosition.x - playerPos.x, item.itemPosition.y - playerPos.y);
                if (toObstacle.magnitude <= hitRadius)
                {
                    item.hasHit = true;
                    if (hpBar != null)
                    {
                        hpBar.TakeDamage(hitDamage);
                    }
                }
            }
        }
    }
}