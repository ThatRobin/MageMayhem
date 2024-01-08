using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "move_towards_figure_goal", menuName = "AI Goal/Move Towards Figure Goal")]
public class MoveTowardsFigureGoal : BaseGoal {

    public override void ExecuteGoal(FigureManager figureManager) {
        base.ExecuteGoal(figureManager);

        GameManager gameManager = GameManager.getInstance();
        List<GameObject> spaces = figureManager.standingSpace.GetComponent<SpaceBehaviour>().getValidSpaces(figureManager.figureData.moveAmount, false);
        
        List<GameObject> boardSpaces = new List<GameObject>();
        foreach (GameObject space in spaces) {
            if (space.GetComponent<SpaceBehaviour>()) {
                if(space.GetComponent<SpaceBehaviour>().IsOccupied()) {
                    if(space.GetComponent<SpaceBehaviour>().GetFigure().CompareTag("Player")) {
                        boardSpaces.Add(space);
                    }
                }
            }
        }
        GameObject playerSpace = boardSpaces[Random.Range(0, boardSpaces.Count)];

        InputHandler.SelectFigure(figureManager.transform, true, true);
        
        InputHandler.FindPath(figureManager, figureManager.standingSpace, playerSpace);

        figureManager.StartCoroutine(figureManager.GoToSpace(() => {
            InputHandler.ResetAppearance(gameManager);
            GameManager.SetOpponentMoved(true);
            if (!figureManager.standingSpace.Equals(gameManager.playerHomeSpace)) {
                GameObject battle = gameManager.battle;
                battle.SetActive(true);
                battle.GetComponent<WheelManager>().playerFigureManager = playerSpace.transform.GetComponent<SpaceBehaviour>().GetFigure().GetComponent<FigureManager>();
                battle.GetComponent<WheelManager>().opponentFigureManager = figureManager;
                battle.GetComponent<WheelManager>().SetupBattle(battle.GetComponent<WheelManager>().playerFigureManager.figureData.figureWheel, figureManager.figureData.figureWheel, (result) => {
                    GameManager.SetOpponentAttacked(true);
                    WheelManager.HandleResults(result);
                    GameManager.getInstance().EndTurn();
                });
            }
            
        }));

        

        InputHandler.ResetAppearance(gameManager);
    }
}
