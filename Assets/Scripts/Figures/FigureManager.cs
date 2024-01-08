using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FigureManager : MonoBehaviour {

    public FigureData figureData;
    public float speed;
    public int outCount = 0;
    public GameObject standingSpace;
    public GameObject targetSpace;
    public GameObject spawnPoint;

    public List<GameObject> spaces = new List<GameObject>();

    private void Awake() {
        this.spawnPoint = this.standingSpace;
        this.standingSpace.GetComponent<BasicSpace>().SetFigure(this.gameObject);
        spaces.Add(spawnPoint);
        targetSpace = spawnPoint;
        Color color = Color.blue;
        if(this.CompareTag("Opponent")) {
            color = Color.red;
        }
        this.transform.GetChild(0).GetChild(6).transform.GetComponent<MeshRenderer>().material.color = color;
        StartCoroutine(GoToSpace());
        if(this.CompareTag("Opponent")) {
            this.GetComponent<Collider>().enabled = false;
        }
    }

    public void SelectFigureColour() {
        Color color = Color.blue;
        if (this.CompareTag("Opponent")) {
            color = Color.red;
        }
        this.transform.GetChild(0).GetChild(6).transform.GetComponent<MeshRenderer>().material.color = Color.Lerp(color, Color.white, 0.5f);
    }

    public void DeselectFigureColour() {
        Color color = Color.blue;
        if (this.CompareTag("Opponent")) {
            color = Color.red;
        }
        this.transform.GetChild(0).GetChild(6).transform.GetComponent<MeshRenderer>().material.color = color;
    }

    public void Respawn() {
        spaces.Add(spawnPoint);
        targetSpace = spawnPoint;
        StartCoroutine(GoToSpace());
        outCount = 3;
    }


    public IEnumerator GoToSpace(Action postMove = null) {
        if (spaces.Count > 0) {
            SpaceBehaviour standingSpaceBehaviour = standingSpace.GetComponent<SpaceBehaviour>();
            BasicSpace basicSpace = standingSpace.GetComponent<BasicSpace>();
            if (standingSpaceBehaviour) {
                standingSpaceBehaviour.parent = null;
            }
            if (basicSpace) {
                basicSpace.SetFigure(null);
            }
            standingSpace = spaces[0];
            this.transform.LookAt(new Vector3(standingSpace.transform.position.x, this.transform.position.y, standingSpace.transform.position.z));
            standingSpaceBehaviour = standingSpace.GetComponent<SpaceBehaviour>();
            basicSpace = standingSpace.GetComponent<BasicSpace>();
            if (basicSpace) {
                basicSpace.SetFigure(this.gameObject);
            }
            while (Vector2.Distance(new Vector2(standingSpace.transform.position.x, standingSpace.transform.position.z), new Vector2(this.transform.position.x, this.transform.position.z)) > 0) {
                float step = speed * Time.deltaTime;

                yield return new WaitForSeconds(0.002f);

                this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(standingSpace.transform.position.x, this.transform.position.y, standingSpace.transform.position.z), step);
            }
            if (standingSpace.name != targetSpace.name) {
                spaces.RemoveAt(0);
                GameManager.PlayMoveFigure();
                yield return StartCoroutine(GoToSpace());
            } else {
                if (standingSpaceBehaviour != null) {
                    List<GameObject> surroundingSpaces = standingSpaceBehaviour.getValidSpaces(1, false);
                    if (GameManager.GetTurnStatus().Equals(TurnStatus.Player)) {
                        foreach (GameObject space in surroundingSpaces) {
                            if (space.GetComponent<SpaceBehaviour>().IsOccupied() && space.GetComponent<SpaceBehaviour>().GetFigure().CompareTag("Opponent")) {
                                List<GameObject> occupiedSpaces = space.GetComponent<SpaceBehaviour>().getValidSpaces(1, false);
                                if (occupiedSpaces.TrueForAll((occupiedSpace) => occupiedSpace.GetComponent<SpaceBehaviour>().IsOccupied() && occupiedSpace.GetComponent<SpaceBehaviour>().GetFigure().CompareTag("Player"))) {
                                    space.GetComponent<SpaceBehaviour>().GetFigure().GetComponent<FigureManager>().Respawn();
                                }
                            }
                        }
                    } else {
                        foreach (GameObject space in surroundingSpaces) {
                            if (space.GetComponent<SpaceBehaviour>().IsOccupied() && space.GetComponent<SpaceBehaviour>().GetFigure().CompareTag("Player")) {
                                List<GameObject> occupiedSpaces = space.GetComponent<SpaceBehaviour>().getValidSpaces(1, false);
                                if (occupiedSpaces.TrueForAll((occupiedSpace) => occupiedSpace.GetComponent<SpaceBehaviour>().IsOccupied() && occupiedSpace.GetComponent<SpaceBehaviour>().GetFigure().CompareTag("Opponent"))) {
                                    space.GetComponent<SpaceBehaviour>().GetFigure().GetComponent<FigureManager>().Respawn();
                                }
                            }
                        }
                    }
                }
            }
        }
        if (postMove != null) {
            postMove();
        }
    }

    
}
