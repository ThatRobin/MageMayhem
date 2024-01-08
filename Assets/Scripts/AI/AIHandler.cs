using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIHandler : MonoBehaviour {

    public List<BaseGoal> goals = new List<BaseGoal>();
    public List<BaseGoal> options = new List<BaseGoal>();

    public void HandleOpponentTurn() {
        GameManager gameManager = GameManager.getInstance();

        List<FigureManager> canMoveToHome = new List<FigureManager>();
        options = new List<BaseGoal>();
        bool opponentHasNoBoardFigures = gameManager.allOpponentFigures.TrueForAll((figure) => figure.standingSpace.GetComponent<BenchBehaviour>());
        bool opponentHasNoBenchFigures = gameManager.allOpponentFigures.TrueForAll((figure) => figure.standingSpace.GetComponent<SpaceBehaviour>());
        bool areBothSpawnsOccupied = gameManager.opponentSpawnPoints.TrueForAll((space) => space.GetComponent<BasicSpace>().IsOccupied());
        bool playerHasNoBoardFigures = gameManager.allPlayerFigures.TrueForAll((figure) => figure.standingSpace.GetComponent<BenchBehaviour>());

        if (!areBothSpawnsOccupied && !opponentHasNoBenchFigures) {
            options.Add(goals[0]);
        }
        foreach (FigureManager figure in gameManager.allOpponentFigures) {
            if (figure.standingSpace.GetComponent<SpaceBehaviour>()) {
                figure.spaces = new List<GameObject>();
                InputHandler.FindPath(figure, figure.standingSpace, GameManager.getInstance().playerHomeSpace);
                
                if(figure.spaces.Count > 0) {
                    if (!options.Contains(goals[1])) {
                        options.Add(goals[1]);
                    }
                    canMoveToHome.Add(figure);
                }

            }
        }
        bool isPlayerInRange = false;
        foreach(FigureManager manager in gameManager.allOpponentFigures) {
            SpaceBehaviour spaceBehaviour = manager.standingSpace.GetComponent<SpaceBehaviour>();
            if(spaceBehaviour) {
                foreach(GameObject space in spaceBehaviour.getValidSpaces(manager.figureData.moveAmount, false)) {
                    if (space.GetComponent<BasicSpace>().IsOccupied() && space.GetComponent<BasicSpace>().GetFigure().CompareTag("Player")) {
                        isPlayerInRange = true;
                    }
                }
            }
        }
        if (isPlayerInRange) {
            options.Add(goals[2]);
        }
        int choice = Random.Range(0, options.Count);
        if(opponentHasNoBoardFigures && !areBothSpawnsOccupied) {
            choice = 0;
        }
        if (options[choice].Equals(goals[0])) { // spawn figure
            List<FigureManager> boardFigues = new List<FigureManager>();
            foreach (FigureManager figure in gameManager.allOpponentFigures) {
                if (figure.standingSpace.GetComponent<BenchBehaviour>()) {
                    boardFigues.Add(figure);
                }
            }
            if(boardFigues.Count == 0) {
                //HandleOpponentTurn();
            } else {
                FigureManager figureManager = boardFigues[Random.Range(0, boardFigues.Count - 1)];
                goals[0].ExecuteGoal(figureManager);
            }
        } else if (options[choice].Equals(goals[1])) { // Move figure towards home
            FigureManager figureManager = canMoveToHome[Random.Range(0, canMoveToHome.Count - 1)];
            goals[1].ExecuteGoal(figureManager);
        } else if (options[choice].Equals(goals[2])) { // Move figure to nearby player figure, and battle

            List<FigureManager> boardFigues = new List<FigureManager>();
            foreach (FigureManager manager in gameManager.allOpponentFigures) {
                SpaceBehaviour spaceBehaviour = manager.standingSpace.GetComponent<SpaceBehaviour>();
                if (spaceBehaviour) {
                    foreach (GameObject space in spaceBehaviour.getValidSpaces(manager.figureData.moveAmount, false)) {
                        if (space.GetComponent<BasicSpace>().IsOccupied() && space.GetComponent<BasicSpace>().GetFigure().CompareTag("Player")) {
                            if (!boardFigues.Contains(manager)) {
                                boardFigues.Add(manager);
                            }
                        }
                    }
                }
            }
            FigureManager figureManager = boardFigues[Random.Range(0, boardFigues.Count - 1)];
            
            goals[2].ExecuteGoal(figureManager);
        }
    }
}
