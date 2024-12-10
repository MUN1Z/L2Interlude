﻿using Core.Controller;
using Core.Module.Handlers;
using Core.Module.NpcData;
using Core.Module.Player;
using Core.Module.WorldData;
using Core.NetworkPacket.ServerPacket;
using Microsoft.Extensions.DependencyInjection;
using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core.NetworkPacket.ClientPacket
{
    public class RequestBypass : PacketBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly PlayerInstance _playerInstance;
        private readonly string _command;
        private readonly WorldInit _worldInit;
        
        public RequestBypass(IServiceProvider serviceProvider, Packet packet, GameServiceController controller) : base(serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _playerInstance = controller.GameServiceHelper.CurrentPlayer;
            _worldInit = serviceProvider.GetRequiredService<WorldInit>();
            _command = packet.ReadString();
        }

        public override async Task Execute()
        {
            if (_playerInstance.IsGM)
            {
                await _playerInstance.SendPacketAsync(new SystemMessage(SystemMessageId.S1).AddString($"[BYPASS] {_command}"));
            }

            var split = _command.Split("#");
            var command = split.First();
            switch (command)
            {
                case "teleport_request":
                {
                    await TeleportRequest(split);
                    break;
                }
                case "teleport_goto":
                {
                    await TeleportGoTo(split);
                    break;
                }
                case "show_radar":
                {
                    await ShowRadar(split);
                    break;
                }
                case "talk_select":
                {
                    await TalkSelected(split);
                    break;
                }
                case "quest_accept":
                {
                    await QuestAccepted(split);
                    break;
                }
                case "quest_choice":
                {
                    await QuestChoice(split);
                    break;
                }
                case "menu_select":
                {
                    await MenuSelect(split.Last());
                    break;
                }
                case "learn_skill":
                {
                    await LearnSkill(split);
                    break;
                }
                case "_heroes":
                {
                    await ShowHeroes();
                    break;
                }
            }

            if (_command.StartsWith("admin_"))
            {
                var adminCommandHandler = _serviceProvider.GetRequiredService<AdminCommandHandler>();
                await adminCommandHandler.Request(_playerInstance, _command);
            }
            
        }

        private async Task ShowHeroes()
        {
            //TODO
            await _playerInstance.SendPacketAsync(new ExHeroList());
        }
        private async Task TalkSelected(IEnumerable<string> split)
        {
            var npcObjectId = Convert.ToInt32(split.Last());
            var npcInstance = GetNpcInstance(npcObjectId);
            await npcInstance.TalkSelected(_playerInstance);
        }
        private async Task QuestAccepted(IEnumerable<string> split)
        {
            MatchCollection matches = new Regex(@"(\d+)\?quest_id=(\d+)").Matches(split.Last());
            if (matches.Count > 0 & matches[0].Groups.Count == 3)
            {
                var npcObjectId = Convert.ToInt32(matches[0].Groups[1].Value);
                var questId = Convert.ToInt32(matches[0].Groups[2].Value);
                var npcInstance = GetNpcInstance(npcObjectId);
                await npcInstance.QuestAccepted(questId, _playerInstance);
            }
        }

        private async Task QuestChoice(IEnumerable<string> split)
        {
            MatchCollection matchesWithOption = new Regex(@"(\d+)\?choice=(\d+)&option=(\d+)").Matches(split.Last());
            MatchCollection matchesWithoutOption = new Regex(@"(\d+)\?choice=(\d+)").Matches(split.Last());
            if (matchesWithOption.Count > 0 & matchesWithOption[0].Groups.Count == 4)
            {
                var npcObjectId = Convert.ToInt32(matchesWithOption[0].Groups[1].Value);
                var choice = Convert.ToInt32(matchesWithOption[0].Groups[2].Value);
                var option = Convert.ToInt32(matchesWithOption[0].Groups[3].Value);
                var npcInstance = GetNpcInstance(npcObjectId);
                await npcInstance.TalkSelected(string.Empty, _playerInstance, true, choice, option);
            }else if(matchesWithoutOption.Count > 0 & matchesWithoutOption[0].Groups.Count == 3)
            {
                var npcObjectId = Convert.ToInt32(matchesWithOption[0].Groups[1].Value);
                var choice = Convert.ToInt32(matchesWithOption[0].Groups[2].Value);
                var npcInstance = GetNpcInstance(npcObjectId);
                await npcInstance.TalkSelected(string.Empty, _playerInstance, true, choice);
            }
        }

        private async Task TeleportRequest(IEnumerable<string> split)
        {
            var npcObjectId = Convert.ToInt32(split.Last());
            var npcInstance = GetNpcInstance(npcObjectId);
            await npcInstance.NpcTeleport().TeleportRequest(_playerInstance);
        }

        private async Task TeleportGoTo(IReadOnlyList<string> split)
        {
            var parseNpc = split[1].Split("?");
            var npcObjectId = Convert.ToInt32(parseNpc.First());
            var teleportHashId = Convert.ToInt32(parseNpc.Last().Split("=")[1].Split(",")[0]);
            var teleportId = Convert.ToInt32(parseNpc.Last().Split("=")[1].Split(",")[1]);
            var npcInstance = GetNpcInstance(npcObjectId);
            await npcInstance.NpcTeleport().TeleportToLocation(teleportHashId, teleportId, _playerInstance);
        }
        private async Task ShowRadar(IReadOnlyList<string> split)
        {
            var parseNpc = split[1].Split("?");
            var npcObjectId = Convert.ToInt32(parseNpc.First());
            var radarHashId = Convert.ToInt32(parseNpc.Last().Split("=")[1].Split(",")[0]);
            var radarId = Convert.ToInt32(parseNpc.Last().Split("=")[1].Split(",")[1]);
            var npcInstance = GetNpcInstance(npcObjectId);
            await npcInstance.NpcRadar().ShowPositionOnRadar(radarHashId, radarId, _playerInstance);
        }

        private async Task MenuSelect(string spl)
        {
            var charLocation = spl.IndexOf("?", StringComparison.Ordinal);
            var npcObjectId = Convert.ToInt32(spl[..charLocation]);
            var askId  = Convert.ToInt32(BetweenStrings(spl, "ask=", "&"));
            var lasCharLocation = spl.LastIndexOf("=", StringComparison.Ordinal);
            var replyId = Convert.ToInt32(spl.Substring(lasCharLocation + 1));
            var npcInstance = GetNpcInstance(npcObjectId);
            await npcInstance.MenuSelect(askId, replyId, _playerInstance);
        }

        private async Task LearnSkill(IEnumerable<string> split)
        {
            var npcObjectId = Convert.ToInt32(split.Last());
            var npcInstance = GetNpcInstance(npcObjectId);
            await npcInstance.NpcLearnSkill().LearnSkillRequest(_playerInstance);
        }

        private string BetweenStrings(string text, string start, string end)
        {
            int p1 = text.IndexOf(start, StringComparison.Ordinal) + start.Length;
            int p2 = text.IndexOf(end, p1, StringComparison.Ordinal);

            return end == "" ? text[p1..] : text.Substring(p1, p2 - p1);
        }
        
        private NpcInstance GetNpcInstance(int objectId)
        {
            return _worldInit.GetNpcInstance(objectId);
        }
    }
}