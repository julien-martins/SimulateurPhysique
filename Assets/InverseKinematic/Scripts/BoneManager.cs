using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneManager : MonoBehaviour
{
    public enum SelectionType
    {
        None,
        Head,
        Body,
        Tail
    }
    
    public Camera CameraMain;
    
    private Vector2 _mouseScreenPos;

    private SelectionType _selectionType;
    private Bone _selectionBone;

    private bool _moveSelectionBone;
    
    // Start is called before the first frame update
    void Start()
    {
        _selectionType = SelectionType.None;
        _selectionBone = null;
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();

        if (_moveSelectionBone)
        {
            Debug.Log("Move Bone");
            Debug.Log(_selectionType);
            _selectionBone.MoveSelection(_selectionType, _mouseScreenPos);
        }
        
    }

    void HandleInput()
    {
        _mouseScreenPos = CameraMain.ScreenToWorldPoint(Input.mousePosition);
        
        //Left click down
        if (_selectionType == SelectionType.None && Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(_mouseScreenPos, Vector2.zero);
            if (hit.collider)
            {
                _selectionBone = hit.collider.gameObject.GetComponentInParent<Bone>();
                
                if (hit.collider.CompareTag("Head"))
                {
                    _selectionType = SelectionType.Head;
                    _selectionBone.Select(_selectionType);
                } 
                else if (hit.collider.CompareTag("Tail"))
                {
                    _selectionType = SelectionType.Tail;
                    _selectionBone.Select(_selectionType);
                } 
                else if (hit.collider.CompareTag("Body"))
                {
                    _selectionType = SelectionType.Body;
                    _selectionBone.Select(_selectionType);
                }
            }
            
        }

        if (_selectionType != SelectionType.None && _moveSelectionBone && Input.GetMouseButtonDown(0))
        {
            ResetSelection();
        }
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            _moveSelectionBone = true;
        }
        
        
    }

    void ResetSelection()
    {
        _moveSelectionBone = false;
        _selectionType = SelectionType.None;
    }
    
}
