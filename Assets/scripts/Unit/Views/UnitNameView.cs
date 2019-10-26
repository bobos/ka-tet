using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitNS
{
  public class UnitNameView : MonoBehaviour {
  
    // Use this for initialization
    void Start () {
    }
  
    public void SetName(string name) {
      TextMesh textMesh = this.transform.GetComponent<TextMesh>();
      textMesh.text = name;
      textMesh.color = Color.white;
    }
    
    // Update is called once per frame
    void Update () {
      transform.rotation = Camera.main.transform.rotation;
    }
  }

}