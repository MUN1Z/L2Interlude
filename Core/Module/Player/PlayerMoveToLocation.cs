﻿using System.Threading.Tasks;
using Core.Module.CharacterData;
using Core.NetworkPacket.ServerPacket;
using L2Logger;

namespace Core.Module.Player
{
    public sealed class PlayerMoveToLocation
    {
        private readonly PlayerInstance _playerInstance;
        public PlayerMoveToLocation(PlayerInstance playerInstance)
        {
            _playerInstance = playerInstance;
        }
        
        public async Task MoveToLocationAsync(int targetX, int targetY, int targetZ, int originX, int originY, int originZ)
        {
            
            if ((targetX == originX) && (targetY == originY) && (targetZ == originZ))
            {
                await _playerInstance.SendPacketAsync(new StopMove(_playerInstance));
                await _playerInstance.SendActionFailedPacketAsync();
            }

            double dx = targetX - _playerInstance.Location.GetX();
            double dy = targetY - _playerInstance.Location.GetY();

            if (((dx * dx) + (dy * dy)) > 98010000)
            {
                await _playerInstance.SendActionFailedPacketAsync();
                return;
            }
            _playerInstance.PlayerDesire().AddDesire(Desire.MoveToDesire, new Location(targetX, targetY, targetZ));
        }
        

        public async Task ValidatePositionAsync(int x, int y, int z, int heading)
        {
            int realX = _playerInstance.Location.GetX();
            int realY = _playerInstance.Location.GetY();
            int realZ = _playerInstance.Location.GetZ();
            
            LoggerManager.Info($"Validate Location: X: {realX}, Y: {realY} Z: {realZ}");

            if ((x == 0) && (y == 0) && (realX != 0))
            {
                return;
            }

            int dx = x - realX;
            int dy = y - realY;
            int dz = z - realZ;
            double diffSq = ((dx * dx) + (dy * dy));
        }
    }
}