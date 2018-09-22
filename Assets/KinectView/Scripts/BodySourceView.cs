using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

struct LastPos
{
    public Vector3 lastLeftHandPos;
    public Vector3 lastRightHandPos;

    public LastPos(Vector3 lastLeftHandPos, Vector3 lastRightHandPos)
    {
        this.lastLeftHandPos = lastLeftHandPos;
        this.lastRightHandPos = lastRightHandPos;
    }
}

public class BodySourceView : MonoBehaviour 
{
    public Vector2 baseScale = Vector2.one * 15f;
    public Vector3 offset;
    public Vector2 morphMultiplier = Vector2.one;
    public GameObject shoeOverlayPrefab;
    public Material BoneMaterial;
    public BodySourceManager _BodyManager;
    public bool showBones = false;

    public Vector3 overlayScaleMultiplier = Vector3.one;
    

    public Material[] shoeMaterials;
    
    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private Dictionary<ulong, LastPos> lastPositions = new Dictionary<ulong, LastPos>();
    private Dictionary<ulong, Transform> shoeOverlays = new Dictionary<ulong, Transform>();
    private Dictionary<ulong, Material> instancedShoeMats = new Dictionary<ulong, Material>();

    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };
    
    void Update () 
    {        
        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }
        
        List<ulong> trackedIds = new List<ulong>();
        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
              }
                
            if(body.IsTracked)
            {
                trackedIds.Add (body.TrackingId);
            }
        }
        
        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
        
        // First delete untracked bodies
        foreach(ulong trackingId in knownIds)
        {
            if(!trackedIds.Contains(trackingId))
            {
                shoeOverlays[trackingId].GetComponent<ShoeController>().Die();
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
                lastPositions.Remove(trackingId);
                shoeOverlays.Remove(trackingId);

                instancedShoeMats.Remove(trackingId);
            }
        }

        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
            }
            
            if(body.IsTracked)
            {
                if(!_Bodies.ContainsKey(body.TrackingId))
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                    lastPositions[body.TrackingId] = new LastPos(Vector3.zero, Vector3.zero);

                    //Create shoe overlay
                    var shoeOverlay = Instantiate(shoeOverlayPrefab);
                    //var selectedShoeMat = shoeMaterials[Random.Range(0, shoeMaterials.Length)];
                    //shoeOverlay.GetComponent<Renderer>().material = selectedShoeMat;
                    shoeOverlay.name = "Shoe overlay";
                    shoeOverlays[body.TrackingId] = shoeOverlay.transform;
                    instancedShoeMats[body.TrackingId] = shoeOverlay.GetComponent<Renderer>().material;
                }
                
                RefreshBodyObject(body, _Bodies[body.TrackingId], lastPositions[body.TrackingId], body.TrackingId);
            }
        }
    }
    
    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);
        
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            jointObj.SetActive(showBones);
            
            LineRenderer lr = jointObj.AddComponent<LineRenderer>();
            lr.SetVertexCount(2);
            lr.material = BoneMaterial;
            lr.SetWidth(0.05f, 0.05f);
            
            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;
        }

        return body;
    }
    
    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject, LastPos lastPosition, ulong trackingid)
    {
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        Vector3 spineBasePos = Vector3.zero;
        Vector3 spineMidPos = Vector3.zero;

        float handDeltaLeft = 0;
        float handDeltaRight = 0;

        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;
            
            if(_BoneMap.ContainsKey(jt))
            {
                targetJoint = body.Joints[_BoneMap[jt]];
            }
            
            Transform jointObj = bodyObject.transform.Find(jt.ToString());
            jointObj.localPosition = GetVector3FromJoint(sourceJoint);
            shoeOverlays[trackingid].localPosition = GetVector3FromJoint(sourceJoint);

            if (jointObj.localPosition.x > maxX)
            {
                maxX = jointObj.localPosition.x;
            }
            if (jointObj.localPosition.x < minX)
            {
                minX = jointObj.localPosition.x;
            }
            if (jointObj.localPosition.y > maxY)
            {
                maxY = jointObj.localPosition.y;
            }
            if (jointObj.localPosition.y < minY)
            {
                minY = jointObj.localPosition.y;
            }

            if (jt == Kinect.JointType.SpineBase)
            {
                spineBasePos = jointObj.localPosition;
            }
            else if (jt == Kinect.JointType.SpineMid)
            {
                spineMidPos = jointObj.localPosition;
            }
            else if(jt == Kinect.JointType.ShoulderLeft)
            {
                handDeltaLeft = Vector3.Distance(lastPosition.lastLeftHandPos, jointObj.localPosition);
                lastPosition.lastLeftHandPos = jointObj.localPosition;
            }
            else if (jt == Kinect.JointType.ShoulderRight)
            {
                handDeltaRight = Vector3.Distance(lastPosition.lastRightHandPos, jointObj.localPosition);
                lastPosition.lastRightHandPos = jointObj.localPosition;
            }

            if (showBones)
            {
                LineRenderer lr = jointObj.GetComponent<LineRenderer>();
                if(targetJoint.HasValue)
                {
                    lr.SetPosition(0, jointObj.localPosition);
                    lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value));
                    lr.SetColors(GetColorForState (sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
                }
                else
                {
                    lr.enabled = false;
                }
            }
        }

        //print("HAND LEFT DELTA " + handDeltaLeft);
        //print("HAND RIGHT DELTA " + handDeltaRight);

        instancedShoeMats[trackingid].SetFloat("_OffsetMultiplierX", handDeltaLeft * morphMultiplier.x);
        instancedShoeMats[trackingid].SetFloat("_OffsetMultiplierY", handDeltaRight * morphMultiplier.y);

        lastPositions[trackingid] = lastPosition;

        //Calc rotation
        float rotationZ = Mathf.Atan2(spineMidPos.y - spineBasePos.y, spineMidPos.x - spineBasePos.x) * 180f / Mathf.PI - 90f;
        //print(rotation);
        var shoeOverlay = shoeOverlays[trackingid];
        var shoePos = shoeOverlay.transform.localPosition;
        shoeOverlay.transform.localPosition = new Vector3((maxX + minX) / 2f + +offset.x, (maxY + minY) / 2f + offset.y, shoePos.z + offset.z);
        float shoeScaleX = baseScale.x + (maxX - minX) * overlayScaleMultiplier.x;
        float shoeScaleY = baseScale.y + (maxY - minY) * overlayScaleMultiplier.y;
        shoeOverlay.transform.localScale = new Vector3(shoeScaleX, shoeScaleY, shoeOverlay.transform.localScale.z * overlayScaleMultiplier.z);
        shoeOverlay.transform.localRotation = Quaternion.Euler(0, 0, rotationZ * 1.5f);
    }

    private static Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
        case Kinect.TrackingState.Tracked:
            return Color.green;

        case Kinect.TrackingState.Inferred:
            return Color.red;

        default:
            return Color.black;
        }
    }
    
    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }
}
