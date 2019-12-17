using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterClass",menuName = "CharacterClass")]
public class ClassScriptableObject : ScriptableObject
{
    public WeaponStatsScriptableObject[] weapons;
    public float maxHealth = 100;
    public float movementSpeed = 7;
    public float airControl = 4;
    public float jumpHeight = 5;
}
