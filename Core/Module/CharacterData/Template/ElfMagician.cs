﻿using System.Collections.Generic;
using Core.Module.CharacterData.Template.Race;
using Helpers;

namespace Core.Module.CharacterData.Template
{
    public abstract class ElfMagician : Elf
    {
        private const int BasePhysicalAttack = 3;
        private const int BaseCritical = 4;
        private const string BaseAttackType = "fist";
        private const int BaseAttackSpeed = 300;
        //private const int BaseDefend = 80; //{Upper body; Lower body; Pitch; Boots; Gloves; Underwear; Mantle} {31;18;12;7;8;3;1}
        private const int BaseDefendUpperBody = 15;
        private const int BaseDefendLowerBody = 8;
        private const int BaseDefendPitch = 12;
        private const int BaseDefendBoots = 7;
        private const int BaseDefendGloves = 8;
        private const int BaseDefendUnderwear = 3;
        private const int BaseDefendMantle = 1;

        private const int BaseMagicAttack = 6;
        //private const int BaseMagicDefend = 41; //{The earring which rises; Left earring; The ring which rises; Left ring; Neck} {9;9;5;5;13}
        private const int BaseMagicDefendLeftEarring = 9;
        private const int BaseMagicDefendRightEarring = 9;
        private const int BaseMagicDefendLeftRing = 5;
        private const int BaseMagicDefendRightRing = 5;
        private const int BaseMagicDefendNecklace = 13;

        private const int BaseAttackRange = 20;
        private const int BaseDamageRange = 0;
        private const int BaseRandDam = 10;

        private const int BaseGroundLowSpeed = 85;
        private const int BaseGroundHighSpeed = 122;
        private const int BaseUnderWaterLowSpeed = 50;
        private const int BaseUnderWaterHighSpeed = 50;
        
        private const byte Int = 37;
        private const byte Str = 21;
        private const byte Con = 25;
        private const byte Men = 40;
        private const byte Dex = 24;
        private const byte Wit = 23;

        private IList<string> _initialEquipment;
        private IList<Location> _initialStartPoint;
        
        protected ElfMagician()
        {
            InitialEquipment();
            InitialStartPoint();
            InitialHpRegen();
        }
        
        private void InitialHpRegen()
        {
            _baseHpRegen = new List<float> { 6.0f, 7.5f, 3.5f, 4.5f, 5.5f, 6.5f, 7.5f, 8.5f };
        }
        
        private void InitialStartPoint()
        {
            _initialStartPoint = new List<Location>
            {
                new Location(46045, 41251, -3440),
                new Location(46117, 41247, -3440),
                new Location(46182, 41198, -3440),
                new Location(46115, 41141, -3440),
                new Location(46048, 41141, -3440),
                new Location(45978, 41196, -3440)
            };
        }

        private void InitialEquipment()
        {
            _initialEquipment = new List<string>
            {
                "apprentice_s_wand",
                "apprentice_s_tunic",
                "apprentice_s_hose",
                "tutorial_guide"
            };
        }

        public byte GetInt()
        {
            return Int;
        }
        public byte GetStr()
        {
            return Str;
        }
        public byte GetCon()
        {
            return Con;
        }
        public byte GetMen()
        {
            return Men;
        }
        public byte GetDex()
        {
            return Dex;
        }
        public byte GetWit()
        {
            return Wit;
        }

        public IEnumerable<string> GetInitialEquipment()
        {
            return _initialEquipment;
        }

        public Location GetInitialStartPoint()
        {
            var rndItem = Rnd.Next(6);
            return _initialStartPoint[rndItem];
        }

        public int GetBasePhysicalAttack()
        {
            return BasePhysicalAttack;
        }

        public int GetBaseAttackSpeed()
        {
            return BaseAttackSpeed;
        }

        public int GetBaseCritical()
        {
            return BaseCritical;
        }
        public int GetBaseDefendUpperBody()
        {
            return BaseDefendUpperBody;
        }

        public int GetBaseDefendLowerBody()
        {
            return BaseDefendLowerBody;
        }

        public int GetBaseDefendPitch()
        {
            return BaseDefendPitch;
        }

        public int GetBaseDefendBoots()
        {
            return BaseDefendBoots;
        }

        public int GetBaseDefendGloves()
        {
            return BaseDefendGloves;
        }

        public int GetBaseDefendUnderwear()
        {
            return BaseDefendUnderwear;
        }

        public int GetBaseDefendMantle()
        {
            return BaseDefendMantle;
        }

        public int GetBaseMagicDefendLeftEarring()
        {
            return BaseMagicDefendLeftEarring;
        }

        public int GetBaseMagicDefendRightEarring()
        {
            return BaseMagicDefendRightEarring;
        }

        public int GetBaseMagicDefendLeftRing()
        {
            return BaseMagicDefendLeftRing;
        }

        public int GetBaseMagicDefendRightRing()
        {
            return BaseMagicDefendRightRing;
        }

        public int GetBaseMagicDefendNecklace()
        {
            return BaseMagicDefendNecklace;
        }

        public int GetBaseMagicAttack()
        {
            return BaseMagicAttack;
        }

        public int GetBaseAttackRange()
        {
            return BaseAttackRange;
        }

        public int GetBaseGroundHighSpeed()
        {
            return BaseGroundHighSpeed;
        }

        public int GetBaseGroundLowSpeed()
        {
            return BaseGroundLowSpeed;
        }
    }
}