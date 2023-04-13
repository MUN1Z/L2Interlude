﻿using System.Linq;
using System.Threading.Tasks;
using Core.Module.NpcAi;
using Core.Module.NpcAi.Ai;
using Core.Module.Player;
using Core.NetworkPacket.ServerPacket;

namespace Core.Module.NpcData
{
    public static class NpcLearnSkill
    {
        public static async Task LearnSkillRequest(PlayerInstance playerInstance, NpcInstance npcInstance)
        {
            var talker = new Talker(playerInstance);
            if (npcInstance.NpcAi().GetDefaultNpc() is GuildCoach guildCoach)
            {
                await guildCoach.LearnSkillRequested(talker);
            }
        }
        
        public static async Task ShowSkillList(PlayerInstance player)
        {
            var acquiredSkills = await player.PlayerSkill().GetPlayerSkills();
            AcquireSkillList acquireSkillList = new AcquireSkillList(AcquireSkillList.SkillType.Usual);
            var skillList = Initializer.SkillAcquireInit()
                .GetSkillAcquireListByClassKey(player.PlayerCharacterInfo().ClassName)
                .Where(sam => sam.LevelToGetSkill <= player.PlayerStatus().Level);
            foreach (var skillAcquire in skillList)
            {
                var currentSkillLevel = 1;
                var skillDataModel = Initializer.SkillDataInit().GetSkillByName(skillAcquire.SkillName);
                if (acquiredSkills.ContainsKey(skillDataModel.SkillId))
                {
                    if (acquiredSkills[skillDataModel.SkillId].Level >= skillDataModel.Level)
                    {
                        continue;
                    }
                    var acquiredLevel = acquiredSkills[skillDataModel.SkillId].Level;
                    currentSkillLevel = acquiredLevel + 1;
                }
                acquireSkillList.AddSkill(skillDataModel.SkillId, currentSkillLevel,
                    skillAcquire.LevelUpSp, skillAcquire.LevelToGetSkill, 0);
                
            }
            await player.SendPacketAsync(acquireSkillList);
        }
    }
}