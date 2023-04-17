﻿using System;
using System.Threading.Tasks;
using Core.Module.CharacterData;
using Core.Module.ItemData;
using Core.Module.Player;
using Core.Module.WorldData;
using Core.NetworkPacket.ServerPacket;
using Core.TaskManager;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Module.NpcData;

public sealed class NpcInstance : Character
{
    private readonly NpcTemplateInit _npcTemplate;
    private readonly NpcKnownList _npcKnownList;
    private readonly NpcUseSkill _npcUseSkill;
    private readonly NpcCombat _npcCombat;
    private readonly NpcBaseStatus _npcBaseStatus;
    private readonly NpcDesire _npcDesire;
    private readonly NpcAi _npcAi;
    private readonly NpcTeleport _npcTeleport;
    private readonly NpcLearnSkill _npcLearnSkill;
    public readonly int NpcId;
    public readonly int NpcHashId;
        
    public int SpawnX { get; set; }
    public int SpawnY { get; set; }
    public int SpawnZ { get; set; }
        
    public NpcInstance(int objectId, NpcTemplateInit npcTemplateInit, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        ObjectId = objectId;
        _npcTemplate = npcTemplateInit;
        NpcHashId = GetStat().Id + 1000000;
        CharacterName = GetStat().Name;
        NpcId = GetStat().Id;
        Level = GetStat().Level;
        _npcKnownList = new NpcKnownList(this);
        _npcUseSkill = new NpcUseSkill(this);
        _npcCombat = new NpcCombat(this);
        _npcBaseStatus = new NpcBaseStatus(this);
        _npcDesire = new NpcDesire(this);
        _npcAi = new NpcAi(this);
        _npcTeleport = new NpcTeleport(this);
        _npcLearnSkill = new NpcLearnSkill(this);
    }

    public NpcUseSkill NpcUseSkill() => _npcUseSkill;
    public NpcTeleport NpcTeleport() => _npcTeleport;
    public override Weapon GetActiveWeaponItem()
    {
        throw new NotImplementedException();
    }

    public override ICharacterCombat CharacterCombat() => _npcCombat;
    public override ICharacterBaseStatus CharacterBaseStatus() => _npcBaseStatus;
    public override ICharacterKnownList CharacterKnownList() => _npcKnownList;
    public NpcTemplateInit GetTemplate() => _npcTemplate;
    public NpcDesire NpcDesire() => _npcDesire;
    public NpcAi NpcAi() => _npcAi;
    public NpcStat GetStat() => _npcTemplate.GetStat();
    public NpcLearnSkill NpcLearnSkill() => _npcLearnSkill; 

    public void OnSpawn(int x, int y, int z, int h)
    {
        Heading = h;
        SpawnMe(x, y, z);
    }

    private async Task SendRequestAsync(PlayerInstance playerInstance)
    {
        if (_npcTemplate.GetStat().CanBeAttacked == 1)
        {
            await playerInstance.SendPacketAsync(new ValidateLocation(this));
            if (Math.Abs(playerInstance.GetZ() - GetZ()) < 400) // this max height difference might need some tweaking
            {
                // Set the PlayerInstance Intention to AI_INTENTION_ATTACK
                playerInstance.CharacterDesire().AddDesire(Desire.AttackDesire, this);
            }
        }

        if (_npcTemplate.GetStat().CanBeAttacked == 1)
        {
            NpcAi().Attacked(playerInstance);
            return;
        }
        NpcAi().Talked(playerInstance);
    }

    public async Task ShowPage(PlayerInstance player, string fnHi)
    {
        await NpcChatWindow.ShowPage(player, fnHi, this);
    }

    public async Task MenuSelect(int askId, int replyId, PlayerInstance playerInstance)
    {
        await NpcAi().MenuSelect(askId, replyId, playerInstance);
    }

    public override async Task RequestActionAsync(PlayerInstance playerInstance)
    {
        if (await IsTargetSelected(playerInstance))
        {
            await SendRequestAsync(playerInstance);
            return;
        }
        await base.RequestActionAsync(playerInstance);
        await ShowTargetInfoAsync(playerInstance);
    }

    public override async Task DoDieProcess()
    {
        await SendToKnownPlayers(new Die(this));
        TaskManagerScheduler.Schedule(async () =>
        {
            if (GetWorldRegion() != null)
            {
                GetWorldRegion().RemoveFromZones(this);
            }
            //await CharacterTargetAction().RemoveTargetAsync();
            //_worldInit.RemoveObject(this);
            var worldInit = ServiceProvider.GetRequiredService<WorldInit>();
            worldInit.RemoveVisibleObject(this, WorldObjectPosition().GetWorldRegion());
            WorldObjectPosition().SetWorldRegion(null);
                
            await SendToKnownPlayers(new DeleteObject(ObjectId));
            CharacterKnownList().RemoveMeFromKnownObjects();
            CharacterKnownList().RemoveAllKnownObjects();
        }, _npcTemplate.GetStat().CorpseTime * 1000);
    }

    private Task<bool> IsTargetSelected(PlayerInstance playerInstance)
    {
        return Task.FromResult(this == playerInstance.CharacterTargetAction().GetTarget());
    }
        
    private async Task ShowTargetInfoAsync(PlayerInstance playerInstance)
    {
        if (_npcTemplate.GetStat().CanBeAttacked == 1)
        {
            await playerInstance.SendPacketAsync(new MyTargetSelected(ObjectId, playerInstance.PlayerStatus().Level - _npcTemplate.GetStat().Level));
            // Send a Server->Client packet StatusUpdate of the NpcInstance to the PlayerInstance to update its HP bar
            StatusUpdate su = new StatusUpdate(ObjectId);
            su.AddAttribute(StatusUpdate.CurHp, (int) CharacterStatus().CurrentHp);
            su.AddAttribute(StatusUpdate.MaxHp, (int) _npcTemplate.GetStat().OrgHp);
            await playerInstance.SendPacketAsync(su);
            return;
        }
        await playerInstance.SendPacketAsync(new MyTargetSelected(ObjectId, 0));
        await playerInstance.SendPacketAsync(new ValidateLocation(this));
    }
}