using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class GlobalLibrary
{
    public static GameObject player;
    public static RoundManager roundManager;
    public static GameObject[] explosion;

    public static GameObject describtorWindow;
    public static Text describtorText;
    public static GameObject windowMenu;

    public static GameObject explosionEffect;
    public static Text keyTipText;

    public static RawImage effectsIcons;
    public static Transform effectsParent;

    public static GameObject gameOverWindow;

    public static void Start(GameObject player, RoundManager roundManager, GameObject[] explosion, GameObject describtor, GameObject windowMenu, GameObject explosionEffect, Text keyTipText, RawImage effectsIcon, GameObject gameOverWindow)
    {
        GlobalLibrary.player = player;
        GlobalLibrary.roundManager = roundManager;
        GlobalLibrary.explosionEffect = explosionEffect;
        GlobalLibrary.describtorWindow = describtor;
        describtorText = describtor.transform.Find("Describtion").GetComponent<Text>();
        GlobalLibrary.windowMenu = windowMenu;
        GlobalLibrary.explosion = explosion;
        GlobalLibrary.keyTipText = keyTipText;
        GlobalLibrary.effectsIcons = effectsIcon;
        GlobalLibrary.effectsParent = effectsIcon.transform.parent;
        GlobalLibrary.gameOverWindow = gameOverWindow;
    }
}
