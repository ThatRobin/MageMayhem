using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InputHandler : MonoBehaviour {

    public FigureManager figureManager;
    public static InputHandler inputHandler;

    private void Awake() {
        InputHandler.inputHandler = this;
    }

    void Update() {
        if (GameManager.GetTurnStatus().Equals(TurnStatus.Player)) {
            HandlePlayerInput();
        }
    }

    public static InputHandler GetInstance() {
        return inputHandler;
    }

    void HandlePlayerInput() {
        if (Input.GetKey(KeyCode.Mouse0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                bool hasMoved = GameManager.HasPlayerMoved();
                bool hasAttacked = GameManager.HasPlayerAttacked();
                if (!hasMoved && !hasAttacked) {
                    if (figureManager != null) {
                        TryWalkToSpace(figureManager, hit.transform);
                        figureManager.DeselectFigureColour();
                    }
                }
                if (figureManager != null) {
                    TryAttackSpace(figureManager, hit.transform);
                    figureManager.DeselectFigureColour();
                }

                figureManager = SelectFigure(hit.transform, !hasMoved && !hasAttacked, !hasAttacked);
            }
        }
    }

    public static void TryWalkToSpace(FigureManager figureManager, Transform hit) {
        GameManager gameManager = GameManager.getInstance();
        if (hit.CompareTag("WalkableSpace")) {
            figureManager.spaces = new List<GameObject>();
            BenchBehaviour benchBehaviour = figureManager.standingSpace.GetComponent<BenchBehaviour>();
            if (benchBehaviour && benchBehaviour.IsOccupied()) {
                foreach (GameObject spawnPoint in gameManager.playerSpawnPoints) {
                    if (!spawnPoint.GetComponent<SpaceBehaviour>().IsOccupied()) {
                        List<GameObject> spaces = spawnPoint.GetComponent<SpaceBehaviour>().getValidSpaces(figureManager.figureData.moveAmount - 1, true);
                        foreach (GameObject space in spaces) {
                            if (space.name == hit.gameObject.name) {
                                figureManager.spaces.Add(spawnPoint);
                                figureManager.targetSpace = spawnPoint;
                            }
                        }
                    }
                }
            }
            figureManager.targetSpace = hit.gameObject;
            if (figureManager.spaces.Count > 0) {
                FindPath(figureManager, figureManager.spaces[0], hit.gameObject);
            } else {
                FindPath(figureManager, figureManager.standingSpace, hit.gameObject);
            }
            figureManager.StartCoroutine(figureManager.GoToSpace(() => {
                ResetAppearance(gameManager);
                if (GameManager.GetTurnStatus().Equals(TurnStatus.Player)) {
                    GameManager.SetPlayerMoved(true);
                    foreach (FigureManager figureManager in gameManager.allPlayerFigures) {
                        if (figureManager.standingSpace.name == gameManager.opponentHomeSpace.name) {
                            gameManager.WinGame();
                        }
                    }
                } else if (GameManager.GetTurnStatus().Equals(TurnStatus.Opponent)) {
                    GameManager.SetOpponentMoved(true);
                    GameManager.getInstance().EndTurn();
                }
            }));
        }
    }

    public static void TryAttackSpace(FigureManager figureManager, Transform hit) {
        GameManager gameManager = GameManager.getInstance();

        GameObject battle = gameManager.battle;
        if (hit.CompareTag("AttackableSpace")) {
            if (GameManager.GetTurnStatus().Equals(TurnStatus.Player)) {
                battle.SetActive(true);
                battle.GetComponent<WheelManager>().playerFigureManager = figureManager;
                battle.GetComponent<WheelManager>().opponentFigureManager = hit.GetComponent<SpaceBehaviour>().GetFigure().GetComponent<FigureManager>();
                battle.GetComponent<WheelManager>().SetupBattle(figureManager.figureData.figureWheel, battle.GetComponent<WheelManager>().opponentFigureManager.figureData.figureWheel, (result) => {
                    GameManager.SetPlayerAttacked(true);
                    WheelManager.HandleResults(result);
                    ResetAppearance(gameManager);
                });
                return;
            }
        }
        if (GameManager.GetTurnStatus().Equals(TurnStatus.Opponent)) {
            battle.SetActive(true);
            //Debug.Log(hit.name);
            battle.GetComponent<WheelManager>().playerFigureManager = hit.GetComponent<SpaceBehaviour>().GetFigure().GetComponent<FigureManager>();
            battle.GetComponent<WheelManager>().opponentFigureManager = figureManager;
            battle.GetComponent<WheelManager>().SetupBattle(battle.GetComponent<WheelManager>().playerFigureManager.figureData.figureWheel, figureManager.figureData.figureWheel, (result) => {
                GameManager.SetOpponentAttacked(true);
                WheelManager.HandleResults(result);
                ResetAppearance(gameManager);
            });
            return;
        }
    }

    public static FigureManager SelectFigure(Transform hit, bool highlightMoves, bool highlightAttackables) {
        GameManager gameManager = GameManager.getInstance();
        if ((hit.CompareTag("Player") && GameManager.GetTurnStatus().Equals(TurnStatus.Player))) {
            GameManager.PlaySelectFigure();
            ResetAppearance(gameManager);
            FigureManager figureManager = hit.GetComponent<FigureManager>();
            figureManager.SelectFigureColour();
            if (figureManager.outCount == 0) {
                if (figureManager != null) {
                    BenchBehaviour benchBehaviour = figureManager.standingSpace.GetComponent<BenchBehaviour>();
                    if (benchBehaviour != null) {
                        foreach (GameObject spawnPoint in gameManager.playerSpawnPoints) {
                            if (!spawnPoint.GetComponent<SpaceBehaviour>().IsOccupied()) {
                                List<GameObject> spaces = spawnPoint.GetComponent<SpaceBehaviour>().getValidSpaces(figureManager.figureData.moveAmount - 1, true);
                                SetAppearance(spaces, highlightMoves, highlightAttackables);
                            }
                        }

                    }
                    SpaceBehaviour spaceBehaviour = figureManager.standingSpace.GetComponent<SpaceBehaviour>();
                    if (spaceBehaviour != null) {
                        List<GameObject> spaces = spaceBehaviour.getValidSpaces(figureManager.figureData.moveAmount, false);
                        SetAppearance(spaces, highlightMoves, highlightAttackables);
                    }
                }
            }
            return figureManager;
        }
        if((hit.CompareTag("Opponent") && GameManager.GetTurnStatus().Equals(TurnStatus.Opponent))) {
            GameManager.PlaySelectFigure();
            ResetAppearance(gameManager);
            FigureManager figureManager = hit.GetComponent<FigureManager>();
            if (figureManager.outCount == 0) {
                if (figureManager != null) {
                    BenchBehaviour benchBehaviour = figureManager.standingSpace.GetComponent<BenchBehaviour>();
                    if (benchBehaviour != null) {
                        foreach (GameObject spawnPoint in gameManager.opponentSpawnPoints) {
                            if (!spawnPoint.GetComponent<SpaceBehaviour>().IsOccupied()) {
                                List<GameObject> spaces = spawnPoint.GetComponent<SpaceBehaviour>().getValidSpaces(figureManager.figureData.moveAmount - 1, true);
                                SetAppearance(spaces, highlightMoves, highlightAttackables);
                            }
                        }
                    }
                    SpaceBehaviour spaceBehaviour = figureManager.standingSpace.GetComponent<SpaceBehaviour>();
                    if (spaceBehaviour != null) {
                        List<GameObject> spaces = spaceBehaviour.getValidSpaces(figureManager.figureData.moveAmount, false);
                        SetAppearance(spaces, highlightMoves, highlightAttackables);
                    }
                }
            }
            return figureManager;
        }
        if (GetInstance().figureManager != null) {
            GameManager.PlayDeselectFigure();
            gameManager.allPlayerFigures.ForEach((figure) => figure.DeselectFigureColour());
            ResetAppearance(gameManager);
        }
        return null;
    }

    public static void SetAppearance(List<GameObject> spaces, bool highlightMoves, bool highlightAttackables, bool displayColours = true) {
        foreach (GameObject space in spaces) {
            if (highlightMoves) {
                space.tag = "WalkableSpace";
                if(displayColours) {
                    if (!space.GetComponent<SpaceBehaviour>().IsOccupied()) {
                        space.GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;
                    }
                }
            }
            if (highlightAttackables) {
                if (space.GetComponent<SpaceBehaviour>().IsOccupied()) {
                    string compTag;
                    if(GameManager.GetTurnStatus().Equals(TurnStatus.Player)) {
                        compTag = "Opponent";
                    } else {
                        compTag = "Player";
                    }
                    if (space.GetComponent<SpaceBehaviour>().GetFigure().CompareTag(compTag)) {
                        space.tag = "AttackableSpace";
                        if (displayColours) {
                            space.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
                        }
                    }
                }
            }
        }
    }
    public static void ResetAppearance(GameManager gameManager) {
        for (int i = 0; i < gameManager.allSpaces.Count; i++) {
            gameManager.allSpaces[i].GetComponent<SpaceBehaviour>().ResetColour();
            gameManager.allSpaces[i].tag = "Space";
        }
    }

    public static float GetDistance(GameObject nodeA, GameObject nodeB) {
        float dstX = Mathf.Abs(nodeA.transform.position.x - nodeB.transform.position.x);
        float dstY = Mathf.Abs(nodeA.transform.position.y - nodeB.transform.position.y);

        if (dstX > dstY) {
            return 14 * dstY + 10 * (dstX - dstY);
        }
        return 14 * dstX + 10 * (dstY - dstX);
    }

    public static List<GameObject> RetracePath(GameObject startNode, GameObject targetNode) {
        List<GameObject> path = new List<GameObject>();
        GameObject currentNode = targetNode;

        while (currentNode != startNode) {
            path.Add(currentNode);
            currentNode = currentNode.GetComponent<SpaceBehaviour>().parent;
        }

        path.Reverse();
        return path;
    }

    public static void FindPath(FigureManager figureManager, GameObject startNode, GameObject targetNode) {
        Queue<GameObject> openQueue = new Queue<GameObject>();
        Dictionary<GameObject, GameObject> parentMap = new Dictionary<GameObject, GameObject>();  // To store parent relationships for path retracing
        openQueue.Enqueue(startNode);

        while (openQueue.Count > 0) {
            GameObject currentNode = openQueue.Dequeue();

            if (currentNode == targetNode) {
                RetraceShortestPath(figureManager, startNode, targetNode, parentMap);
                return;
            }

            if (currentNode.GetComponent<SpaceBehaviour>()) {
                foreach (GameObject neighbour in currentNode.GetComponent<SpaceBehaviour>().connectedSpaces) {
                    if (parentMap.ContainsKey(neighbour) || openQueue.Contains(neighbour)) continue;

                    neighbour.GetComponent<SpaceBehaviour>().gCost = currentNode.GetComponent<SpaceBehaviour>().gCost + 1; // Assuming unit cost for simplicity

                    parentMap[neighbour] = currentNode;
                    openQueue.Enqueue(neighbour);
                }
            }
        }
    }

    private static void RetraceShortestPath(FigureManager figureManager, GameObject startNode, GameObject targetNode, Dictionary<GameObject, GameObject> parentMap) {
        List<GameObject> path = new List<GameObject>();
        GameObject currentNode = targetNode;

        while (currentNode != startNode) {
            path.Add(currentNode);
            currentNode = parentMap[currentNode];
        }

        path.Reverse();  // Reverse the path to get it from start to target

        List<GameObject> spaces = startNode.GetComponent<SpaceBehaviour>().getValidSpaces(figureManager.figureData.moveAmount, false);
        List<GameObject> shortestPath = new List<GameObject>();

        foreach (GameObject pathNode in path) {
            if (spaces.Contains(pathNode) && !pathNode.GetComponent<BasicSpace>().IsOccupied()) {
                shortestPath.Add(pathNode);
            } else {
                break;  // Stop adding nodes if an occupied or invalid space is encountered
            }
        }

        figureManager.spaces.AddRange(shortestPath);
    }

    /*
    public static void FindPath(FigureManager figureManager, GameObject startNode, GameObject targetNode) {
        Queue<GameObject> openQueue = new Queue<GameObject>();
        HashSet<GameObject> closedSet = new HashSet<GameObject>();
        openQueue.Enqueue(startNode);

        while (openQueue.Count > 0) {
            GameObject currentNode = openQueue.Dequeue();
            closedSet.Add(currentNode);

            if (currentNode == targetNode) {
                List<GameObject> spaces = startNode.GetComponent<SpaceBehaviour>().getValidSpaces(figureManager.figureData.moveAmount, false);
                List<GameObject> retracedPaths = RetracePath(startNode, targetNode);
                List<GameObject> sharedSpaces = new List<GameObject>();
                foreach (GameObject path in retracedPaths) {
                    if (spaces.Contains(path) && !path.GetComponent<BasicSpace>().IsOccupied()) {
                        sharedSpaces.Add(path);
                    }
                }
                figureManager.spaces.AddRange(sharedSpaces);
                return;
            }

            if (currentNode.GetComponent<SpaceBehaviour>()) {
                foreach (GameObject neighbour in currentNode.GetComponent<SpaceBehaviour>().connectedSpaces) {
                    if (closedSet.Contains(neighbour) || openQueue.Contains(neighbour)) continue;

                    neighbour.GetComponent<SpaceBehaviour>().gCost = currentNode.GetComponent<SpaceBehaviour>().gCost + 1; // Assuming unit cost for simplicity
                    neighbour.GetComponent<SpaceBehaviour>().hCost = GetDistance(neighbour, targetNode);
                    neighbour.GetComponent<SpaceBehaviour>().parent = currentNode;

                    openQueue.Enqueue(neighbour);
                }
            }
        }
    }


    public static void FindPath(FigureManager figureManager, GameObject startNode, GameObject targetNode) {
        List<GameObject> openSet = new List<GameObject>();
        HashSet<GameObject> closedSet = new HashSet<GameObject>();
        openSet.Add(startNode);

        while (openSet.Count > 0) {
            GameObject currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++) {
                if (openSet[i].GetComponent<SpaceBehaviour>().fCost < currentNode.GetComponent<SpaceBehaviour>().fCost || openSet[i].GetComponent<SpaceBehaviour>().fCost == currentNode.GetComponent<SpaceBehaviour>().fCost && openSet[i].GetComponent<SpaceBehaviour>().hCost < currentNode.GetComponent<SpaceBehaviour>().hCost) {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode) {
                List<GameObject> spaces = startNode.GetComponent<SpaceBehaviour>().getValidSpaces(figureManager.figureData.moveAmount, false);
                List<GameObject> retracedPaths = RetracePath(startNode, targetNode);
                List<GameObject> sharedSpaces = new List<GameObject>();
                foreach (GameObject path in retracedPaths) {
                    if (spaces.Contains(path) && !path.GetComponent<BasicSpace>().IsOccupied()) {
                        sharedSpaces.Add(path);
                    }
                }
                figureManager.spaces.AddRange(sharedSpaces);
                return;
            }

            if (currentNode.GetComponent<SpaceBehaviour>()) {
                foreach (GameObject neighbour in currentNode.GetComponent<SpaceBehaviour>().connectedSpaces) {
                    if (closedSet.Contains(neighbour)) continue;

                    float newMovementCostToNeighbour = currentNode.GetComponent<SpaceBehaviour>().gCost + GetDistance(currentNode, neighbour);
                    if (newMovementCostToNeighbour < neighbour.GetComponent<SpaceBehaviour>().gCost || !openSet.Contains(neighbour)) {
                        neighbour.GetComponent<SpaceBehaviour>().gCost = newMovementCostToNeighbour;
                        neighbour.GetComponent<SpaceBehaviour>().hCost = GetDistance(neighbour, targetNode);
                        neighbour.GetComponent<SpaceBehaviour>().parent = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                    }
                }
            }
        }
    }
   */

}
