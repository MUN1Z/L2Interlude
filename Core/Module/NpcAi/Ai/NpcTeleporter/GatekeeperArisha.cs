using System.Collections.Generic;
using Core.Module.NpcAi.Ai.NpcType;
using Helpers;

namespace Core.Module.NpcAi.Ai.NpcTeleporter;

public class GatekeeperArisha : Teleporter
{
    public override IList<TeleportList> Position => new List<TeleportList>
    {
        new("Rune Castle Town Store", 43849, -47877, -792, 150, 0),
        new("Rune Castle Town Guild", 38316, -48216, -1152, 150, 0)
    };
}