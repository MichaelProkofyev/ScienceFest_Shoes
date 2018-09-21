using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoeSpawner : MonoBehaviour {


    public BodySourceView bodySourceView;
    public float cooldown = 1f;
    public float destroyDelay = 1f;
    public float xMaxOffset = 2f;


    public ParticleSystem particles;
    public float particleMatChangeCycle = 1f;
    // Use this for initialization
    void Start () {
        StartCoroutine(UpdateMaterial());
	}

    IEnumerator UpdateMaterial()
    {
        while(true)
        {
            yield return new WaitForSeconds(particleMatChangeCycle);
            particles.GetComponent<Renderer>().material = bodySourceView.shoeMaterials[Random.Range(0, bodySourceView.shoeMaterials.Length)];
        }
    }

    IEnumerator SpawnShoes()
    {
        while(true)
        {
            Vector3 spawnPosition = new Vector3(transform.position.x + Random.Range(-xMaxOffset, xMaxOffset), transform.position.y, transform.position.z);
            var newShoe = Instantiate(bodySourceView.shoeOverlayPrefab, spawnPosition, transform.rotation);
            var selectedShoeMat = bodySourceView.shoeMaterials[Random.Range(0, bodySourceView.shoeMaterials.Length)];
            newShoe.GetComponent<Renderer>().material = selectedShoeMat;
            Destroy(newShoe, destroyDelay);
            yield return new WaitForSeconds(cooldown);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
