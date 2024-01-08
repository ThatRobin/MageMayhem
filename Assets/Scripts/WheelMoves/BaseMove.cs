using System;
using System.Linq;
using UnityEngine;

public enum MoveType {
    White,
    Gold,
    Purple,
    Blue,
    Red
}

[CreateAssetMenu(fileName = "move", menuName = "Move")]
public class BaseMove : ScriptableObject {

    public MoveType moveType;
    public string moveName;
    public int moveDamage;


    public Color getColour() {
        switch(moveType) {
            case MoveType.White:
                return Color.white;
            case MoveType.Gold:
                return new Color(252f / 255f, 148 / 255f, 3 / 255f);
            case MoveType.Purple:
                return new Color(116 / 255f, 21 / 255f, 138 / 255f);
            case MoveType.Blue:
                return new Color(118 / 255f, 171 / 255f, 219 / 255f);
            case MoveType.Red:
                return new Color(219 / 255f, 120 / 255f, 118 / 255f);
            default:
                return Color.white;
        }
    }

    public bool isDamageMove() {
        if (moveType.Equals(MoveType.White) || moveType.Equals(MoveType.Gold)) {
            return true;
        }
        return false;
    }

    public string getName() {
        return moveName;
    }

    public string getMainText() {
        if(isDamageMove()) {
            return moveDamage.ToString();
        } else if(moveType.Equals(MoveType.Purple)) {
            return String.Concat(Enumerable.Repeat("*", moveDamage));
        }
        return "";
    }
}
