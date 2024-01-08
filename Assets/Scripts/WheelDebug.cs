using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelDebug : MonoBehaviour
{

    public Wheel player;
    private RenderMoves wheelManager;
    // Start is called before the first frame update
    void Start()
    {
        wheelManager = this.GetComponent<RenderMoves>();
    }

    // Update is called once per frame
    public void ResetWheel() {
        if (this.transform.childCount > 0) {
            foreach (Transform child in this.transform) {
                Destroy(child.gameObject);
            }
        }
        if(player != null) {
            wheelManager.wheel = player;
            wheelManager.RenderWheel();
        }
    }
}
