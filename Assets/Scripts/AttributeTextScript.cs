using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AttributeTextScript : MonoBehaviour {

    void Awake()
    {
        GetComponent<Text>().text = "+" + FindObjectOfType<ShapesManager>().GetAttributeGain().ToString();
        GetComponent<Text>().color = Color.green;

        if (FindObjectOfType<ShapesManager>().GetTurn() == 1)
            this.transform.SetParent(GameObject.Find("P1Avatar").transform, false);
        else if (FindObjectOfType<ShapesManager>().GetTurn() == 2)
            this.transform.SetParent(GameObject.Find("P2Avatar").transform, false);
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
