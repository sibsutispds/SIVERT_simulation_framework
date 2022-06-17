using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
// using UnityEngine.UI;

public class TimeScreen : MonoBehaviour
{
    public TextMesh TimeSign;
    // Start is called before the first frame update
    void Start()
    {
        TimeSign = this.GetComponent<TextMesh>();
    }

    // Update is called once per frame
    void Update()
    {
        TimeSign.text = "Sim Time: " + Time.time.ToString();

    }
}
