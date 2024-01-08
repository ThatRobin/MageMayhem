using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "wheel", menuName = "Wheel")]
public class Wheel : ScriptableObject {

    public List<PriortyMove> moves;

}

[System.Serializable]
public class PriortyMove {

    [SerializeField]
    private BaseMove move;
    [SerializeField]
    private int priority = 10;

    public int getPriority() {
        return priority;
    }

    public BaseMove getMove() {
        return move;
    }
}