using System.Collections.Generic;
using UnityEngine;

namespace More_World_Locations_AIO;

public class PortTrader : MonoBehaviour
{
    public float m_standRange = 15f;
    public float m_greetRange = 5f;
    public float m_byeRange = 5f;
    public float m_hideDialogDelay = 5f;
    public float m_randomTalkInterval = 30f;
    public float m_dialogHeight = 1.5f;
    public static readonly List<string> m_randomTalk = new()
    {
        new LocalKeys.Key("$msg_randomPortTalk_1", "Ahh, another tall one wantin’ me ships? Don’t trip on the pier, human.").GetKey(),
        new LocalKeys.Key("$msg_randomPortTalk_2", "Coin first, cargo later. That’s the way of the sea—and the way o’ me beard!").GetKey(),
        new LocalKeys.Key("$msg_randomPortTalk_3", "Got goods to move? My boats are faster than a troll smell downwind.").GetKey(),
        new LocalKeys.Key("$msg_randomPortTalk_4", "Humans always want somethin’ shipped yesterday. Lucky for you, I run tomorrow’s boats today!").GetKey(),
        new LocalKeys.Key("$msg_randomPortTalk_5", "You pay, I haul. Simple as swingin’ a hammer, less noisy too.").GetKey(),
        new LocalKeys.Key("$msg_randomPortTalk_6", "Keep yer fingers off the rum barrels—those be for the crew, not the cargo!").GetKey(),
        new LocalKeys.Key("$msg_randomPortTalk_7", "By me beard, I’ve hauled heavier than you! Don’t fret, yer shipment’ll make it.").GetKey(),
        new LocalKeys.Key("$msg_randomPortTalk_8", "Ports, manifests, and coin. That’s my life. What’s yours, eh human?").GetKey(),
    };

    public static readonly List<string> m_randomGreets = new()
    {
        new LocalKeys.Key("$msg_randomGreet_1", "Well met, longshanks! Come to send yer shiny things across the seas?").GetKey(),
        new LocalKeys.Key("$msg_randomGreet_2", "Har! Another customer! Mind the ropes, they trip taller folk like you.").GetKey(),
        new LocalKeys.Key("$msg_randomGreet_3", "A fair wind blows, and so do fair deals—if ye got the coin.").GetKey(),
        new LocalKeys.Key("$msg_randomGreet_4", "By me beard, you look travel-worn. Lucky for ye, I deal in travel.").GetKey(),
        new LocalKeys.Key("$msg_randomGreet_5", "Step lively, human! Me dock’s no place for daydreamers.").GetKey(),
        new LocalKeys.Key("$msg_randomGreet_6", "Good day to ye! Got cargo? I got ships.").GetKey(),
        new LocalKeys.Key("$msg_randomGreet_7", "Ah, the smell of coin on the breeze. You’ve got business, eh?").GetKey(),
        new LocalKeys.Key("$msg_randomGreet_8", "Welcome to me port! Don’t touch nothin’ shiny, ‘less ye bought it.").GetKey(),
    };
    public static readonly List<string> m_randomGoodbye = new()
    {
        new LocalKeys.Key("$msg_randomGoodbye_1", "Fair winds, and don’t sink me profits!").GetKey(),
        new LocalKeys.Key("$msg_randomGoodbye_2", "Off with ye then—me ships won’t load themselves.").GetKey(),
        new LocalKeys.Key("$msg_randomGoodbye_3", "Safe travels, longshanks. Spend more coin next time!").GetKey(),
        new LocalKeys.Key("$msg_randomGoodbye_4", "Mind the seas—they’re rougher than me uncle after three ales.").GetKey(),
        new LocalKeys.Key("$msg_randomGoodbye_5", "Cargo waits for no one. Move along!").GetKey(),
        new LocalKeys.Key("$msg_randomGoodbye_6", "Till next tide, may yer boots stay dry.").GetKey(),
        new LocalKeys.Key("$msg_randomGoodbye_7", "Don’t be a stranger—or worse, a debtor.").GetKey(),
        new LocalKeys.Key("$msg_randomGoodbye_8", "Goodbye, and may yer beard grow as strong as yer purse.").GetKey(),
    };
    public EffectList m_randomTalkFX = new();
    public EffectList m_randomGreetFX = new();
    public EffectList m_randomGoodbyeFX = new();
    public bool m_didGreet;
    public bool m_didGoodbye;
    public Animator m_animator = null!;
    public LookAt m_lookAt = null!;
    private static readonly int stand = Animator.StringToHash("Stand");
    public void Start()
    {
        m_animator = GetComponentInChildren<Animator>();
        m_lookAt = GetComponentInChildren<LookAt>();
        InvokeRepeating(nameof(RandomTalk), m_randomTalkInterval, m_randomTalkInterval);
    }

    public void Update()
    {
        Player? closestPlayer = Player.GetClosestPlayer(transform.position, Mathf.Max(m_byeRange + 3f, m_standRange));
        if (closestPlayer)
        {
            float distance = Vector3.Distance(closestPlayer.transform.position, transform.position);
            if (distance < (double)m_standRange)
            {
                m_animator.SetBool(stand, true);
                m_lookAt.SetLoockAtTarget(closestPlayer.GetHeadPoint());
            }

            if (!m_didGreet && distance < (double)m_greetRange)
            {
                m_didGreet = true;
                Say(m_randomGreets, "Greet");
                m_randomGreetFX.Create(transform.position, Quaternion.identity);
            }

            if (!m_didGreet || m_didGoodbye || distance <= (double)m_byeRange)
                return;
            m_didGoodbye = true;
            Say(m_randomGoodbye, "Greet");
            m_randomGoodbyeFX.Create(transform.position, Quaternion.identity);
        }
        else
        {
            m_animator.SetBool(stand, false);
            m_lookAt.ResetTarget();
        }
    }

    public void RandomTalk()
    {
        if (!m_animator.GetBool(stand) || StoreGui.IsVisible() || !Player.IsPlayerInRange(transform.position, m_greetRange)) return;
        Say(m_randomTalk, "Talk");
        m_randomTalkFX.Create(transform.position, Quaternion.identity);
    }
    
    public void Say(List<string> texts, string trigger)
    {
        Say(texts[Random.Range(0, texts.Count)], trigger);
    }

    public void Say(string text, string trigger)
    {
        Chat.instance.SetNpcText(gameObject, Vector3.up * m_dialogHeight, 20f, m_hideDialogDelay, "", text, false);
        if (trigger.Length <= 0) return;
        m_animator.SetTrigger(trigger);
    }
}