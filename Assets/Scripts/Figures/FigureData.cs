using UnityEngine;

[CreateAssetMenu(fileName = "figure", menuName = "Figure")]
public class FigureData : ScriptableObject {

    public string figureAmount;
    public Wheel figureWheel;
    public int moveAmount;

}
