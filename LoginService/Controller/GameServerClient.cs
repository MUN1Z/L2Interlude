﻿using Config;
using L2Logger;
using LoginService.Controller.Handlers;
using LoginService.Network.GameServerPackets;
using Network;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LoginService.Controller
{
    public class GameServerClient
    {
        public string Wan { get; set; }
        public short Port { get; set; }
        public short CurrentPlayers { get; set; }
        public short MaxPlayers { get; set; } = 1000;
        public string Info { get; set; }
        public bool Connected { get; set; }
        public bool TestMode { get; set; }
        public bool GmOnly { get; set; }
        public byte ServerId { get; set; }

        private readonly GameServerPacketHandler _packetHandler;
        private NetworkStream _stream;
        private TcpClient _client;
        private readonly List<int> _accountsInGame;
        private LoginConfig _config;
        public GameServerClient(GameServerPacketHandler packetHandler, LoginConfig config)
        {
            _packetHandler = packetHandler;
            _config = config;
            _accountsInGame = new List<int>();
        }

        public async Task AcceptServer(TcpClient client)
        {
            await ReadAsync(client);
        }

        private async Task ReadAsync(TcpClient tcpClient)
        {
            _stream = tcpClient.GetStream();
            _client = tcpClient;
            Connected = true;

            try
            {
                while (true)
                {
                    byte[] buffer = new byte[2];
                    int bytesRead = await _stream.ReadAsync(buffer, 0, 2);

                    if (bytesRead != 2)
                    {
                        throw new Exception("Wrong packet");
                    }

                    short length = BitConverter.ToInt16(buffer, 0);

                    buffer = new byte[length];
                    bytesRead = await _stream.ReadAsync(buffer, 0, length);

                    if (bytesRead != length)
                    {
                        throw new Exception("Wrong packet");
                    }

                    await Task.Run(() => _packetHandler.HandlePacket(new Packet(buffer, 1), this));
                }
            }
            catch (Exception e)
            {
                LoggerManager.Error($"ServerThread: {e.Message}");
            }
        }

        public void AccountInGame(int accountId, byte status)
        {
            if (status == 1)
            {
                if (!_accountsInGame.Contains(accountId))
                    _accountsInGame.Add(accountId);
            }
            else
            {
                if (_accountsInGame.Contains(accountId))
                    _accountsInGame.Remove(accountId);
            }
        }
        public async Task SendAsync(ServerPacket serverPacket)
        {
            await serverPacket.WriteAsync();

            byte[] data = serverPacket.GetBuffer();
            if (_config.DebugConfig.ShowHeaderPacket && _config.DebugConfig.ShowPacketToGame)
            {
                LoggerManager.Debug($"GameServerClient: AUTH>>GAME header: [{data[0].ToString("x2")}] size: [{data.Length}]");
            }
            if (_config.DebugConfig.ShowNamePacket && _config.DebugConfig.ShowPacketToGame)
            {
                LoggerManager.Debug($"GameServerClient: AUTH>>GAME name: {serverPacket.GetType().Name}");
            }
            if (_config.DebugConfig.ShowPacket && _config.DebugConfig.ShowPacketToGame)
            {
                string str = "";
                foreach (byte b in data)
                    str += b.ToString("x2") + " ";
                LoggerManager.Debug($"GameServerClient: AUTH>>GAME body: [ {str} ]");
            }

            byte[] lengthInBytes = BitConverter.GetBytes((short)data.Length);
            byte[] message = new byte[data.Length + 2];

            lengthInBytes.CopyTo(message, 0);
            data.CopyTo(message, 2);

            try
            {
                await _stream.WriteAsync(message, 0, message.Length);
                await _stream.FlushAsync();
            }
            catch (Exception ex)
            {
                LoggerManager.Error(ex.Message);
            }
        }

        public async Task SendPlayerAsync(LoginServiceController loginClient)
        {
            await SendAsync(new PleaseAcceptPlayer(1, loginClient.SessionKey));
        }
    }
}
