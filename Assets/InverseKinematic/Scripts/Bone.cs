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

    public void MoveSelection(BoneManager.SelectionType type, Vector2 newPos)
    {
        switch (type)
        {
            case BoneManager.SelectionType.Head:
                Head.position = newPos;
                break;
            case BoneManager.SelectionType.Body:
                Head.position = newPos;
                Tail.position = newPos;
                break;
            case BoneManager.SelectionType.Tail:
                Tail.position = newPos;
                break;
        }
        
        UpdateBody();
    }

    void UpdateBody()
    {
        Body.position = Head.position;
        Body.localScale = Vector2.Distance(Head.position, Tail.position);
    }
    
}
