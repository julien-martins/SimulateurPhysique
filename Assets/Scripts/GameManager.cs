using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    private GameManager(){}

    public static GameManager Instance {get; private set;}

    public GameObject platformPrefab;
    public GameObject joinPrefab;

    public Transform platformsParent;
    public Transform nodesParent;
    
    public float ConstanteRappel = 0.01f;
    public float Gravity = 1.5f;
    
    private Vector2 mouseScreenPos;
    
    private GameObject _platformToPlace;
    
    private bool _joinSelectioned = false;
    private GameObject _joinSelectedObject;

    private bool moveJoin = false;
    private Join moveJoinObj;

    private bool _simulatePhysics;

    private List<Join> _allJoins;

    void Awake() {
        if(instance != null && instance != this)
            Destroy(gameObject);

        _allJoins = new();
        
        instance = this;
    }

    private void FixedUpdate()
    {
        if (_simulatePhysics)
        {
            SimulatePhysics();
        }
    }

    void Update()
    {
        mouseScreenPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if(Input.GetKeyDown(KeyCode.Space)) TogglePhysics();
        
        if (Input.GetMouseButtonDown(1))
        {
            if (_joinSelectioned)
            {
                Destroy(_platformToPlace);
                _joinSelectioned = false;
            }
            else
            {
                RaycastHit2D hit = Physics2D.Raycast(mouseScreenPos, Vector2.down);
                if (hit.collider != null)
                {
                    if (hit.collider.CompareTag("Join"))
                    {
                        if (!hit.collider.gameObject.GetComponent<Join>().Fixed)
                        {
                            moveJoin = true;
                            moveJoinObj = hit.collider.gameObject.GetComponent<Join>();
                        }
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(1) && moveJoin) moveJoin = false;

        if (Input.GetMouseButtonDown(0) && !moveJoin)
        {
            RaycastHit2D hit = Physics2D.Raycast(mouseScreenPos, Vector2.down);
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Join"))
                {
                    if (!_joinSelectioned)
                    {
                        _joinSelectioned = true;
                        _joinSelectedObject = hit.collider.gameObject;
                        _platformToPlace = Instantiate(platformPrefab, platformsParent);
                        _platformToPlace.transform.position = hit.collider.gameObject.transform.position;
                        _platformToPlace.GetComponent<Platform>().LeftAnchor =
                            hit.collider.gameObject.GetComponent<Join>();
                        Debug.Log("Join Clicked");
                    }
                    else if (_joinSelectedObject != hit.collider.gameObject)
                    {
                        _platformToPlace.GetComponent<Platform>().RightAnchor =
                            hit.collider.gameObject.GetComponent<Join>();
                        Platform plat = _platformToPlace.GetComponent<Platform>();
                        _platformToPlace.transform.localScale = new Vector3(
                            Vector2.Distance(plat.GetLeftAnchorPosition(), hit.collider.gameObject.transform.position),
                            0.3f, 0);

                        //Add platform to the two joins
                        hit.collider.gameObject.GetComponent<Join>().ConnectedPlatforms.Add(plat);
                        _joinSelectedObject.GetComponent<Join>().ConnectedPlatforms.Add(plat);

                        _joinSelectioned = false;
                    }

                }
                else if (hit.collider.CompareTag("Platform"))
                {

                }
            }
            else
            {
                //Check if there are no UI on mouse
                if (!EventSystem.current.IsPointerOverGameObject() && _joinSelectioned)
                {
                    //Add join at the end of road
                    var joinObject = Instantiate(joinPrefab, nodesParent);
                    joinObject.transform.position = new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0);
                    Join joinScript = joinObject.GetComponent<Join>();
                    joinScript.PositionInitial = joinScript.transform.position;
                    _allJoins.Add(joinScript);
                    joinScript.Fixed = false;
                    Platform plat = _platformToPlace.GetComponent<Platform>();
                    _platformToPlace.transform.localScale = new Vector3(
                        Vector2.Distance(plat.GetLeftAnchorPosition(), joinObject.transform.position), 0.3f, 0);
                    plat.RightAnchor = joinScript;

                    //Add platform to the two joins
                    joinScript.ConnectedPlatforms.Add(plat);
                    _joinSelectedObject.GetComponent<Join>().ConnectedPlatforms.Add(plat);

                    _joinSelectioned = false;
                }
            }
        }

        if (_joinSelectioned)
        {
            Debug.Log("Update temporary platform");
            Platform plat = _platformToPlace.GetComponent<Platform>();
            //Change scale with mouse
            _platformToPlace.transform.localScale =
                new Vector3(Vector2.Distance(plat.GetLeftAnchorPosition(), mouseScreenPos), 0.3f, 0);

            //Change angle
            float angleRad = Mathf.Atan2(mouseScreenPos.y - plat.GetLeftAnchorPosition().y,
                mouseScreenPos.x - plat.GetLeftAnchorPosition().x);
            float angle = (180 / Mathf.PI) * angleRad;

            _platformToPlace.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        if (moveJoin)
        {
            moveJoinObj.gameObject.transform.position = mouseScreenPos;

            UpdatePlatformConnectedToJoin(moveJoinObj);
        }
    }

    public void ClearBridge()
    {
        foreach (Transform child in nodesParent.transform)
        {
            Destroy(child.gameObject);
        }
        
        foreach (Transform child in platformsParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    void UpdatePlatformConnectedToJoin(Join joinToUpdate)
    {
        foreach (var platform in joinToUpdate.ConnectedPlatforms)
        {
            if (platform.RightAnchor == joinToUpdate)
            {
                //Change scale with mouse
                platform.Width = Vector2.Distance(platform.GetLeftAnchorPosition(), joinToUpdate.transform.position);
                platform.transform.localScale = new Vector3(platform.Width, 0.3f, 0);

                //Change angle
                float angleRad = Mathf.Atan2(joinToUpdate.transform.position.y - platform.GetLeftAnchorPosition().y, joinToUpdate.transform.position.x - platform.GetLeftAnchorPosition().x);
                float angle = (180 / Mathf.PI) * angleRad;
                    
                platform.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
            else if (platform.LeftAnchor == joinToUpdate)
            {
                platform.transform.position = joinToUpdate.transform.position;
                    
                //Change scale with mouse
                platform.Width = Vector2.Distance(platform.GetLeftAnchorPosition(), platform.GetRightAnchorPosition());
                platform.transform.localScale = new Vector3(platform.Width, 0.3f, 0);
                    
                //Change angle
                float angleRad = Mathf.Atan2(platform.GetRightAnchorPosition().y - platform.GetLeftAnchorPosition().y, platform.GetRightAnchorPosition().x - platform.GetLeftAnchorPosition().x);
                float angle = (180 / Mathf.PI) * angleRad;
                    
                platform.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
                
        }
    }

    public void TogglePhysics()
    {
        _simulatePhysics = !_simulatePhysics;
    }

    public void SimulatePhysics()
    {
        foreach (Join join in _allJoins)
        {   
            //Force Gravite
            join.Speed += Time.deltaTime * Vector3.down * 2.0f;
            
            //Force elastique
            Vector3 sumElasticForce = Vector3.zero;
            foreach(Platform platform in join.ConnectedPlatforms)
            {
                Vector3 dir = Vector3.zero;
                if (platform.LeftAnchor == join)
                {
                    dir = platform.LeftAnchor.transform.position - platform.RightAnchor.transform.position;
                }
                else if (platform.RightAnchor == join)
                {
                    dir = platform.RightAnchor.transform.position - platform.LeftAnchor.transform.position;
                }
                
                //Change the color of the platform 
                float force = -ConstanteRappel * (platform.Width - platform.RestWidth);
                Debug.Log(force);
                
                if(Math.Abs(force) > 1.8f) Debug.Log("Change Color");
                
                platform.Sprite.color = new Color(255, 127, 39);
                
                
                sumElasticForce += force * dir;
            }

            join.Speed += sumElasticForce * Time.fixedDeltaTime;
            
            //Update Platform link to the join
            UpdatePlatformConnectedToJoin(join);

            join.transform.position += join.Speed * Time.fixedDeltaTime / 20;
        }
    }
    
}
