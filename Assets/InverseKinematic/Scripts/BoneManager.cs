using System;
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

    public Bone BonePrefab;
    
    private Vector2 _oldMouseScreenPos;
    private Vector2 _mouseScreenPos;

    private SelectionType _selectionType;
    private Bone _selectionBone;

    private bool _moveSelectionBone;
    private bool _moveSelectionIKBone;
    
    // Start is called before the first frame update
    void Start()
    {
        _oldMouseScreenPos = Vector2.zero;
        _mouseScreenPos = Vector2.zero;
        
        _selectionType = SelectionType.None;
        _selectionBone = null;
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();

        if (_moveSelectionBone)
        {
            Vector3 deltaPos = _mouseScreenPos - _oldMouseScreenPos;
            _selectionBone.MoveSelection(_selectionType, deltaPos);
        }
        
    }

    void HandleInput()
    {
        _oldMouseScreenPos = _mouseScreenPos;
        _mouseScreenPos = CameraMain.ScreenToWorldPoint(Input.mousePosition);
        
        //Left click down
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(_mouseScreenPos, Vector2.zero);
            if (hit.collider)
            {
                if(_selectionBone)
                    _selectionBone.Deselect();
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

        if (_selectionType != SelectionType.None && (_moveSelectionBone || _moveSelectionIKBone) && Input.GetMouseButtonDown(0))
        {
            ResetSelection();
        }
        
        //Move Input
        if (Input.GetKeyDown(KeyCode.G))
        {
            _moveSelectionBone = true;
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            _moveSelectionIKBone = true;
        }
        
        //Extend Input
        if ((_selectionType == SelectionType.Head || _selectionType == SelectionType.Tail) &&
            Input.GetKeyDown(KeyCode.E))
        {
            ExtendBone();
        }

        if (_moveSelectionIKBone)
        {
            ReachTarget(_selectionBone, _mouseScreenPos);
        }
        
    }

    void ReachTarget(Bone bone, Vector2 target)
    {
        
        //Calculate the current length
        var c_dx = bone.Tail.position.x - bone.Head.position.x;
        var c_dy = bone.Tail.position.y - bone.Head.position.y;
        var c_dist = Math.Sqrt(c_dx * c_dx + c_dy * c_dy);

        //Calculate the stretched length
        var s_dx = bone.Head.position.x - target.x;
        var s_dy = bone.Head.position.y - target.y;
        var s_dist = Math.Sqrt(s_dx * s_dx + s_dy * s_dy);
        
        //Calculate how much to scale the stretched line
        var scale = c_dist / s_dist;

        bone.Tail.position = new Vector3(target.x, target.y, 0);
        bone.Head.position = new Vector3((float)(target.x + s_dx * scale), (float)(target.y + s_dy * scale), 0);
        
        bone.UpdateBody();
    }
    
    void ExtendBone()
    {
        Bone bone;
        if (_selectionType == SelectionType.Head)
        {
            bone = Instantiate(BonePrefab, _selectionBone.Head.transform);
        }
        else
        {
            bone = Instantiate(BonePrefab, _selectionBone.Tail.transform);
        }

        _selectionBone.Deselect();
        
        bone.Head.gameObject.SetActive(false);

        _selectionType = SelectionType.Tail;
        _selectionBone = bone;
        
        _selectionBone.Select(_selectionType);
        
        _moveSelectionBone = true;
        
        _selectionBone.UpdateBody();
    }
    
    void ResetSelection()
    {
        _moveSelectionBone = false;
        _moveSelectionIKBone = false;
    }
    
}
