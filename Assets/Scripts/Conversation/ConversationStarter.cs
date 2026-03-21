using System.Collections.Generic;
using DialogueEditor;
using UnityEngine;

public class ConversationStarter : MonoBehaviour
{
    [SerializeField] private NPCConversation myConversation;
    [SerializeField] private KeyCode interactKey = KeyCode.F;
    [SerializeField] private GameObject talkHintPopup;

    public NPCConversation DefaultConversation => myConversation;

    private IConversationOverrideProvider[] conversationOverrideProviders;

    private void Awake()
    {
        List<IConversationOverrideProvider> providers = new List<IConversationOverrideProvider>();
        MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IConversationOverrideProvider provider)
            {
                providers.Add(provider);
            }
        }

        conversationOverrideProviders = providers.ToArray();
    }

    private void Start()
    {
        SetHintVisible(false);
    }

    private void OnDisable()
    {
        SetHintVisible(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        SetHintVisible(CanStartConversation());
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (!CanStartConversation())
        {
            SetHintVisible(false);
            return;
        }

        SetHintVisible(true);

        if (Input.GetKeyDown(interactKey))
        {
            NPCConversation conversationToStart = GetConversationToStart();
            if (conversationToStart != null)
            {
                ConversationManager.Instance.StartConversation(conversationToStart);
                SetHintVisible(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        SetHintVisible(false);
    }

    private bool CanStartConversation()
    {
        return ConversationManager.Instance != null && !ConversationManager.Instance.IsConversationActive;
    }

    private void SetHintVisible(bool visible)
    {
        if (talkHintPopup == null)
        {
            return;
        }

        if (talkHintPopup.activeSelf != visible)
        {
            talkHintPopup.SetActive(visible);
        }
    }

    private NPCConversation GetConversationToStart()
    {
        foreach (IConversationOverrideProvider provider in conversationOverrideProviders)
        {
            if (provider != null && provider.TryGetConversationOverride(myConversation, out NPCConversation conversation) && conversation != null)
            {
                return conversation;
            }
        }

        return myConversation;
    }
}
