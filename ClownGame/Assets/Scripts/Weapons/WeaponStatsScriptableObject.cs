using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponStats",menuName = "WeaponStats")]
public class WeaponStatsScriptableObject : ScriptableObject
{
    [Header("BaseSettings")]
    public FireType fireType;
    public ProjectileType projectileType;

    [Header("BaseStats")]
    public float damage = 1;
    public float fireRate = 0.4f;
    public float accuracy = 0.15f;
    public float horizontalRotationRecoil = 1;
    public float backwardsRecoil = 1;
    public int clipSize = 16;
    public int maxAmmoPouchSize = 160;
    public float reloadSpeed;
    public int projectileAmount = 1;

    [Header("BurstFireStats")]
    public float burstFireDelay;
    public int burstRounds;

    [Header("RaycastShooting")]
    public float raycastFireLength;
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
        burst
    }

    public enum ProjectileType
    {
        rayCast,
        projectile,
        damageZone
    }
}
