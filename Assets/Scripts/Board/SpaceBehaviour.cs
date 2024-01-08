using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SpaceBehaviour : BasicSpace {

    public Color baseColour;

    public float gCost;
    public float hCost;
    public GameObject parent;
    public List<GameObject> connectedSpaces = new List<GameObject>();
    public float fCost {
        get {
            return gCost + hCost;
        }
    }

    public List<GameObject> getValidSpaces(int moves, bool includeSelf) {
        List<GameObject> validSpaces = new List<GameObject>();
        if (includeSelf) {
            validSpaces.Add(this.gameObject);
        }
        foreach (GameObject space in connectedSpaces) {
            if (space.GetComponent<SpaceBehaviour>().IsOccupied()) {
                if (GameManager.GetTurnStatus().Equals(TurnStatus.Player)) {
                    if (this.IsOccupied()) {
                        if(space.GetComponent<SpaceBehaviour>().GetFigure().CompareTag("Opponent")) {
                            validSpaces.Add(space);
                        }
                    }
                } else if (GameManager.GetTurnStatus().Equals(TurnStatus.Opponent)) {
                    if (space.GetComponent<SpaceBehaviour>().GetFigure().CompareTag("Player")) {
                        validSpaces.Add(space);
                    }
                }
                continue;
            } else {
                validSpaces.Add(space);
            }
            if (moves > 1) {
                foreach(GameObject foundSpace in space.GetComponent<SpaceBehaviour>().getValidSpaces(moves - 1, false)) {
                    if (foundSpace.name != this.name) {
                        validSpaces.Add(foundSpace);
                    }
                }
            }
        }
        return validSpaces;
    }

    public void ResetColour() {
        this.GetComponentInChildren<MeshRenderer>().material.color = baseColour;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected() {
        if(connectedSpaces.Count > 0) {
            for (int x = 0; x < connectedSpaces.Count; x++) {
                if(connectedSpaces[x] != null) {
                    Vector3 originalPos = this.transform.position - new Vector3(0, 0.0001f, 0);
                    Vector3 newPos = connectedSpaces[x].transform.position - new Vector3(0, 0.0001f, 0);
                    var thickness = 10;
                    Handles.DrawBezier(originalPos, newPos, originalPos, newPos, Color.blue, null, thickness);
                }
            }
        }
    }
#endif
}
