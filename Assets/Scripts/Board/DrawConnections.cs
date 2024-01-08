using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DrawConnections : MonoBehaviour {

    public GameObject lineRendererObject;
    public List<ConnectableTile> tiles;

    void Start() {
        for (int x = 0; x < tiles.Count; x++) {
            for (int y = 0; y < tiles[x].getConnections().Count; y++) {
                GameObject tmp = Instantiate(lineRendererObject, this.transform);
                LineRenderer lineRenderer = tmp.GetComponent<LineRenderer>();
                Vector3 originalPos = tiles[x].getSelf().transform.position - new Vector3(0, 0.001f, 0);
                Vector3 newPos = tiles[x].getConnections()[y].transform.position - new Vector3(0, 0.001f, 0);
                lineRenderer.SetPosition(0, originalPos);
                lineRenderer.SetPosition(1, newPos);
            }
        }
        
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected() {
        for (int x = 0; x < tiles.Count; x++) {
            for (int y = 0; y < tiles[x].getConnections().Count; y++) {
                Vector3 originalPos = tiles[x].getSelf().transform.position - new Vector3(0, 0.0001f, 0);
                Vector3 newPos = tiles[x].getConnections()[y].transform.position - new Vector3(0, 0.0001f, 0);
                var thickness = 10;
                Handles.DrawBezier(originalPos, newPos, originalPos, newPos, Color.blue, null, thickness);
            }
        }
    }
#endif
}

[System.Serializable]
public class ConnectableTile {

    [SerializeField]
    private GameObject self;
    [SerializeField]
    private List<GameObject> connections = new List<GameObject>();

    public GameObject getSelf() {
        return self;
    }

    public List<GameObject> getConnections() {
        return connections;
    }

}