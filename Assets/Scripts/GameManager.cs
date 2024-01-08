using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TurnStatus {
    Player,
    Opponent
}

public class GameManager : MonoBehaviour {

    public TurnStatus visualTurnStatus = TurnStatus.Player;

    private static TurnStatus turnStatus = TurnStatus.Player;
    public bool hasPlayerMoved = false;
    public bool hasPlayerAttacked = false;
    private static bool hasOpponentMoved = false;
    private static bool hasOpponentAttacked = false;
    private static bool isGameOver = false;
    public GameObject opponent;

    public GameObject opponentHomeSpace;
    public GameObject playerHomeSpace;

    public List<GameObject> allSpaces = new List<GameObject>();

    public List<GameObject> playerSpawnPoints = new List<GameObject>();
    public List<GameObject> opponentSpawnPoints = new List<GameObject>();

    public List<FigureManager> allPlayerFigures = new List<FigureManager>();
    public List<FigureManager> allOpponentFigures = new List<FigureManager>();
    
    public GameObject battle;
    public GameObject battleResultScreen;
    public GameObject winScreen;
    public GameObject loseScreen;

    public static GameManager gameManager;
    public Button endTurnButton;

    public static GameManager getInstance() {
        return gameManager;
    }

    private AudioSource audio;
    public AudioClip firstTurnClip;
    public AudioClip startTurnClip;
    public AudioClip nextTurnClip;
    public AudioClip endTurnClip;
    public AudioClip winClip;
    public AudioClip loseClip;
    public AudioClip selectFigureClip;
    public AudioClip deselectFigureClip;
    public AudioClip moveFigureClip;
    public AudioClip battleDrawClip;
    public AudioClip battleWinClip;
    public AudioClip battleLoseClip;

    private void Awake() {
        audio = this.GetComponent<AudioSource>();
        foreach(GameObject gmObj in allSpaces) {
            gmObj.GetComponent<SpaceBehaviour>().ResetColour();
        }
        GameManager.gameManager = this;
        OnTurnStart(turnStatus);
    }

    public void WinGame() {
        winScreen.SetActive(true);
        audio.clip = winClip;
        audio.Play();
        isGameOver = true;
    }

    public void LoseGame() {
        loseScreen.SetActive(true);
        audio.clip = loseClip;
        audio.Play();
        isGameOver = true;
    }

    public void EndTurn() {
        if (turnStatus.Equals(TurnStatus.Player)) {
            SetPlayerAttacked(false);
            OnTurnEnd(turnStatus);
            if (!isGameOver) {
                turnStatus = TurnStatus.Opponent;
                visualTurnStatus = TurnStatus.Opponent;
                OnTurnStart(turnStatus);
                opponent.GetComponent<AIHandler>().HandleOpponentTurn();
            }
        } else if (turnStatus.Equals(TurnStatus.Opponent)) {
            OnTurnEnd(turnStatus);
            turnStatus = TurnStatus.Player;
            visualTurnStatus = TurnStatus.Player;
            OnTurnStart(turnStatus); 
        }
    }
    
    public void OnTurnStart(TurnStatus turnStatus) {
        audio.clip = startTurnClip;
        audio.Play();
        GameManager.SetPlayerMoved(false);
        if (turnStatus.Equals(TurnStatus.Opponent)) {
            bool isSpawnLocked = opponentSpawnPoints.TrueForAll((spawnPoint) => {
                if(spawnPoint.GetComponent<SpaceBehaviour>().IsOccupied()) {
                    return allPlayerFigures.Contains(spawnPoint.GetComponent<SpaceBehaviour>().GetFigure().GetComponent<FigureManager>());
                }
                return false;
            });
            bool hasBoardFigures = allOpponentFigures.Count > 0 && allOpponentFigures.TrueForAll((figure) => figure.standingSpace.GetComponent<SpaceBehaviour>());
            if(isSpawnLocked && !hasBoardFigures) {
                WinGame();
            }
        }
    }

    public void OnTurnEnd(TurnStatus turnStatus) {
        audio.clip = endTurnClip;
        audio.Play();
        if (turnStatus.Equals(TurnStatus.Player)) {
            endTurnButton.interactable = false;
            foreach (FigureManager figureManager in allPlayerFigures) {
                if(figureManager.standingSpace.name == opponentHomeSpace.name) {
                    WinGame();
                }
                if (figureManager.outCount > 0) {
                    figureManager.outCount -= 1;
                }
            }
        } else if (turnStatus.Equals(TurnStatus.Opponent)) {
            endTurnButton.interactable = true;
            foreach (FigureManager figureManager in allOpponentFigures) {
                if (figureManager.standingSpace.name == playerHomeSpace.name) {
                    LoseGame();
                }
                if (figureManager.outCount > 0) {
                    figureManager.outCount -= 1;
                }
            }
        }
    }

    public static TurnStatus GetTurnStatus() {
        return turnStatus;
    }

    public static bool HasPlayerMoved() {
        return getInstance().hasPlayerMoved;
    }

    public static bool HasPlayerAttacked() {
        return getInstance().hasPlayerAttacked;
    }

    public static void SetPlayerMoved(bool value) {
        getInstance().hasPlayerMoved = value;
    }

    public static void SetPlayerAttacked(bool value) {
        getInstance().hasPlayerAttacked = value;
    }

    public static bool HasOpponentMoved() {
        return hasOpponentMoved;
    }

    public static bool HasOpponentAttacked() {
        return hasOpponentAttacked;
    }

    public static void SetOpponentMoved(bool value) {
        hasOpponentMoved = value;
    }

    public static void SetOpponentAttacked(bool value) {
        hasOpponentAttacked = value;
    }

    public static void PlaySelectFigure() {
        GameManager manager = getInstance();
        manager.audio.clip = manager.selectFigureClip;
        manager.audio.Play();
    }

    public static void PlayDeselectFigure() {
        GameManager manager = getInstance();
        manager.audio.clip = manager.deselectFigureClip;
        manager.audio.Play();
    }

    public static void PlayBattleDraw() {
        GameManager manager = getInstance();
        manager.audio.clip = manager.battleDrawClip;
        manager.audio.Play();
    }

    public static void PlayBattleWin() {
        GameManager manager = getInstance();
        manager.audio.clip = manager.battleWinClip;
        manager.audio.Play();
    }

    public static void PlayBattleLose() {
        GameManager manager = getInstance();
        manager.audio.clip = manager.battleLoseClip;
        manager.audio.Play();
    }

    public static void PlayMoveFigure() {
        GameManager manager = getInstance();
        manager.audio.clip = manager.moveFigureClip;
        manager.audio.Play();
    }
}
