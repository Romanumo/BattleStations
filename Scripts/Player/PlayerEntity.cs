using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class PlayerEntity : EntityBehaviour
{
    [Header("Player stats")]
    [SerializeField] PlayerStats player;
    [SerializeField] Spells[] spells;
    int chosenSpell = -1;
    Rigidbody controller;

    [HideInInspector] public bool isMagicBlocked = false;
    [HideInInspector] public bool isBasicBlocked = false;

    [Header("UI")]
    [SerializeField] RectTransform maxHealth;
    [SerializeField] RectTransform health;
    [SerializeField] RectTransform weaponChoser;

    private void Start()
    {
        controller = this.gameObject.GetComponent<Rigidbody>();
        player.stats.projectileStats = BulletFactory.Define<BulletStats, ProjectileEffect>(player.effect);
        foreach(Spells spell in spells)
        {
            spell.Start(this);
        }
    }

    void Update()
    {
        Aim();
        Controls();
        CheckHealth();
        SpellsUpdate();
        Movement();
    }

    #region UpdateMethods
    private void Controls()
    {
        if (Input.GetMouseButton(0))
        {
            if (isReloaded && !GeneralFunctions.isOverUI)
            {
                if (chosenSpell == -1 && !isBasicBlocked)
                    Shoot(player);
                else if (chosenSpell != -1 && !isMagicBlocked)
                    UseSpell();
            }
        }

        if(Input.GetKeyUp(KeyCode.Q))
        {
            GeneralFunctions.MenuWindowState(true);
        }
    }

    private void Aim()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        float angle = 0;
        if (Physics.Raycast(ray, out hit))
            angle = Quaternion.LookRotation(hit.point - this.transform.position).eulerAngles.y;
        this.transform.rotation = Quaternion.Euler(0, angle, 0);
    }

    private void Movement()
    {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (input != Vector3.zero)
            controller.AddForce(input.normalized * Time.deltaTime * player.stats.speed);
    }

    private void SpellsUpdate()
    {
        foreach (Spells spell in spells)
        {
            if (spell.isBought)
                spell.Update();
        }
    }

    public override void CheckHealth()
    {
        player.health += player.regen * Time.deltaTime * 5f;
        maxHealth.sizeDelta = new Vector2(player.maxHealth * 5, health.sizeDelta.y);
        health.sizeDelta = new Vector2(maxHealth.sizeDelta.x * ((float)((float)player.health / (float)player.maxHealth)) ,health.sizeDelta.y);

        if (player.health <= 0)
            GeneralFunctions.GameOver();
    }
    #endregion

    #region Spells
    private void UseSpell()
    {
        spells[chosenSpell].UseSpell();
        player.stats.projectileStats = BulletFactory.Define<SpellsStats, SpellType>(spells[chosenSpell].type);
        Shoot(player, spells[chosenSpell].spellObj);
    }

    public void UnlockSpell(SpellType type, Button button)
    {
        Spells spell = FindSpell(type);
        spell.isBought = true;
        spell.AssignOverloadTransfrom(button.transform.Find("OverloadMax").Find("Overload").GetComponent<RectTransform>());

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate () { chosenSpell = FindSpellIndex(type); weaponChoser.transform.position = button.transform.position; });
    }

    Spells FindSpell(SpellType type)
    {
        int index = FindSpellIndex(type);
        if (index == -1)
            return null;

        return spells[index];
    }

    int FindSpellIndex(SpellType type)
    {
        for (int i = 0; i < spells.Length; i++)
        {
            if (spells[i].type == type)
            {
                return i;
            }
        }
        return -1;
    }

    void ThroughSpellsAction(Action<Spells> action, bool isBoughtNeeded = true)
    {
        if (action == null)
            return;

        foreach (Spells spell in spells)
        {
            if (spell.isBought || !isBoughtNeeded)
            {
                action.Invoke(spell);
            }
        }
    }
    #endregion

    #region ForeignCode
    public PlayerStats GetStats()
    {
        return player;
    }

    public void BackToNormalWepon(Vector3 iconPos)
    {
        chosenSpell = -1;
        player.stats.projectileStats = BulletFactory.Define<BulletStats, ProjectileEffect>(ProjectileEffect.None);
        weaponChoser.transform.position = iconPos;
    }

    public override void ReceiveDamage(EntityStats sender) => player.ReceiveDamage(sender, this.transform.position);

    public void ReceiveOverload(int amount) => ThroughSpellsAction(delegate (Spells spell) { spell.AddOverload(amount); });

    public void ReduceOverloadGain(int amount) => ThroughSpellsAction(delegate (Spells spell) { spell.overloadGain = Mathf.Max(spell.overloadGain - amount, 7); }, false);
    #endregion
}

public enum SpellType { Fireball, Speeder, Freezer, DivideZero };
[System.Serializable]
public class Spells
{
    //It will get "On Hit" and "On Overload" in factory through string and reflection
    public string name;
    public SpellType type;
    public float maxOverload;
    public float overloadGain;
    public float overloadDecay;
    public GameObject spellObj;
    public bool isBought;

    RectTransform overloadTransform;
    float overload;
    float maxOverloadLength;
    PlayerEntity player;
    SpellsStats stats;

    public void Start(PlayerEntity player)
    {
        stats = BulletFactory.Define<SpellsStats, SpellType>(type);
        this.player = player;
    }

    public void AssignOverloadTransfrom(RectTransform transfrom)
    {
        overloadTransform = transfrom;
        maxOverloadLength = overloadTransform.parent.GetComponent<RectTransform>().sizeDelta.x;
    }

    public void UseSpell()
    {
        overload += overloadGain;
        if(overload > maxOverload)
        {
            if (stats.onOverload != null)
            {
                OnOverloadInfo info = new OnOverloadInfo(player.GetStats(), player, GlobalLibrary.player.transform.position);
                stats.onOverload.Invoke(info);
            }

            overload = 0;
        }
    }

    public void Update()
    {
        overload -= Time.deltaTime * overloadDecay;

        if(isBought && overloadTransform != null)
            overloadTransform.sizeDelta = new Vector2(maxOverloadLength * GetOverloadPercentage(), overloadTransform.sizeDelta.y);
    }

    public void AddOverload(int amount)
    {
        overload += amount;
    }

    public float GetOverloadPercentage()
    {
        return ((float)((float)overload / (float)maxOverload));
    }
}