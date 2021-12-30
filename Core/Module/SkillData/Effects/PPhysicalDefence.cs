﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Module.Player;
using Core.NetworkPacket.ServerPacket;

namespace Core.Module.SkillData.Effects
{
    public class PPhysicalDefence : Effect
    {
        private readonly int _defence;
        private readonly int _abnormalTime;

        public PPhysicalDefence(IReadOnlyList<string> param, SkillDataModel skillDataModel)
        {
            SkillDataModel = skillDataModel;
            _defence = Convert.ToInt32(param[2]);
            _abnormalTime = skillDataModel.AbnormalTime;
            IsModPer = (param[3] == "per");
        }
        public override async Task Process(PlayerInstance playerInstance, PlayerInstance targetInstance)
        {
            var effectResult = CanPlayerUseSkill(playerInstance, targetInstance);
            if (effectResult.IsNotValid)
            {
                await playerInstance.SendPacketAsync(new SystemMessage(effectResult.SystemMessageId));
                return;
            }
            await StartEffectTask(_abnormalTime * 1000, targetInstance);
        }
        
        public int GetPhysicalDefence()
        {
            return _defence;
        }
    }
}