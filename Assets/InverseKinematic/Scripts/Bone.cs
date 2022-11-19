using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bone : MonoBehaviour
{
    public Transform Parent;
    
    public Transform Head;
    public SpriteRenderer HeadSprite;

    public Transform Body;
    public SpriteRenderer BodySprite;
    
    public Transform Tail;
    public SpriteRenderer TailSprite;

    private Vector3 initialHeadPos;
    private Vector3 initialTailPos;
    private Vector3 initialScale;

    public void Start()
    {
        initialScale = Body.transform.localScale;
        initialHeadPos = Head.position;
        initialTailPos = Tail.position;
    }
    
    public void Select(BoneManager.SelectionType type)
    {
        switch (type)
        {
            case BoneManager.SelectionType.Head:
                TailSprite.color = Color.white;
                BodySprite.color = Color.white;
                HeadSprite.color = Color.cyan;
                break;
            case BoneManager.SelectionType.Body:
                TailSprite.color = Color.white;
                BodySprite.color = Color.cyan;
                HeadSprite.color = Color.white;
                break;
            case BoneManager.SelectionType.Tail:
                TailSprite.color = Color.cyan;
                BodySprite.color = Color.white;
                HeadSprite.color = Color.white;
                break;
            default:
                TailSprite.color = Color.white;
                BodySprite.color = Color.white;
                HeadSprite.color = Color.white;
                break;
        }
    }

    public void Deselect()
    {
        TailSprite.color = Color.white;
        BodySprite.color = Color.white;
        HeadSprite.color = Color.white;
    }

    public void MoveSelection(BoneManager.SelectionType type, Vector3 deltaPos)
    {
        switch (type)
        {
            case BoneManager.SelectionType.Head:
                Head.position += deltaPos;
                break;
            case BoneManager.SelectionType.Body:
                Head.position += deltaPos;
                Tail.position += deltaPos;

                break;
            case BoneManager.SelectionType.Tail:
                Tail.position += deltaPos;
                break;
        }
        
        UpdateBody();
    }

    public void UpdateBody()
    {
        Body.position = Head.position;
        float dist = Vector2.Distance(Head.position, Tail.position);
        Body.localScale = new Vector3(dist + dist/7, dist/7, 0.0f);
        
        float angleRad = Mathf.Atan2(Tail.transform.position.y - Head.transform.position.y, Tail.transform.position.x - Head.transform.position.x);
        float angle = (180 / Mathf.PI) * angleRad;
                    
        Body.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
}
