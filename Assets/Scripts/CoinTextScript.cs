using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CoinTextScript : MonoBehaviour {
    void Awake()
    {
        GetComponent<Text>().text = "+" + FindObjectOfType<ShapesManager>().GetCoinGain().ToString();

        if (FindObjectOfType<ShapesManager>().GetTurn() == 1)
        {
            this.transform.SetParent(GameObject.Find("P1Avatar").transform, false);
            GetComponent<RectTransform>().localPosition = new Vector3(40f, -230f, 0f);
        }
        else if (FindObjectOfType<ShapesManager>().GetTurn() == 2)
        {
            this.transform.SetParent(GameObject.Find("P2Avatar").transform, false);
            GetComponent<RectTransform>().localPosition = new Vector3(-60f, -230f, 0f);
        }        
    }

    // Use this for initialization
    void Start()
    {
        StartCoroutine(DestroyText());
    }

    public IEnumerator DestroyText()
    {
        yield return new WaitForSeconds(Constants.DamageDelay);
        Destroy(this.gameObject);
    }
}
