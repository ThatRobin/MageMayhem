using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicSpace : MonoBehaviour {

    private GameObject figure;

    public bool IsOccupied() {
        return this.figure != null;
    }

    public void SetFigure(GameObject figure) {
        this.figure = figure;
    }

    public GameObject GetFigure() {
        return this.figure;
    }
}
