﻿using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;
using Config;
using Helpers;
using L2Logger;
using LoginService.Controller.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Security;

namespace LoginService.Controller
{
    public class ClientController
    {
        private const int ScrambleCount = 10;
        private const int BlowfishCount = 20;
        private byte[][] _blowfishKeys;
        private IServiceProvider _serviceProvider;
        private ScrambledKeyPair[] _keyPairs;
        private LoginConfig _config;

        private readonly LoginPacketHandler _loginPacketHandler;

        private readonly ConcurrentDictionary<int, LoginServiceController> _loggedClients;

        public ClientController(IServiceProvider serviceProvider, LoginPacketHandler loginPacketHandler)
        {
            _serviceProvider = serviceProvider;
            _loginPacketHandler = loginPacketHandler;
            _loggedClients = new ConcurrentDictionary<int, LoginServiceController>();
            _config = _serviceProvider.GetService<LoginConfig>();
        }

        public IServiceProvider GetServiceProvider() => _serviceProvider;

        public async Task AcceptClient(TcpClient client)
        {
            LoginServiceController clientObject = new LoginServiceController(client, this, _loginPacketHandler, _config);
            await clientObject.Process();

            _loggedClients.TryAdd(clientObject.SessionId, clientObject);
        }

        public async Task Initialise()
        {
            LoggerManager.Info("Loading Keys...");
            await Task.Run(GenerateScrambledKeys);
            await Task.Run(GenerateBlowFishKeys);
            InitializeRSA();
        }

        private void GenerateBlowFishKeys()
        {
            _blowfishKeys = new byte[BlowfishCount][];

            for (int i = 0; i < BlowfishCount; i++)
            {
                _blowfishKeys[i] = new byte[16];
                Rnd.NextBytes(_blowfishKeys[i]);
            }
            LoggerManager.Info($"Stored {_blowfishKeys.Length} keys for Blowfish communication.");
        }

        private void GenerateScrambledKeys()
        {
            LoggerManager.Info("Scrambling keypairs.");

            _keyPairs = new ScrambledKeyPair[ScrambleCount];

            for (int i = 0; i < ScrambleCount; i++)
            {
                _keyPairs[i] = new ScrambledKeyPair();
            }

            LoggerManager.Info($"Cached {_keyPairs.Length} KeyPairs for RSA communication.");
        }
        private void InitializeRSA()
        {
            Rsa.Initialize(GetScrambledKeyPair());
            LoggerManager.Info($"Initialized RSA Engine.");
        }

        public byte[] GetBlowfishKey()
        {
            return _blowfishKeys[Rnd.Next(BlowfishCount - 1)];
        }

        public ScrambledKeyPair GetScrambledKeyPair()
        {
            return _keyPairs[0];
        }

        public void RemoveClient(LoginServiceController loginClient)
        {
            if (!_loggedClients.ContainsKey(loginClient.SessionId))
            {
                return;
            }

            LoginServiceController o;
            _loggedClients.TryRemove(loginClient.SessionId, out o);
        }
    }
}
