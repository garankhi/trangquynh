using DialogueEditor;
using UnityEngine;
using UnityEngine.UI;

public class PeopleNeedHelpBoardUI : MonoBehaviour
{
    [SerializeField] private GameObject boardPanel;
    [SerializeField] private bool hideBoardOnAwake = true;
    [SerializeField] private PeopleNeedHelpMissionTracker missionTracker;
    [SerializeField] private Image mission1Tick;
    [SerializeField] private Image mission2Tick;
    [SerializeField] private Image mission3Tick;

    private bool waitingForConversationEnd;

    private void Awake()
    {
        if (boardPanel == null)
        {
            boardPanel = gameObject;
        }

        if (hideBoardOnAwake)
        {
            SetBoardVisible(false);
        }

        RefreshTickVisuals();
    }

    private void OnDisable()
    {
        waitingForConversationEnd = false;
        ConversationManager.OnConversationEnded -= HandleConversationEnded;
    }

    public void ShowBoardAfterConversation()
    {
        if (!CanShowBoard())
        {
            HideBoard();
            return;
        }

        if (ConversationManager.Instance != null && ConversationManager.Instance.IsConversationActive)
        {
            waitingForConversationEnd = true;
            ConversationManager.OnConversationEnded -= HandleConversationEnded;
            ConversationManager.OnConversationEnded += HandleConversationEnded;
            return;
        }

        SetBoardVisible(true);
    }

    public void HideBoard()
    {
        waitingForConversationEnd = false;
        ConversationManager.OnConversationEnded -= HandleConversationEnded;
        SetBoardVisible(false);
    }

    public void MarkMissionCompleted(string missionId)
    {
        if (missionTracker == null)
        {
            return;
        }

        missionTracker.MarkMissionCompleted(missionId);
        RefreshTickVisuals();

        if (!CanShowBoard())
        {
            HideBoard();
        }
    }

    private void HandleConversationEnded()
    {
        if (!waitingForConversationEnd)
        {
            return;
        }

        waitingForConversationEnd = false;
        ConversationManager.OnConversationEnded -= HandleConversationEnded;

        if (!CanShowBoard())
        {
            HideBoard();
            return;
        }

        SetBoardVisible(true);
    }

    private bool CanShowBoard()
    {
        return missionTracker == null || !missionTracker.AreAllTrackedMissionsCompleted();
    }

    private void RefreshTickVisuals()
    {
        SetTickVisible(mission1Tick, IsMissionCompleted("mission1"));
        SetTickVisible(mission2Tick, IsMissionCompleted("mission2"));
        SetTickVisible(mission3Tick, IsMissionCompleted("mission3"));
    }

    private bool IsMissionCompleted(string missionId)
    {
        return missionTracker != null && missionTracker.IsMissionCompleted(missionId);
    }

    private void SetTickVisible(Image tick, bool isVisible)
    {
        if (tick != null)
        {
            tick.gameObject.SetActive(isVisible);
        }
    }

    private void SetBoardVisible(bool visible)
    {
        if (boardPanel != null && boardPanel.activeSelf != visible)
        {
            boardPanel.SetActive(visible);
        }
    }
}
