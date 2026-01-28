using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunBase
{
	[Header("銃のベース")]
	[Tooltip("銃のダメージ")]
	[SerializeField] protected float damage = 10.0f;
	[Tooltip("着弾した物体を後ろに押す力")]
	[SerializeField] protected float impactForce = 30.0f;
	[Tooltip("何秒間隔で撃つか")]
	[SerializeField] protected float fireRate = 0.1f;
	[Tooltip("射撃間隔時間用のカウントタイマー")]
	protected float fireCountTimer = 0.0f;
	[Tooltip("現在のマガジンの弾数")]
	protected int currentMagazine;
	public int CurrentMagazine => currentMagazine;
	[Tooltip("現在の残弾数")]
	protected int currentAmmo = 40;
	public int CurrentAmmo => currentAmmo;

	[Tooltip("リロードのオン・オフ")]
	protected bool isReloadTimeActive = false;
	public bool IsReloadTimeActive => isReloadTimeActive;

	[Tooltip("リロード時間用のカウントタイマー")]
	protected float reloadCountTimer = 0.0f;
}
