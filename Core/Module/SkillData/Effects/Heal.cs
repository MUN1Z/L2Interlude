using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Module.CharacterData;
using Core.Module.Player;
using Core.NetworkPacket.ServerPacket;
using L2Logger;

namespace Core.Module.SkillData.Effects
{
    public class Heal : Effect
    {
        private readonly int _healPoint;

        public Heal(IReadOnlyList<string> param, SkillDataModel skillDataModel)
        {
            _healPoint = Convert.ToInt32(param[1]);
            SkillDataModel = skillDataModel;
        }
        
        public override async Task Process(Character currentInstance, Character targetInstance)
        {
            var effectResult = CanPlayerUseSkill(currentInstance, targetInstance);
            if (effectResult.IsNotValid)
            {
                await currentInstance.SendPacketAsync(new SystemMessage(effectResult.SystemMessageId));
                return;
            }
            var heal = (_healPoint + Math.Sqrt(targetInstance.CharacterCombat().GetMagicalAttack()));
            targetInstance.CharacterStatus().IncreaseCurrentHp(heal);
            await SendStatusUpdate(targetInstance);
            
            LoggerManager.Info($"Magic Heal Points: {heal}");
            SystemMessage sm = new SystemMessage(SystemMessageId.S1HpRestored);
            sm.AddNumber((int) heal);
            //await targetInstance.SendPacketAsync(sm); TODO
        }
    }
}