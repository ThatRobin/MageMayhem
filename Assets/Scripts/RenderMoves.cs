using ntw.CurvedTextMeshPro;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RenderMoves : MonoBehaviour {

    public GameObject moveText;
    public GameObject move;
    public Wheel wheel;
    public TextMeshProUGUI textMeshProUGUI;
    private List<Image> charts = new List<Image>();
    private List<float> values = new List<float>();

    public void RenderWheel() {
        transform.eulerAngles = new Vector3(0,0,UnityEngine.Random.Range(0, 360));
        List<PriortyMove> moves = wheel.moves;
        for (int i = 0; i < moves.Count; i++) {
            GameObject objToSpawn = Instantiate(move, this.transform);
            Image img = objToSpawn.GetComponent<Image>();
            img.color = moves[i].getMove().getColour();
            charts.Add(img);
            values.Add(moves[i].getPriority());
            //objToSpawn = Instantiate(objToSpawn, this.transform);
            GameObject text = Instantiate(moveText, objToSpawn.transform);
            text.GetComponent<RectTransform>().position = Vector3.zero;
            TMP_Text tmpText = text.GetComponent<TMP_Text>();
            tmpText.color = Color.black;
            tmpText.text = moves[i].getMove().moveName;
            //tmpText.enabled = false;
            tmpText = text.transform.GetChild(0).GetComponent<TMP_Text>();
            tmpText.color = Color.black;
            tmpText.text = moves[i].getMove().getMainText();
        }
        SetValues(values);
    }

    void SetValues(List<float> valuesToSet) {
        float totalValues = 0;
        for (int i = 0; i < valuesToSet.Count; i++) {
            float percentage = findPercentage(valuesToSet, i);
            Transform moveTransform = this.transform.GetChild(i).GetChild(0);
            moveTransform.GetComponent<RectTransform>().localPosition = Vector3.zero;

            TMP_Text moveText = moveTransform.GetComponent<TMP_Text>();

            float arcDegrees = Math.Max(-87.1f, (percentage * -360) + 6);

            moveText.fontSize = -(float)CalculateFontSize(arcDegrees, moveText.text.Length);
            moveTransform.GetComponent<TextProOnACircle>().m_arcDegrees = arcDegrees;
            moveTransform.GetChild(0).GetComponent<TextProOnACircle>().m_arcDegrees = -(this.transform.GetChild(i).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text.Length / 0.07915567282f);
            moveTransform.GetComponent<TextProOnACircle>().m_angularOffset = -90 + (totalValues * 360) + ((percentage / 2f) * 360);
            moveTransform.GetChild(0).GetComponent<TextProOnACircle>().m_angularOffset = -90 + (totalValues * 360) + ((percentage / 2f) * 360);
            totalValues += percentage;
            this.transform.GetChild(i).GetComponent<Image>().fillAmount = totalValues;
        }
        List<Transform> tempObjs = new List<Transform>();
        for (int i = 0; i < this.transform.childCount; i++) {
            tempObjs.Add(this.transform.GetChild(i));
        }
        tempObjs.Sort((move, move2) => move.GetComponent<Image>().fillAmount.CompareTo(move2.GetComponent<Image>().fillAmount));
        for (int i = 0; i < tempObjs.Count; i++) {
            tempObjs[i].SetAsFirstSibling();
        }
    }

    static double CalculateFontSize(double arcDegrees, double lengthOfWord) {
        return arcDegrees / lengthOfWord;
    }

    float findPercentage(List<float> valueToSet, int index) {
        float totalAmount = 0;
        for (int i = 0; i < valueToSet.Count; i++) {
            totalAmount += valueToSet[i];
        }
        return valueToSet[index] / totalAmount;
    }
}
