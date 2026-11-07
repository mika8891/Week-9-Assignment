using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public Vector3 itemPosition;
    
    public int lane;
    public bool isHighObstacle;
    public bool hasHitPlayer;

    [Header("Perspective Track Settings")]
    public float horizonY = 3.0f;     // Top horizon Y (spawn point)
    public float baseGroundY = -3.5f; // Bottom ground Y (player level)

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = GetRandomColor();
        }
    }

    private void Update()
    {
        // Starter math calculation using CameraComponent.focalLength (300f)
        float perspective = CameraComponent.focalLength / (CameraComponent.focalLength   + itemPosition.z);
        perspective = Mathf.Clamp01(perspective); // Prevent overflow

        float progress = 1.0f - perspective;
        float visualT = 1.0f - (progress * progress); // Non-linear visual interpolation
        // Scale quad by depth
        transform.localScale = Vector3.one * perspective;

        // Linear interpolation from Horizon (top) to Ground (bottom player lane)
        float projectedX = itemPosition.x * perspective;
        float projectedY = Mathf.Lerp(horizonY, baseGroundY + itemPosition.y, perspective);

        transform.position = new Vector3(projectedX, projectedY, 0.0f);

        // Painter's algorithm depth sorting
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = Mathf.RoundToInt((5000.0f - itemPosition.z) * 0.1f);
        }
    }

    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    private Color GetRandomColor()
    {
        var rRand = Random.Range(0f, 1f);
        var gRand = Random.Range(0f, 1f);
        var bRand = Random.Range(0f, 1f);
        return new Color(rRand, gRand, bRand);
    }
}