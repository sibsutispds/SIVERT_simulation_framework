using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeSign : MonoBehaviour
{
    private GameObject go;

    public TextMesh timeSign;
    // Start is called before the first frame update
    void Start()
    {
        go = this.gameObject;
        timeSign = go.GetComponent<TextMesh>();
    }

    // Update is called once per frame
    void Update()
    {
        
        timeSign.text = "Lund Central Station. SimTime: " + Time.time.ToString();
    }
}
