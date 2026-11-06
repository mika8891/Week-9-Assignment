using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public Vector3 itemPosition;
    private SpriteRenderer spriteRenderer;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if(spriteRenderer != null)
        {
            spriteRenderer.color = GetRandomColor();
        }
    }

    private void Update()
    {
            var perspective = CameraComponent.focalLenth / (CameraComponent.focalLenth + itemPosition.z);
            transform.localScale = Vector3.one * perspective;
            transform.position = new Vector2(itemPosition.x, itemPosition.y) * perspective;
    }

    private Color GetRandomColor()
    {
        var rRand = Random.Range(0f, 1f);
        var gRand = Random.Range(0f, 1f);
        var bRand = Random.Range(0f, 1f);
        return new Color(rRand, gRand, bRand);                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                
    }


}
