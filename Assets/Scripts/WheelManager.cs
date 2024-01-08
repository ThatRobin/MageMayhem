using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum WinStatus {
    Draw,
    Win,
    Lose
}

public class WheelManager : MonoBehaviour {

    public GameObject wheel;
    private GameObject playerWheelObject;
    private GameObject opponentWheelObject;
    public FigureManager playerFigureManager;
    public FigureManager opponentFigureManager;
    public Wheel playerWheel;
    public Wheel opponentWheel;
    float spinSpeed = 1f;
    float maxSpinSpeed = 5f;
    public Action<KeyValuePair<WinStatus, BaseMove>> postBattle;

    public Button endTurnButton;

    public GameObject resultScreen;
    public static WheelManager wheelManager;

    public bool battleActive;

    public void Awake() {
        WheelManager.wheelManager = this;
    }

    public static WheelManager GetWheelManager() {
        return WheelManager.wheelManager;
    }

    public void SetupBattle(Wheel player, Wheel opponent, Action<KeyValuePair<WinStatus, BaseMove>> postbattle = null) {
        endTurnButton.interactable = false;
        playerWheel = player;
        opponentWheel = opponent;
        postBattle = postbattle;


        Vector3 scale = this.GetComponent<RectTransform>().localScale;
        float radius = (wheel.GetComponent<RectTransform>().sizeDelta.x - 5) / 2f;
        float posX = radius * Mathf.Cos(45);
        float posY = radius * Mathf.Sin(45);

        playerWheelObject = Instantiate(wheel, this.transform.parent);
        RenderMoves playerMoves = playerWheelObject.GetComponent<RenderMoves>();
        playerMoves.wheel = playerWheel;
        playerMoves.RenderWheel();
        playerWheelObject.GetComponent<RectTransform>().localScale = scale;
        playerWheelObject.GetComponent<RectTransform>().localPosition = new Vector3(-posX * scale.x, -posY * scale.y, 0);
        playerWheelObject.name = "PlayerWheel";

        opponentWheelObject = Instantiate(wheel, this.transform.parent);
        RenderMoves opponentMoves = opponentWheelObject.GetComponent<RenderMoves>();
        opponentMoves.wheel = opponentWheel;
        opponentMoves.RenderWheel();
        opponentWheelObject.GetComponent<RectTransform>().localScale = scale;
        opponentWheelObject.GetComponent<RectTransform>().localPosition = new Vector3(posX * scale.x, posY * scale.y, 0);
        opponentWheelObject.name = "OpponentWheel";

        this.transform.SetAsLastSibling();

        Vector3 center = ((playerWheelObject.transform.position + opponentWheelObject.transform.position) * 0.5f);
        Vector3 targetDir = playerWheelObject.transform.position - opponentWheelObject.transform.position;
        transform.rotation = Quaternion.Euler(0, 0, Vector3.Angle(targetDir, new Vector3(targetDir.x, 0, 0)));
        transform.position = center;

        battleActive = true;
        spinSpeed = maxSpinSpeed;
    }

    private void FixedUpdate() {
        if (battleActive) {
            if(spinSpeed > 0f) {
                playerWheelObject.transform.Rotate(new Vector3(0, 0, spinSpeed));
                opponentWheelObject.transform.Rotate(new Vector3(0, 0, spinSpeed));
                spinSpeed -= 0.025f;
            }  else {
                battleActive = false;
                StartCoroutine(StartBattle());
            }
        }
    }

    public IEnumerator StartBattle() {
        float winZone = 211.68f;
        float playerZ = playerWheelObject.GetComponent<RectTransform>().localRotation.eulerAngles.z + winZone;
        float opponentZ = opponentWheelObject.GetComponent<RectTransform>().localRotation.eulerAngles.z + 180 + winZone;
        opponentZ = opponentZ > 360 ? opponentZ -= 360 : opponentZ;
        opponentZ = opponentZ < 0 ? opponentZ += 360 : opponentZ;
        playerZ = playerZ > 360 ? playerZ -= 360 : playerZ;
        playerZ = playerZ < 0 ? playerZ += 360 : playerZ;
        BaseMove playerMove = getMove(playerZ, playerWheelObject.GetComponent<RenderMoves>().wheel);
        BaseMove opponentMove = getMove(opponentZ, opponentWheelObject.GetComponent<RenderMoves>().wheel);
        BaseMove winningMove = WheelManager.CompareMoves(playerMove, opponentMove);

        yield return new WaitForSeconds(2f);

        KeyValuePair<WinStatus, BaseMove> winningSet = new KeyValuePair<WinStatus, BaseMove>();
        if (winningMove == null) {
            //Debug.Log("Draw between " + playerMove.moveName + " and " + opponentMove.moveName);
            winningSet = new KeyValuePair<WinStatus, BaseMove>(WinStatus.Draw, winningMove);
        } else if (winningMove.Equals(playerMove)) {
            //Debug.Log("Player wins with " + playerMove.moveName);
            winningSet = new KeyValuePair<WinStatus, BaseMove>(WinStatus.Win, winningMove);
        } else if (winningMove.Equals(opponentMove)) {
            //Debug.Log("Opponent wins with " + opponentMove.moveName);
            winningSet = new KeyValuePair<WinStatus, BaseMove>(WinStatus.Lose, winningMove);
        }
        yield return StartCoroutine(DetermineWinner(winningSet));
    }

    IEnumerator DetermineWinner(KeyValuePair<WinStatus, BaseMove> result) {
        if (!result.Key.Equals(WinStatus.Draw)) {
            if (result.Value.isDamageMove()) {
                if (result.Key.Equals(WinStatus.Win)) {
                    opponentFigureManager.Respawn();
                    GameManager.PlayBattleWin();
                } else if (result.Key.Equals(WinStatus.Lose)) {
                    playerFigureManager.Respawn();
                    GameManager.PlayBattleLose();
                }
            }
        } else {
            GameManager.PlayBattleDraw();
        }

        Destroy(playerWheelObject);
        Destroy(opponentWheelObject);

        if(postBattle != null) {
            postBattle(result);
        }

        yield return null;
    }

    public static void HandleResults(KeyValuePair<WinStatus, BaseMove> result) {
        WheelManager wheelManager = GetWheelManager();
        wheelManager.StartCoroutine(wheelManager.ShowResultScreen(result));
    }

    IEnumerator ShowResultScreen(KeyValuePair<WinStatus, BaseMove> result) {
        wheelManager.resultScreen.SetActive(true);
        wheelManager.GetComponent<Image>().enabled = false;
        GameObject winScreen = wheelManager.resultScreen.transform.GetChild(0).gameObject;
        GameObject loseScreen = wheelManager.resultScreen.transform.GetChild(1).gameObject;
        GameObject drawScreen = wheelManager.resultScreen.transform.GetChild(2).gameObject;
        if (result.Key == WinStatus.Win) {
            string screenText = "You won using " + result.Value.moveName + ".\n";
            if (result.Value != null && result.Value.isDamageMove()) {
                screenText += "The loser has been sent to the bench.";
            } else {
                screenText += "The loser has recieved the effect. (WIP, NOT YET IMPLEMENTED)";
            }
            winScreen.GetComponentInChildren<TMP_Text>().text = screenText;
            winScreen.SetActive(true);
            loseScreen.SetActive(false);
            drawScreen.SetActive(false);
        } else if (result.Key == WinStatus.Lose) {
            string screenText = "You lost against " + result.Value.moveName + ".\n";
            if (result.Value != null && result.Value.isDamageMove()) {
                screenText += "Your figure has been sent to the bench.";
            } else {
                screenText += "You have recieved the effect. (WIP, NOT YET IMPLEMENTED)";
            }
            loseScreen.GetComponentInChildren<TMP_Text>().text = screenText;
            loseScreen.SetActive(true);
            winScreen.SetActive(false);
            drawScreen.SetActive(false);
        } else if (result.Key == WinStatus.Draw) {
            loseScreen.GetComponentInChildren<TMP_Text>().text = "Its a draw! All figures remain where they were.";
            drawScreen.SetActive(true);
            winScreen.SetActive(false);
            loseScreen.SetActive(false);
        }
        yield return new WaitForSeconds(3f);
        wheelManager.resultScreen.SetActive(false);
        wheelManager.GetComponent<Image>().enabled = true;
        wheelManager.gameObject.SetActive(false);
        wheelManager.endTurnButton.interactable = true;
    }

    BaseMove getMove(float rotationOffset, Wheel wheel) {
        float totalPriority = 0;
        float iterPriority = 0;
        foreach (PriortyMove move in wheel.moves) {
            totalPriority += move.getPriority();
        }
        foreach (PriortyMove move in wheel.moves) {
            iterPriority += move.getPriority();
            if (((iterPriority / totalPriority) * 360) >= rotationOffset) {
                return move.getMove();
            }
        }
        return null;
    }

    private static BaseMove CompareMoves(BaseMove playerMove, BaseMove opponentMove) {
        if (playerMove != null && opponentMove != null) {
            if (playerMove.moveType.Equals(MoveType.Red) && opponentMove.moveType.Equals(MoveType.Red)) {
                return null;
            } else if (playerMove.moveType.Equals(MoveType.Red)) {
                return opponentMove;
            } else if (opponentMove.moveType.Equals(MoveType.Red)) {
                return playerMove;
            }
            if (playerMove.moveType.Equals(opponentMove.moveType)) {
                if (playerMove.moveDamage > opponentMove.moveDamage) {
                    return playerMove;
                } else if (playerMove.moveDamage < opponentMove.moveDamage) {
                    return opponentMove;
                } else {
                    return null;
                }
            } else if (playerMove.moveType.Equals(MoveType.White) && opponentMove.moveType.Equals(MoveType.Gold)) {
                if (playerMove.moveDamage > opponentMove.moveDamage) {
                    return playerMove;
                } else if (playerMove.moveDamage < opponentMove.moveDamage) {
                    return opponentMove;
                } else {
                    return null;
                }
            } else if (opponentMove.moveType.Equals(MoveType.White) && playerMove.moveType.Equals(MoveType.Gold)) {
                if (playerMove.moveDamage > opponentMove.moveDamage) {
                    return playerMove;
                } else if (playerMove.moveDamage < opponentMove.moveDamage) {
                    return opponentMove;
                } else {
                    return null;
                }
            } else {
                if (playerMove.moveType.Equals(MoveType.Blue)) {
                    return playerMove;
                }
                if (opponentMove.moveType.Equals(MoveType.Blue)) {
                    return opponentMove;
                }
                if (playerMove.moveType.Equals(MoveType.Purple)) {
                    if (opponentMove.moveType.Equals(MoveType.White)) {
                        return playerMove;
                    } else if (opponentMove.moveType.Equals(MoveType.Gold)) {
                        return opponentMove;
                    }
                }
                if (opponentMove.moveType.Equals(MoveType.Purple)) {
                    if (playerMove.moveType.Equals(MoveType.White)) {
                        return opponentMove;
                    } else if (playerMove.moveType.Equals(MoveType.Gold)) {
                        return playerMove;
                    }
                }
            }
        }
        return null;
    }

}
