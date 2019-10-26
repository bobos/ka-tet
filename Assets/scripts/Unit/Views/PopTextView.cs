using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitNS
{
  public class PopTextView : MonoBehaviour {
  
    // Use this for initialization
    float timer = 0.0f;
    float textSpeed = 1.0f;
    UnitView unitView = null;
    void Start () { 
    }

    public void Show(UnitView unitView, string msg, Color color) {
      this.unitView = unitView;
      TextMesh textMesh = this.transform.GetComponent<TextMesh>();
      textMesh.text = msg;
      textMesh.color = color;
      //Vector3 p = unitView.transform.position;
      //transform.position = new Vector3(p.x, p.y + 1f, p.z);
    }
    
    // Update is called once per frame
    void Update () {
      transform.rotation = Camera.main.transform.rotation;
      timer += Time.deltaTime;
      transform.Translate(new Vector3(0, textSpeed * Time.deltaTime, 0));
      // last 1.5 sec
      if (timer > 1.5f) {
        unitView.Animating = false;
        Destroy(gameObject);
      }
    }
  }

}