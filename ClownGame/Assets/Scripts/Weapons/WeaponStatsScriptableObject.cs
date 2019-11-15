using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponStatsScriptableObject : ScriptableObject
{
    [Header("BaseSettings")]
    public FireType fireType;
    public ProjectileType projectileType;

    [Header("BaseStats")]
    public float damage = 1;
    public float fireRate = 0.4f;
    public float accuracy = 0.15f;
    public float recoil = 1;
    public float clipSize = 16;
    public float maxAmmoPouchSize = 160;
    public float reloadSpeed;
    public int projectileAmount = 1;

    [Header("RaycastShooting")]
    public bool piercing;
    public float bulletHealth;
    public bool explodingBullets;
    public float explosionDamage;

    [Header("ProjectileWeapons")]
    public float gravity;
    public float lifeTime;
    public int bounces;
    public float bounceStrenght;

    public enum FireType
    {
        auto,
        singleFire,
        Burst
    }

    public enum ProjectileType
    {
        rayCast,
        projectile,
        damageZone
    }
}
