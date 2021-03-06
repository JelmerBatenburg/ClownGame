﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

[CreateAssetMenu(fileName = "WeaponStats",menuName = "WeaponStats")]
public class WeaponStatsScriptableObject : ScriptableObject
{
    [Header("Visuals")]
    public GameObject weaponObject;

    [Header("CameraFeedbackStats")]
    public float explosionScreenShakeTime;
    public float explosionScreenShakeIntensity;
    public float cameraRecoil;
    public float camereHorizontalRecoil;

    [Header("BaseSettings")]
    public FireType fireType;
    public ProjectileType projectileType;
    public AudioClip clip;

    [Header("BaseStats")]
    public float damage = 1;
    public float fireRate = 0.4f;
    public float spread = 0.15f;
    public float horizontalRotationRecoil = 1;
    public float backwardsRecoil = 1;
    public int clipSize = 16;
    public int maxAmmoPouchSize = 160;
    public float reloadSpeed;
    public int projectileAmount = 1;
    public float force = 140;

    [Header("MultipleShots")]
    public bool multipleShots;
    public int shotAmount;

    [Header("BurstFireStats")]
    public float burstFireDelay;
    public int burstRounds;

    [Header("RaycastShooting")]
    public float raycastFireLength;
    public bool piercing;
    public float bulletHealth;
    public bool explodingBullets;

    [Header("ExplosionInfo")]
    public float explosionDamage;
    public float explosionRadius;
    public int explosionParticleIndex;
    public float explosionTimer;

    [Header("ProjectileWeapons")]
    public float fireStrenght;
    public string projectileName;

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
