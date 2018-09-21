using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoeController : MonoBehaviour {

    public Renderer shoeRenderer;
    public float blendCooldownDuration = 1f;
    public float blendSpeed = 1f;
    public Texture[] shoeTextures;

    private float blendCooldownCurrent;
    private float blendProgress = 0;
    private int textureIndex1 = 0;
    private int textureIndex2 = 0;

    // Use this for initialization
    void Start () {
        textureIndex1 = Random.Range(0, shoeTextures.Length);
        textureIndex2 = Random.Range(0, shoeTextures.Length);
        shoeRenderer.material.SetTexture("_MainTex1", shoeTextures[textureIndex1]);
        shoeRenderer.material.SetTexture("_MainTex2", shoeTextures[textureIndex2]);
        shoeRenderer.material.SetFloat("_BlendValue", 0);
    }
	
	void Update ()
    {
        if (blendCooldownCurrent <= 0)
        {
            blendProgress += Time.deltaTime * blendSpeed;
            shoeRenderer.material.SetFloat("_BlendValue", blendProgress);

            if (blendProgress >= 1f)
            {
                blendProgress = 0;
                blendCooldownCurrent = blendCooldownDuration;
                //update textures
                shoeRenderer.material.SetTexture("_MainTex1", shoeTextures[textureIndex2]);
                textureIndex2 = Random.Range(0, shoeTextures.Length);
                shoeRenderer.material.SetTexture("_MainTex2", shoeTextures[textureIndex2]);
                shoeRenderer.material.SetFloat("_BlendValue", 0);
            }

        }
        else
        {
            blendCooldownCurrent -= Time.deltaTime;
        }

	}
}
