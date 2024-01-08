using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "spawn_figure_goal", menuName = "AI Goal/Spawn Figure Goal")]
public class SpawnFigureGoal : BaseGoal {

    public override void ExecuteGoal(FigureManager figureManager) {
        base.ExecuteGoal(figureManager);
        GameManager gameManager = GameManager.getInstance();
        List<GameObject> spawnPoints = gameManager.opponentSpawnPoints.FindAll((spawnPoint) => !spawnPoint.GetComponent<SpaceBehaviour>().IsOccupied());
        int randomNumber = Random.Range(0, spawnPoints.Count);
        GameObject spawnPoint = spawnPoints[randomNumber];

        figureManager.spaces.Add(spawnPoint);
        figureManager.targetSpace = spawnPoint;

        figureManager.StartCoroutine(figureManager.GoToSpace(GoToSpaceFromSpawn));

    }

    void GoToSpaceFromSpawn() {
        GameManager gameManager = GameManager.getInstance();
        List<GameObject> spaces = figureManager.targetSpace.GetComponent<SpaceBehaviour>().getValidSpaces(figureManager.figureData.moveAmount - 1, true);

        figureManager.spaces = new List<GameObject>();
        
        GameObject space = spaces[Random.Range(0, spaces.Count - 1)];

        InputHandler.SetAppearance(spaces, true, true);

        InputHandler.FindPath(figureManager, figureManager.standingSpace, space);

        figureManager.StartCoroutine(figureManager.GoToSpace(() => {
            InputHandler.ResetAppearance(gameManager);
            GameManager.SetOpponentMoved(true);
            GameManager.getInstance().EndTurn();
        }));
    }

}
