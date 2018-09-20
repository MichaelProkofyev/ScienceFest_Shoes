using UnityEngine;
using System.Collections;

public class InfraredSourceView : MonoBehaviour 
{
    public InfraredSourceManager _InfraredManager;
    
    void Start () 
    {
        gameObject.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
    }
    
    void Update()
    {
        gameObject.GetComponent<Renderer>().material.mainTexture = _InfraredManager.GetInfraredTexture();
    }
}
