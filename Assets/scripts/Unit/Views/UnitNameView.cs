using CourtNS;
using UnityEngine;

namespace UnitNS
{
  public class UnitNameView : MonoBehaviour {
  
    // Use this for initialization
    void Start () {
    }
  
    public void SetNameColor(string name, Color color, int size) {
      TextMesh textMesh = this.transform.GetComponent<TextMesh>();
      textMesh.text = name;
      textMesh.color = color;
      textMesh.fontSize = size;
      transform.rotation = Camera.main.transform.rotation;
    }
    
    // Update is called once per frame
    void Update () {
      transform.rotation = Camera.main.transform.rotation;
    }
  }

}