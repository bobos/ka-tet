using CourtNS;
using UnityEngine;

namespace UnitNS
{
  public class UnitNameView : MonoBehaviour {
  
    // Use this for initialization
    void Start () {
    }
  
    public void SetName(General general) {
      TextMesh textMesh = this.transform.GetComponent<TextMesh>();
      textMesh.text = general.Name();
      textMesh.color = Color.yellow;
      transform.rotation = Camera.main.transform.rotation;
    }
    
    // Update is called once per frame
    void Update () {
      transform.rotation = Camera.main.transform.rotation;
    }
  }

}