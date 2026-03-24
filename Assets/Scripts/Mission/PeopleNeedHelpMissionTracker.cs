using System.Collections.Generic;
using UnityEngine;

public class PeopleNeedHelpMissionTracker : MonoBehaviour
{
    [SerializeField] private List<string> trackedMissionIds = new List<string> { "mission1", "mission2", "mission3", "mission4" };

    private readonly HashSet<string> completedMissionIds = new HashSet<string>();

    public void MarkMissionCompleted(string missionId)
    {
        string normalizedMissionId = NormalizeMissionId(missionId);
        if (string.IsNullOrEmpty(normalizedMissionId))
        {
            return;
        }

        completedMissionIds.Add(normalizedMissionId);
    }

    public bool IsMissionCompleted(string missionId)
    {
        string normalizedMissionId = NormalizeMissionId(missionId);
        if (string.IsNullOrEmpty(normalizedMissionId))
        {
            return false;
        }

        return completedMissionIds.Contains(normalizedMissionId);
    }

    public bool AreAllTrackedMissionsCompleted()
    {
        if (trackedMissionIds == null || trackedMissionIds.Count == 0)
        {
            return false;
        }

        int validMissionCount = 0;

        for (int i = 0; i < trackedMissionIds.Count; i++)
        {
            string normalizedMissionId = NormalizeMissionId(trackedMissionIds[i]);
            if (string.IsNullOrEmpty(normalizedMissionId))
            {
                continue;
            }

            validMissionCount++;

            if (!completedMissionIds.Contains(normalizedMissionId))
            {
                return false;
            }
        }

        return validMissionCount > 0;
    }

    private string NormalizeMissionId(string missionId)
    {
        return string.IsNullOrWhiteSpace(missionId) ? string.Empty : missionId.Trim().ToLowerInvariant();
    }
}
