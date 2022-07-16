using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

public enum PerkType { Damage, Defence, Speed, Reload, OverloadDefence, Health, OverloadStabilise, MagicMight, Regeneration };
public static class PerksFactory
{
    static Dictionary<PerkType, PerkStats> perksList;

    public static void Start()
    {
        perksList = new Dictionary<PerkType, PerkStats>();
        foreach (PerkType perk in Enum.GetValues(typeof(PerkType)))
        {
            perksList.Add(perk, FindPerk(perk));
        }
    }

    public static PerkStats Define(PerkType type)
    {
        return perksList[type];
    }

    static PerkStats FindPerk(PerkType effect)
    {
        MethodInfo mi = typeof(PerksFactory).GetMethod(effect.ToString());
        PerkStats buff = (PerkStats)mi.Invoke(typeof(PerksFactory), null);

        if (buff == null)
            Debug.LogWarning("Perks type havent been founded");

        return buff;
    }

    public static PerkStats Damage()
    {
        Action<PlayerStats> onTaking = delegate (PlayerStats player)
        {
            player.stats.attack += 8;
        };
        return new PerkStats(onTaking);
    }

    public static PerkStats Defence()
    {
        Action<PlayerStats> onTaking = delegate (PlayerStats player)
        {
            player.stats.armor += 4;
        };
        return new PerkStats(onTaking);
    }

    public static PerkStats Speed()
    {
        Action<PlayerStats> onTaking = delegate (PlayerStats player)
        {
            player.stats.speed += 800;
        };
        return new PerkStats(onTaking);
    }

    public static PerkStats Reload()
    {
        Action<PlayerStats> onTaking = delegate (PlayerStats player)
        {
            player.stats.reload /= 1.25f;
        };
        return new PerkStats(onTaking);
    }

    public static PerkStats OverloadDefence()
    {
        Action<PlayerStats> onTaking = delegate (PlayerStats player)
        {
            player.overloadDefence++;
        };
        return new PerkStats(onTaking);
    }

    public static PerkStats Health()
    {
        Action<PlayerStats> onTaking = delegate (PlayerStats player)
        {
            player.maxHealth += 15;
            player.health += 30;
        };
        return new PerkStats(onTaking);
    }

    public static PerkStats OverloadStabilise()
    {
        Action<PlayerStats> onTaking = delegate (PlayerStats player)
        {
            player.ReduceOverloadGain(8);
            player.magicAttackAdd -= 2;
        };
        return new PerkStats(onTaking);
    }

    public static PerkStats MagicMight()
    {
        Action<PlayerStats> onTaking = delegate (PlayerStats player)
        {
            player.magicAttackAdd += 12;
        };
        return new PerkStats(onTaking);
    }

    public static PerkStats Regeneration()
    {
        Action<PlayerStats> onTaking = delegate (PlayerStats player)
        {
            player.regen += 0.015f;
        };
        return new PerkStats(onTaking);
    }
}

public class PerkStats
{
    public Action<PlayerStats> onTaking;

    public PerkStats(Action<PlayerStats> onTaking)
    {
        this.onTaking = onTaking;
    }
}
