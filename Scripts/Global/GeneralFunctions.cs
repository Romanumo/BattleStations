using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class GeneralFunctions
{
    public static bool isOverUI = false;
    static List<Timer> timers;

    public static List<GameObject> effects;
    static ProjectileEffectManager effectProfileManager;

    public const float halfScreen = 920f;
    const float effectIconsOffset = 120f;

    public static void Start()
    {
        PerksFactory.Start();
        timers = new List<Timer>();
        effects = new List<GameObject>();
        effectProfileManager = GameObject.FindObjectOfType<ProjectileEffectManager>();
    }

    public static void Update()
    {
        TimersUpdate();
    }

    #region Timers
    public static void AddTimer(Action onFinished, float timer)
    {
        timers.Add(new Timer(onFinished, timer));
    }

    public static void AddProgressiveTimer(Action onFinished, Action<float> onUpdate, float timer)
    {
        timers.Add(new ProgressiveTimer(onFinished, timer, onUpdate));
    }

    public static void AddProgressiveTimer(Action onFinished, Action onUpdate, float timer)
    {
        timers.Add(new ProgressiveTimer(onFinished, timer, onUpdate));
    }

    public static void RemoveTimer(Timer timer)
    {
        timers.Remove(timer);
    }

    static void TimersUpdate()
    {
        if (timers.Count > 0)
        {
            for (int i = 0; i < timers.Count; i++)
            {
                if (timers[i] != null)
                    timers[i].Update();
            }
        }
    }
    #endregion

    #region UI
    public static void AddEffectIcon(float duration, ProjectileEffect effect)
    {
        GameObject effectIcon = GameObject.Instantiate(GlobalLibrary.effectsIcons.gameObject, GlobalLibrary.effectsParent.position + new Vector3(effectIconsOffset * effects.Count + GlobalLibrary.effectsParent.position.x, 0, 0), new Quaternion());

        effectIcon.transform.parent = GlobalLibrary.effectsParent;
        effects.Add(effectIcon);
        effectIcon.GetComponent<RawImage>().texture = effectProfileManager.FindProfile(effect).icon;

        //Offset all icons in the right to the left, when this effect dissapears
        AddTimer(delegate ()
        {
            for (int i = effects.IndexOf(effectIcon); i < effects.Count; i++)
            {
                effects[i].transform.position -= new Vector3(effectIconsOffset, 0, 0);
            }
            effects.Remove(effectIcon);
            UnityEngine.Object.Destroy(effectIcon);
        }, duration);
    }

    public static void GameOver()
    {
        Time.timeScale = 0;
        GlobalLibrary.gameOverWindow.SetActive(true);
        GlobalLibrary.player.GetComponent<PlayerEntity>().enabled = false;
    }

    public static void ShowDescribtion(string describtion)
    {
        GlobalLibrary.describtorWindow.SetActive(true);
        GlobalLibrary.describtorText.text = describtion;
        GlobalLibrary.describtorWindow.transform.position = Input.mousePosition;
        if (Input.mousePosition.x > GameObject.FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.width - halfScreen)
            GlobalLibrary.describtorWindow.transform.position -= new Vector3(GlobalLibrary.describtorWindow.GetComponent<RectTransform>().rect.width, 0, 0);
        isOverUI = true;
    }

    public static void HideDescribtion()
    {
        GlobalLibrary.describtorWindow.SetActive(false);
        isOverUI = false;
    }

    public static void MenuWindowState(bool state)
    {
        Time.timeScale = (state) ? 0 : 1;
        GlobalLibrary.windowMenu.SetActive(state);
    }

    public static void KeyTipShow(float time, string text)
    {
        Text keyTip = GlobalLibrary.keyTipText;
        ChangeTextAlpha(keyTip, 1);
        keyTip.gameObject.SetActive(true);
        keyTip.text = text;

        AddProgressiveTimer(delegate () { keyTip.gameObject.SetActive(false); },
            delegate (float timer) { ChangeTextAlpha(keyTip, timer); }, time);
    }

    static void ChangeTextAlpha(Text text, float a)
    {
        text.material.color = new Color(text.material.color.r, text.material.color.g, text.material.color.b, a);
    }
    #endregion

    public static void AddExplosionEffect(Vector3 position)
    {
        GameObject explosion = GameObject.Instantiate(GlobalLibrary.explosionEffect, position, new Quaternion());
        AddTimer(delegate () { UnityEngine.Object.Destroy(explosion); }, 3f);
    }

    public static void AddExplosion(Vector3 position, EntityStats stats, float radius, GameObject explosionPrototype)
    {
        ExplosionProjectile explosion = GameObject.Instantiate(explosionPrototype, Vector3.zero, new Quaternion()).GetComponent<ExplosionProjectile>();
        GameObject explosionObj = explosion.gameObject;

        explosion.SetExplosion(stats, position, radius);
        AddTimer(delegate () { if(explosionObj != null) UnityEngine.Object.Destroy(explosionObj); }, 3f);
    }
}

public class Timer
{
    protected Action onFinished;
    protected float timer;

    public Timer(Action onFinished, float maxTimer)
    {
        this.onFinished = onFinished;
        timer = maxTimer;
    }

    public virtual void Update()
    {
        timer -= Time.deltaTime;
        if(timer < 0)
        {
            if (onFinished != null)
            {
                onFinished.Invoke();
                GeneralFunctions.RemoveTimer(this);
            }
        }
    }
}

public class ProgressiveTimer : Timer
{
    Action<float> onUpdate;
    float maxTimer;

    public ProgressiveTimer(Action onFinished, float maxTimer, Action<float> onUpdate) : base(onFinished, maxTimer)
    {
        this.onUpdate = onUpdate;
        this.maxTimer = maxTimer;
    }

    public ProgressiveTimer(Action onFinished, float maxTimer, Action onUpdate) : base(onFinished, maxTimer)
    {
        this.onUpdate = delegate (float timer) { onUpdate(); };
        this.maxTimer = maxTimer;
    }

    public override void Update()
    {
        base.Update();

        if (onUpdate != null)
            onUpdate.Invoke((float)((float)timer / (float)maxTimer));
    }
}

