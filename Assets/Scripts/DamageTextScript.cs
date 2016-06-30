using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageTextScript : MonoBehaviour {
    public float minX = 0f;
    public float maxX = 0f;
    public float minY = 7f;
    public float maxY = 43f;

    void Awake()
    {
        GetComponent<Text>().text = FindObjectOfType<ShapesManager>().GetDamageDealt().ToString();

        if (FindObjectOfType<ShapesManager>().GetTurn() == 1)
        {
            minX = 190f;
            maxX = 314f;
        }
        else if (FindObjectOfType<ShapesManager>().GetTurn() == 2)
        {
            minX = -314f;
            maxX = -190f;
        }

        this.transform.SetParent(GameObject.Find("Canvas").transform, false);
        GetComponent<RectTransform>().localPosition = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 0f);
    }

	// Use this for initialization
	void Start () {        
        StartCoroutine(DestroyText());
	}

    public IEnumerator DestroyText()
    {
        yield return new WaitForSeconds(Constants.DamageDelay);
        Destroy(this.gameObject);
    }
}
