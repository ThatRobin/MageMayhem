using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "move_towards_home_goal", menuName = "AI Goal/Move Towards Home Goal")]
public class MoveTowardsHomeGoal : BaseGoal {

    public override void ExecuteGoal(FigureManager figureManager) {
        base.ExecuteGoal(figureManager);

        GameManager gameManager = GameManager.getInstance();

        InputHandler.SelectFigure(figureManager.transform, true, true);

        List<GameObject> spaces = figureManager.standingSpace.GetComponent<SpaceBehaviour>().getValidSpaces(figureManager.figureData.moveAmount, false);

        InputHandler.FindPath(figureManager, figureManager.standingSpace, gameManager.playerHomeSpace);

        figureManager.targetSpace = figureManager.spaces.Last();

        figureManager.StartCoroutine(figureManager.GoToSpace(() => {
            InputHandler.ResetAppearance(gameManager);
            if (GameManager.GetTurnStatus().Equals(TurnStatus.Player)) {
                GameManager.SetPlayerMoved(true);
            } else if (GameManager.GetTurnStatus().Equals(TurnStatus.Opponent)) {
                GameManager.SetOpponentMoved(true);
                GameManager.getInstance().EndTurn();
            }
        }));
        //InputHandler.TryWalkToSpace(figureManager, figureManager.targetSpace.transform);

        //InputHandler.ResetAppearance(gameManager);
    }
}
