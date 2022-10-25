using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public Join LeftAnchor { get; set; }
    public Join RightAnchor { get; set; }

    public Vector3 GetLeftAnchorPosition() => LeftAnchor.transform.position;
    public Vector3 GetRightAnchorPosition() => RightAnchor.transform.position;

}
