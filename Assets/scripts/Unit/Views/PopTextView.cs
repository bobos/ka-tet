using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitNS
{
  public class PopTextView : MonoBehaviour {
  
    // Use this for initialization
    float timer = 0.0f;
    float textSpeed = 2.5f;
    View view = null;
    void Start () { 
    }

    public void Show(View view, string msg, Color color) {
      this.view = view;
      TextMesh textMesh = this.transform.GetComponent<TextMesh>();
      textMesh.text = msg;
      textMesh.color = color;
    }
    
    // Update is called once per frame
    void Update () {
      transform.rotation = Camera.main.transform.rotation;
      timer += Time.deltaTime;
      transform.Translate(new Vector3(0, textSpeed * Time.deltaTime, 0));
      // last 1 sec
      if (timer > 0.7f) {
        view.Animating = false;
        Destroy(gameObject);
      }
    }
  }

}