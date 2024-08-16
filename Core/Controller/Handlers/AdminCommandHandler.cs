﻿using Core.Attributes;
using Core.Controller.Handlers.AdminCommands;
using Core.Module.Player;
using Core.NetworkPacket.ServerPacket;
using Helpers;
using L2Logger;
using System;
using System.Collections.Generic;
using System.Reflection;


//CLR: 4.0.30319.42000
//USER: GL
//DATE: 15.08.2024 20:01:45

namespace Core.Controller.Handlers
{
    public class AdminCommandHandler : IAdminCommandHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SortedList<string, AbstractAdminCommand> commands = new SortedList<string, AbstractAdminCommand>();
        public AdminCommandHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            IEnumerable<Type> typelist = Utility.GetTypesInNamespace(Assembly.GetExecutingAssembly(), "Core.Controller.Handlers.AdminCommands");
            foreach (Type t in typelist)
            {
                if (!t.Name.StartsWith("Abstract"))
                {
                    Register(Activator.CreateInstance(t));
                }
            }

            LoggerManager.Info($"AdminCommandHandler: Loaded {commands.Count} commands.");
        }

        public void Request(PlayerInstance admin, string alias) 
        {
            if (!alias.StartsWith("admin_"))
                alias = "admin_" + alias;

            string cmd = alias;
            if (alias.Contains(" "))
                cmd = alias.Split(' ')[0];

            if (!commands.ContainsKey(cmd))
            {
                admin.SendPacketAsync(new SystemMessage($"Command {cmd} not exists."));
                //admin.SendActionFailedPacketAsync();
                LoggerManager.Warn($"AdminCommandHandler: Command {cmd} not exists.");
                return;
            }

            AbstractAdminCommand processor = commands[cmd];
            try
            {
                processor.UseCommand(admin, alias);
            }
            catch (Exception ex)
            {
                LoggerManager.Error($"AdminCommandHandler: {ex.Message} {ex.StackTrace}");
            }
        }

        public void Register(object processor)
        {
            CommandAttribute attribute =
                (CommandAttribute)processor.GetType().GetCustomAttribute(typeof(CommandAttribute));
            commands.Add(attribute.CommandName, (AbstractAdminCommand)processor);
        }
    }
}
