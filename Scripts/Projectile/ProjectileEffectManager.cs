using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileEffectManager : MonoBehaviour
{
    [SerializeField] ProjectileEffectProfile[] effectsProfiles;

    public ProjectileEffectProfile FindProfile(ProjectileEffect effect)
    {
        foreach(ProjectileEffectProfile profile in effectsProfiles)
        {
            if(profile.effect == effect)
            {
                return profile;
            }
        }
        return new ProjectileEffectProfile();
    }
}

[System.Serializable]
public struct ProjectileEffectProfile
{
    public string name;
    public ProjectileEffect effect;
    public Texture2D icon;
}