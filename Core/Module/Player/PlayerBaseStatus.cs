﻿using System;
using System.Collections.Generic;
using Core.Module.CharacterData;
using Core.Module.CharacterData.Template;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Module.Player;

public class PlayerBaseStatus : ICharacterBaseStatus
{
    private readonly PlayerInstance _playerInstance;
    private readonly PcParameterInit _statBonusInit;
    public float CurrentCp { get; set; }
    public byte Level { get; set; } = 1;
    public PlayerBaseStatus(PlayerInstance playerInstance)
    {
        _playerInstance = playerInstance;
        _statBonusInit = playerInstance.ServiceProvider.GetRequiredService<PcParameterInit>();
    }

    /// <summary>
    /// maxСp = maxCp + (maxCp * CON bonus / 100)
    /// </summary>
    /// <returns></returns>
    public int GetMaxCp()
    {
        var cpBegin = _playerInstance.TemplateHandler().GetCpBegin(Level);
        var conStat = _playerInstance.TemplateHandler().GetCon();
        return (int) (cpBegin + (cpBegin * _statBonusInit.GetConBonus(conStat) / 100));
    }

    /// <summary>
    /// MAX HP  = base * mod_con * mod_per + mod_diff
    /// maxHp = maxHp + (maxHp * CON bonus / 100)
    /// </summary>
    /// <returns></returns>
    public int GetMaxHp()
    {
        var hpBegin = _playerInstance.TemplateHandler().GetHpBegin(Level);
        var conStat =  _playerInstance.TemplateHandler().GetCon();
        var conBonus = _statBonusInit.GetConBonus(conStat);
        var modCon = (conBonus + 100) / 100f;
        var result = hpBegin * modCon;
        var effects = GetPlayerEffects();
        result = CalculateStats.CalculateMaxHp(effects, result);
        return (int)Math.Round(result);
    }
        
    /// <summary>
    /// maxMp = maxMp + (maxMp * MEN bonus / 100)
    /// </summary>
    /// <returns></returns>
    public int GetMaxMp()
    {
        var mpBegin = _playerInstance.TemplateHandler().GetMpBegin(Level);
        var menStat =  _playerInstance.TemplateHandler().GetMen();
        return (int) (mpBegin + (mpBegin * _statBonusInit.GetMenBonus(menStat) / 100));
    }
        
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public float GetHpRegenRate()
    {
        var baseHpRegen = _playerInstance.TemplateHandler().GetBaseHpRegen(Level);
        var conStat = _playerInstance.TemplateHandler().GetCon();
        var modCon = (_statBonusInit.GetConBonus(conStat) + 100) / 100f;
        var result = baseHpRegen * modCon;
        return result;
    }

    public float GetMpRegenRate()
    {
        throw new NotImplementedException();
    }

    private IEnumerable<EffectDuration> GetPlayerEffects()
    {
        return _playerInstance.CharacterEffect().GetEffects().Values;
    }
}