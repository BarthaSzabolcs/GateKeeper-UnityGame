﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    #region ShowInEditor
    [SerializeField] List<WeaponData> data;
    [SerializeField] PlayerController wielder;
    #endregion
    #region HideInEditor
    SpriteRenderer renderer;
    List<WeaponData> instances;
    int dataIndex = 0;
    public int DataIndex
    {
        get
        {
            return dataIndex;
        }
        set
        {
            if(value > data.Count - 1)
            {
                dataIndex = 0;
            }
            else if(value < 0)
            {
                dataIndex = data.Count - 1;
            }
            else
            {
                dataIndex = value;
            }
        }
    }
    [HideInInspector] public Coroutine reloadingRoutine;
    #endregion

    #region UnityFunctions
    private void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        wielder.OnAttackTriggered += Attack;
        wielder.OnReloadTriggered += Reload;
        wielder.OnNextWeaponTriggered += ChangeToNextWeapon;
        wielder.OnPreviousWeaponTriggered += ChangeToPreviousWeapon;
        InitializeInstances();
        renderer.sprite = instances[DataIndex].sprite;
    }
    #endregion
    void Initialize(int i)
    {
        RangedWeaponData rangedData = data[i] as RangedWeaponData;
        if (rangedData != null)
        {
            rangedData = Instantiate(rangedData);
            rangedData.Initialize
            (
                GetComponent<Rigidbody2D>()
            );
            instances[i] = rangedData;
        }

        MeeleWeaponData meeleData = data[i] as MeeleWeaponData;
        if (meeleData != null)
        {
            meeleData = Instantiate(meeleData);
            meeleData.Initialize
            (

            );
        }
        RefreshApearance();
    }
    void InitializeInstances()
    {
        instances = new List<WeaponData>(new WeaponData[data.Count]);
        for (int i = 0; i < data.Count; i++)
        {
            Initialize(i);
        }
    }
    void RefreshApearance()
    {
        renderer.sprite = instances[DataIndex].sprite;
    }

    void Attack()
    {
        if(reloadingRoutine == null)
        {
            instances[dataIndex].Attack();
        }
        else if (reloadingRoutine != null)
        {
            StopCoroutine(reloadingRoutine);
            reloadingRoutine = null;
        }
    }
    void Reload()
    {
        RangedWeaponData ranged = instances[dataIndex] as RangedWeaponData;
        if (ranged != null)
        {
            if (reloadingRoutine == null)
            {
                reloadingRoutine = StartCoroutine(ranged.ReloadRoutine(this));
            }
        }
    }

    void ChangeToNextWeapon()
    {
        if(reloadingRoutine != null)
        {
            StopCoroutine(reloadingRoutine);
            reloadingRoutine = null;
        }
        DataIndex++;
        RefreshApearance();
    }
    void ChangeToPreviousWeapon()
    {
        if (reloadingRoutine != null)
        {
            StopCoroutine(reloadingRoutine);
            reloadingRoutine = null;
        }
        DataIndex--;
        RefreshApearance();
    }

    public void AddNewWeapon(WeaponData newData)
    {
        data.Add(newData);
    }
    public void RemoveWeapon(WeaponData dataToRemove)
    {
        data.Remove(dataToRemove);
    }
}
