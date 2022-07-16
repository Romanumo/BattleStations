using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalFunctionManager : MonoBehaviour
{
    [SerializeField] private GameObject[] explosion;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private GameObject describtorWindow;
    [SerializeField] private GameObject menuWindow;
    [SerializeField] private Text keyTipText;
    [SerializeField] private RawImage effectIcons;
    [SerializeField] private GameObject gameOverWindow;

    void Awake()
    {
        GeneralFunctions.Start();
        GlobalLibrary.Start(GameObject.FindObjectOfType<PlayerEntity>().gameObject,
                            GameObject.FindObjectOfType<RoundManager>(),
                            explosion,
                            describtorWindow,
                            menuWindow,
                            explosionEffect,
                            keyTipText,
                            effectIcons,
                            gameOverWindow);
    }

    void Update()
    {
        GeneralFunctions.Update();
    }
}
