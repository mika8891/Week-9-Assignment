using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Positioning & State")]
    public Vector3 itemPosition;
    public bool hasHit = false;

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
        float denominator = CameraComponent.focalLength + itemPosition.z;
        if (denominator <= 0.001f) denominator = 0.001f;

        float perspective = CameraComponent.focalLength / denominator;

        transform.localScale = Vector3.one * perspective;
        transform.position = new Vector2(itemPosition.x, itemPosition.y) * perspective;

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = Mathf.RoundToInt(5000.0f - itemPosition.z);
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
        return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }
}