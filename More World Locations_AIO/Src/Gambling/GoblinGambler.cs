using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace More_World_Locations_AIO.Gambling;

/// <summary>
/// A gambling NPC that plays Knucklebones Duel with the player.
/// Attach to a cloned Goblin prefab to create an interactable gambler.
/// </summary>
public class GoblinGambler : MonoBehaviour, Interactable, Hoverable
{
    private const int BetAmount = 10;
    private const string CoinsPrefabName = "Coins";
    
    // Range settings
    private const float GreetRange = 8f;
    private const float StandRange = 15f;
    private const float IdleTalkInterval = 30f;
    
    private ZNetView m_nview;
    private Animator m_animator;
    private LookAt m_lookAt;
    
    // State tracking
    private bool m_didGreet;
    private float m_lastIdleTalk;
    
    // Dialog lists
    private static readonly List<string> GreetLines = new()
    {
        "Heh heh... got gold, tall one?",
        "Bones don't lie! Wanna play?",
        "Ten gold. You and me. Fate decides!",
        "Step up, step up! Try your luck!",
        "Ah, fresh meat... I mean, a customer!"
    };
    
    private static readonly List<string> IdleLines = new()
    {
        "*rattles bones*",
        "The bones grow restless...",
        "Come, come! Gold waits for no one!",
        "Highest roll wins. Simple, yes?",
        "Roll doubles, win big! Hehe..."
    };
    
    private static readonly List<string> WinLines = new()
    {
        "Gah! Lucky fool! Take it!",
        "Bah! The bones betray me!",
        "Fine, fine... here's your gold.",
        "Curse the Norns! You win!"
    };
    
    private static readonly List<string> LoseLines = new()
    {
        "HAHA! Mine now!",
        "The bones favor ME!",
        "Better luck never, human!",
        "Fate smiles on goblins today!"
    };
    
    private static readonly List<string> TieLines = new()
    {
        "A tie! House wins, hehe!",
        "Even bones, but I keep the gold!",
        "Fate says... I win ties!"
    };
    
    private static readonly List<string> DoublesLines = new()
    {
        "DOUBLES?! Curse these bones!",
        "By Odin's beard! Double fortune!",
        "The Norns mock me!"
    };
    
    private static readonly List<string> NoGoldLines = new()
    {
        "No gold? No game! Scram!",
        "Come back when you're not broke!",
        "Empty pockets, empty game!"
    };

    public void Awake()
    {
        // Get components from parent (the actual goblin) since we're on a child InteractPoint
        m_nview = GetComponentInParent<ZNetView>();
        m_animator = GetComponentInParent<Animator>();
        m_lookAt = GetComponentInParent<LookAt>();
    }

    public void Update()
    {
        // Use parent transform (the goblin) for position checks
        Transform goblinTransform = transform.parent != null ? transform.parent : transform;
        Player closestPlayer = Player.GetClosestPlayer(goblinTransform.position, StandRange);
        
        if (closestPlayer != null)
        {
            float distance = Vector3.Distance(closestPlayer.transform.position, goblinTransform.position);
            
            // Face the player
            if (m_lookAt != null)
            {
                m_lookAt.SetLoockAtTarget(closestPlayer.GetHeadPoint());
            }
            
            // Greet player when they get close
            if (!m_didGreet && distance < GreetRange)
            {
                m_didGreet = true;
                Say(GreetLines[Random.Range(0, GreetLines.Count)]);
            }
            
            // Idle chatter when player is nearby
            if (distance < GreetRange && Time.time - m_lastIdleTalk > IdleTalkInterval)
            {
                m_lastIdleTalk = Time.time;
                Say(IdleLines[Random.Range(0, IdleLines.Count)]);
            }
        }
        else
        {
            // Reset greet when player leaves
            m_didGreet = false;
            if (m_lookAt != null)
            {
                m_lookAt.ResetTarget();
            }
        }
    }

    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        if (hold) return false;
        
        Player player = user as Player;
        if (player == null) return false;
        
        Inventory inventory = player.GetInventory();
        
        // Get coins by shared name (localized) - "$item_coins"
        int playerGold = inventory.CountItems("$item_coins");
        
        // Check if player has enough gold
        if (playerGold < BetAmount)
        {
            Say(NoGoldLines[Random.Range(0, NoGoldLines.Count)]);
            return false;
        }
        
        // Take the bet - use shared name
        inventory.RemoveItem("$item_coins", BetAmount);
        
        // Roll the bones!
        int playerDie1 = Random.Range(1, 7);
        int playerDie2 = Random.Range(1, 7);
        int goblinDie1 = Random.Range(1, 7);
        int goblinDie2 = Random.Range(1, 7);
        
        int playerTotal = playerDie1 + playerDie2;
        int goblinTotal = goblinDie1 + goblinDie2;
        bool playerDoubles = playerDie1 == playerDie2;
        
        // Show the roll
        string rollMessage = $"You: {playerDie1}+{playerDie2}={playerTotal}  Goblin: {goblinDie1}+{goblinDie2}={goblinTotal}";
        player.Message(MessageHud.MessageType.Center, rollMessage);
        
        // Determine outcome
        int payout = 0;
        string resultLine;
        
        if (playerDoubles)
        {
            // Doubles = automatic win + 2x
            payout = BetAmount * 2;
            resultLine = DoublesLines[Random.Range(0, DoublesLines.Count)];
        }
        else if (playerTotal > goblinTotal)
        {
            // Player wins
            payout = BetAmount * 2;
            resultLine = WinLines[Random.Range(0, WinLines.Count)];
        }
        else if (playerTotal == goblinTotal)
        {
            // Tie = goblin wins (house edge)
            payout = 0;
            resultLine = TieLines[Random.Range(0, TieLines.Count)];
        }
        else
        {
            // Goblin wins
            payout = 0;
            resultLine = LoseLines[Random.Range(0, LoseLines.Count)];
        }
        
        // Pay out winnings
        if (payout > 0)
        {
            GameObject coinPrefab = PrefabManager.Instance.GetPrefab(CoinsPrefabName);
            if (coinPrefab != null)
            {
                inventory.AddItem(coinPrefab, payout);
                player.Message(MessageHud.MessageType.TopLeft, $"Won {payout} gold!");
            }
        }
        else
        {
            player.Message(MessageHud.MessageType.TopLeft, $"Lost {BetAmount} gold!");
        }
        
        Say(resultLine);
        return true;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item)
    {
        return false;
    }

    public string GetHoverText()
    {
        return Localization.instance.Localize(
            "$mwl_goblin_gambler\n" +
            $"[<color=yellow><b>$KEY_Use</b></color>] Gamble ({BetAmount} gold)");
    }

    public string GetHoverName()
    {
        return Localization.instance.Localize("$mwl_goblin_gambler");
    }
    
    private void Say(string text)
    {
        Chat.instance.SetNpcText(gameObject, Vector3.up * 2f, 20f, 5f, "", text, false);
        
        if (m_animator != null)
        {
            m_animator.SetTrigger("alerted");
        }
    }
}
