using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public enum ProjectileEffect { None, ArmorReducer, SpeedReducer, Overloader, MagicBlocker, BasicBlocker };
public static class BulletFactory
{
    const float minSpeed = 5000;

    //The old way of finding bullet stats by type. Decided to leave this. Newer and more optimal version in PerksIntrefaceClass
    public static Stats Define<Stats, Type>(Type type)
    {
        MethodInfo mi = typeof(BulletFactory).GetMethod(type.ToString());
        Stats buff = (Stats)mi.Invoke(typeof(BulletFactory), null);

        if (buff == null)
            Debug.LogWarning("Bullet of type: " + type + ", havent been founded");

        return buff;
    }

    #region BulletStats
    public static BulletStats ArmorReducer()
    {
        Action<OnHitInfo> onHit = delegate (OnHitInfo info)
        {
            if (info.target.stats.armor == 0)
                return;

            int temp = info.target.stats.armor;
            info.target.stats.armor = 0;
            GeneralFunctions.AddTimer(delegate () { info.target.stats.armor = temp; }, 15f);

            GeneralFunctions.KeyTipShow(2f, "Armor temporary nullified!");
            GeneralFunctions.AddEffectIcon(20f, ProjectileEffect.ArmorReducer);
        };

        return new BulletStats(onHit);
    }

    public static BulletStats Overloader()
    {
        Action<OnHitInfo> onHit = delegate (OnHitInfo info)
        {
            if (!(info.target is PlayerStats))
                return;

            PlayerStats player = (PlayerStats)info.target;
            player.ReceiveOverload(20);

            GeneralFunctions.KeyTipShow(2f, "Magic unstabilise!");
        };

        return new BulletStats(onHit);
    }

    public static BulletStats MagicBlocker()
    {
        Action<OnHitInfo> onHit = delegate (OnHitInfo info)
        {
            if (!(info.target is PlayerStats))
                return;

            PlayerStats player = (PlayerStats)info.target;
            player.MagicState(true);
            GeneralFunctions.AddTimer(delegate () { player.MagicState(false); }, 20f);

            GeneralFunctions.KeyTipShow(2f, "Magic blocked for 20 sec!");
            GeneralFunctions.AddEffectIcon(20f, ProjectileEffect.MagicBlocker);
        };

        return new BulletStats(onHit);
    }

    public static BulletStats BasicBlocker()
    {
        Action<OnHitInfo> onHit = delegate (OnHitInfo info)
        {
            if (!(info.target is PlayerStats))
                return;

            PlayerStats player = (PlayerStats)info.target;
            player.BasicState(true);
            GeneralFunctions.AddTimer(delegate () { player.BasicState(false); }, 20f);

            GeneralFunctions.KeyTipShow(2f, "Gun blocked for 20 sec!");
            GeneralFunctions.AddEffectIcon(20f, ProjectileEffect.BasicBlocker);
        };

        return new BulletStats(onHit);
    }

    public static BulletStats SpeedReducer()
    {
        return BulletFactory.SpeedReducerManual(500, 15f);
    }

    public static BulletStats SpeedReducerManual(int speedDecrease, float time)
    {
        Action<OnHitInfo> onHit = delegate (OnHitInfo info)
        {
            if (info.target.stats.speed < minSpeed)
                return;

            info.target.stats.speed -= speedDecrease;
            GeneralFunctions.AddTimer(delegate () { info.target.stats.speed += speedDecrease; }, time);

            GeneralFunctions.KeyTipShow(2f, "Speed temporary reduced!");
            GeneralFunctions.AddEffectIcon(time, ProjectileEffect.SpeedReducer);
        };

        return new BulletStats(onHit);
    }

    public static BulletStats None()
    {
        return new BulletStats(null);
    }
    #endregion

    #region SpellStats
    public static SpellsStats Fireball()
    {
        Action<OnHitInfo> onHit = delegate (OnHitInfo info)
        {
            PlayerStats player = (PlayerStats)info.sender;
            
            EntityStats explosionStats = new EntityStats(0, new EntityStatsStorage(player.magicAttackAdd * 2, 0, 0, 0, BulletFactory.None()));
            GeneralFunctions.AddExplosion(info.hitPos, explosionStats, (player.magicAttackAdd / 10f), GlobalLibrary.explosion[0]);
        };

        Action<OnOverloadInfo> onOverload = delegate (OnOverloadInfo info)
        {
            PlayerStats player = (PlayerStats)info.sender;
            EntityStats explosionStats = new EntityStats(0, new EntityStatsStorage(Mathf.RoundToInt(player.magicAttackAdd * 2.2f), 0, 0, 0, BulletFactory.None()));
            GeneralFunctions.AddExplosion(info.playerPos, explosionStats, (player.magicAttackAdd / 5f), GlobalLibrary.explosion[0]);

            GeneralFunctions.KeyTipShow(3f, "Fireball overload!");
            player.health += (int)player.overloadDefence * 10;
        };

        return new SpellsStats(onHit, onOverload);
    }

    public static SpellsStats Speeder()
    {
        Action<OnHitInfo> onHit = delegate (OnHitInfo info)
        {
            PlayerStats player = (PlayerStats)info.sender;
            int speedAdd = player.magicAttackAdd * 100;
            player.stats.speed += speedAdd;
            GeneralFunctions.AddTimer(delegate () { player.stats.speed -= speedAdd; }, 30f);
        };

        Action<OnOverloadInfo> onOverload = delegate (OnOverloadInfo info)
        {
            PlayerStats player = (PlayerStats)info.sender;
            int tempSpeed = player.stats.speed;
            player.stats.speed += (int)(tempSpeed * (player.magicAttackAdd / 30f));
            player.health -= 80 + (int)player.overloadDefence * 18;
            GeneralFunctions.AddTimer(delegate () { player.stats.speed = tempSpeed; }, 45f);

            GeneralFunctions.KeyTipShow(3f, "Speed overload!");
        };

        return new SpellsStats(onHit, onOverload);
    }

    public static SpellsStats Freezer()
    {
        Action<OnHitInfo> onHit = delegate (OnHitInfo info)
        {
            PlayerStats player = (PlayerStats)info.sender;
            EntityStats explosionStats = new EntityStats(0, new EntityStatsStorage(0, 0, 0, 0, BulletFactory.SpeedReducerManual(750, 30f)));
            GeneralFunctions.AddExplosion(info.hitPos, explosionStats, (player.magicAttackAdd / 10f), GlobalLibrary.explosion[1]);
        };

        Action<OnOverloadInfo> onOverload = delegate (OnOverloadInfo info)
        {
            PlayerStats player = (PlayerStats)info.sender;
            EntityStats explosionStats = new EntityStats(0, new EntityStatsStorage(0, (int)(10 - player.overloadDefence), 0, 0, BulletFactory.SpeedReducerManual(1800, 60f)));
            player.stats.speed += (int)player.overloadDefence * 650;
            GeneralFunctions.AddExplosion(info.playerPos, explosionStats, (player.magicAttackAdd / 5f), GlobalLibrary.explosion[1]);

            GeneralFunctions.KeyTipShow(3f, "Freeze spell overload!");
        };

        return new SpellsStats(onHit, onOverload);
    }

    public static SpellsStats DivideZero()
    {
        Action<OnHitInfo> onHit = delegate (OnHitInfo info)
        {
            info.target.health = 0;
        };

        Action<OnOverloadInfo> onOverload = delegate (OnOverloadInfo info)
        {
            PlayerStats player = (PlayerStats)info.sender;

            int tempArmor = player.stats.armor;
            float tempHealth = player.health - (20 * player.overloadDefence + 5f);
            player.stats.armor = int.MaxValue;
            EntityStats explosionStats = new EntityStats(0, new EntityStatsStorage(int.MaxValue - 1, 0, 0, 0, BulletFactory.None()));
            GeneralFunctions.AddExplosion(info.playerPos, explosionStats, (player.magicAttackAdd / 2f), GlobalLibrary.explosion[2]);
            GeneralFunctions.AddTimer(delegate () { player.stats.armor = tempArmor; }, 1f);

            float tempReload = player.stats.reload;
            player.stats.reload = int.MaxValue;
            player.health -= tempHealth;
            float timer = 50f / Mathf.Max(player.overloadDefence / 3, 1);
            GeneralFunctions.AddTimer(delegate () { player.stats.reload = tempReload; info.player.isReloaded = true; player.health += tempHealth; }, timer);

            GeneralFunctions.KeyTipShow(3f, "Hack spell overload! Weapon and spells are unavailable!");
            GeneralFunctions.AddEffectIcon(timer, ProjectileEffect.MagicBlocker);
            GeneralFunctions.AddEffectIcon(timer, ProjectileEffect.BasicBlocker);
        };

        return new SpellsStats(onHit, onOverload);
    }
    #endregion
}

public class BulletStats
{
    public Action<OnHitInfo> onHit;

    public BulletStats(Action<OnHitInfo> onHit)
    {
        this.onHit = onHit;
    }
}

public class SpellsStats : BulletStats
{
    public Action<OnOverloadInfo> onOverload;

    public SpellsStats(Action<OnHitInfo> onHit, Action<OnOverloadInfo> onOverload) : base (onHit)
    {
        this.onOverload = onOverload;
    }
}

public struct OnHitInfo
{
    public EntityStats sender;
    public EntityStats target;
    public Vector3 hitPos;

    public OnHitInfo(EntityStats sender, EntityStats target, Vector3 pos)
    {
        this.sender = sender;
        this.target = target;
        this.hitPos = pos;
    }
}

public struct OnOverloadInfo
{
    public PlayerEntity player;
    public EntityStats sender;
    public Vector3 playerPos;

    public OnOverloadInfo(EntityStats sender, PlayerEntity player, Vector3 pos)
    {
        this.player = player;
        this.sender = sender;
        this.playerPos = pos;
    }
}
