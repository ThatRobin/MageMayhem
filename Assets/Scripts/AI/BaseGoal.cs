using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseGoal : ScriptableObject {

    public FigureManager figureManager;

    public virtual void ExecuteGoal(FigureManager figureManager) {
        this.figureManager = figureManager;
    }

}
