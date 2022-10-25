using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    private GameManager(){}

    public static GameManager Instance {get; private set;}

    public GameObject platformPrefab;
    public GameObject joinPrefab;

    public Transform platformsParent;
    public Transform nodesParent;
    
    private Vector2 mouseScreenPos;
    
    private GameObject _platformToPlace;
    private bool _joinSelectioned = false;
    private GameObject _joinSelectedObject;
    
    void Awake() {
        if(instance != null && instance != this)
            Destroy(gameObject);

        instance = this;
    }

    void Update()
    {
        mouseScreenPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(1) && _joinSelectioned)
        {
            Destroy(_platformToPlace);
            _joinSelectioned = false;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(mouseScreenPos, Vector2.down);
            if(hit.collider != null)
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
                    else if(_joinSelectedObject != hit.collider.gameObject)
                    {
                        _platformToPlace.GetComponent<Platform>().RightAnchor =
                            hit.collider.gameObject.GetComponent<Join>();
                        Platform plat = _platformToPlace.GetComponent<Platform>();
                        _platformToPlace.transform.localScale = new Vector3(Vector2.Distance(plat.GetLeftAnchorPosition(), hit.collider.gameObject.transform.position), 0.3f, 0);
                        _joinSelectioned = false;
                    }

                }
                else if (hit.collider.CompareTag("Platform"))
                {

                }
            }
            else
            {
                if (_joinSelectioned)
                {
                    //Add join at the end of road
                    var joinObject = Instantiate(joinPrefab, nodesParent);
                    joinObject.transform.position = new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0);
                    Join joinScript = joinObject.GetComponent<Join>();
                    joinScript.Fixed = false;
                    Platform plat = _platformToPlace.GetComponent<Platform>();
                    _platformToPlace.transform.localScale = new Vector3(Vector2.Distance(plat.GetLeftAnchorPosition(), joinObject.transform.position), 0.3f, 0);
                    _platformToPlace.GetComponent<Platform>().RightAnchor = joinScript;
                    _joinSelectioned = false;
                }
            }
        }

        if (_joinSelectioned)
        {
            Debug.Log("Update temporary platform");
            Platform plat = _platformToPlace.GetComponent<Platform>();
            //Change scale with mouse
            _platformToPlace.transform.localScale = new Vector3(Vector2.Distance(plat.GetLeftAnchorPosition(), mouseScreenPos), 0.3f, 0);

            //Change angle
            float angleRad = Mathf.Atan2(mouseScreenPos.y - plat.GetLeftAnchorPosition().y, mouseScreenPos.x - plat.GetLeftAnchorPosition().x);
            float angle = (180 / Mathf.PI) * angleRad;

            _platformToPlace.transform.rotation = Quaternion.Euler(0, 0, angle);
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

}
