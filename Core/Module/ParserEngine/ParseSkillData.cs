﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using L2Logger;

namespace Core.Module.ParserEngine
{
    public class ParseSkillData : IParse
    {
        private readonly IResult _result;

        public ParseSkillData()
        {
            _result = new Result();
        }
        public void ParseLine(string line)
        {
            try
            {
                var items = line.Split("\t");
                if (items.Length <= 1)
                    return;
                var skillBegin = new SkillBegin();
                foreach (var item in items)
                {
                    var split = item.Replace("\t", "").Split("=");
                    if (split.Length <= 1)
                        continue;
                    var key = split[0].Trim();
                    var value = split[1].Trim();

                    switch (key)
                    {
                        case "skill_name":
                            skillBegin.SkillName = value.RemoveBrackets();
                            break;
                        case "skill_id":
                            skillBegin.SkillId = Convert.ToInt32(value);
                            continue;
                        case "level":
                            skillBegin.Level = Convert.ToInt32(value);
                            continue;
                        case "operate_type":
                            skillBegin.OperateType = value;
                            break;
                        case "magic_level":
                            skillBegin.MagicLevel = Convert.ToInt32(value);
                            break;
                        case "effect":
                            var effects = ParseEffect(skillBegin.SkillName, value);
                            skillBegin.Effect = effects;
                            break;
                        case "operate_cond":
                            skillBegin.OperateCond = value;
                            break;
                        case "is_magic":
                            skillBegin.IsMagic = Convert.ToByte(value);
                            break;
                        case "mp_consume2":
                            skillBegin.MpConsume2 = Convert.ToInt32(value);
                            break;
                        case "cast_range":
                            skillBegin.CastRange = Convert.ToInt32(value);
                            break;
                        case "effective_range":
                            skillBegin.EffectiveRange = Convert.ToInt32(value);
                            break;
                        case "skill_hit_time":
                            skillBegin.SkillHitTime = Convert.ToSingle(value, CultureInfo.InvariantCulture);
                            break;
                        case "skill_cool_time":
                            skillBegin.SkillCoolTime = Convert.ToSingle(value, CultureInfo.InvariantCulture);
                            break;
                        case "skill_hit_cancel_time":
                            skillBegin.SkillHitCancelTime = Convert.ToSingle(value, CultureInfo.InvariantCulture);
                            break;
                        case "reuse_delay":
                            skillBegin.ReuseDelay = Convert.ToSingle(value, CultureInfo.InvariantCulture);
                            break;
                        case "attribute":
                            skillBegin.Attribute = value;
                            break;
                        case "effect_point":
                            skillBegin.EffectPoint = value;
                            break;
                        case "abnormal_type":
                            skillBegin.AbnormalType = value;
                            break;
                        case "target_type":
                            skillBegin.TargetType = value;
                            break;
                        case "affect_scope":
                            skillBegin.AffectScope = value;
                            break;
                        case "affect_limit":
                            skillBegin.AffectLimit = value;
                            break;
                        case "next_action":
                            skillBegin.NextAction = value;
                            break;
                        case "ride_state":
                            skillBegin.RideState = value;
                            break;
                        case "debuff":
                            skillBegin.DeBuff = Convert.ToByte(value);
                            break;
                        case "abnormal_time":
                            skillBegin.AbnormalTime = Convert.ToInt32(value);
                            break;
                        case "abnormal_lv":
                            break;
                        case "abnormal_visual_effect":
                            break;
                        case "affect_range":
                            break;
                        case "affect_object":
                            break;
                        case "mp_consume1":
                            break;
                        case "activate_rate":
                            break;
                        case "lv_bonus_rate":
                            break;
                        case "basic_property":
                            break;
                        case "item_consume":
                            break;
                        case "hp_consume":
                            break;
                        case "fan_range":
                            break;
                        case "affect_scope_height":
                            break;
                        default:
                            skillBegin.RideState = value;
                            break;
                    }
                }
                if (skillBegin.SkillName is null)
                    return;
                _result.AddItem(skillBegin.SkillName, skillBegin);
            }
            catch (Exception ex)
            {
                LoggerManager.Error(ex.Message);
            }
        }

        private IList<string> ParseEffect(string skillName, string line)
        {
            var pattern1 = @"\{.*?\}";
            line =  line.Replace("{all}", "all");
            line = line.Replace("{bow}", "bow");
            line = line.Replace("{dagger}", "dagger");
            line = line.Replace("{pole}", "pole");
            line = line.Replace("{blunt}", "blunt");
            line = line.Replace("{dual}", "dual");
            line = line.Replace("{sword;blunt}", "sword;blunt");
            line = line.Replace("{armor_light}", "armor_light");
            line = line.Replace("{armor_none;armor_light;armor_heavy}", "armor_none;armor_light;armor_heavy");
            line = line.Replace("{armor_heavy}", "armor_heavy");
            line = line.Replace("{armor_magic}", "armor_magic");
            line = line.Replace("{fist;dualfist}", "fist;dualfist");
            line = line.Replace("{sword;blunt;pole}", "sword;blunt;pole");
            line = line.Replace("{bow;dualfist}", "bow;dualfist");
            line = line.Replace("{bow;dagger;fist;dualfist}", "bow;dagger;fist;dualfist");
            line = line.Replace("{sword;blunt;pole;dualfist;dual}", "sword;blunt;pole;dualfist;dual");
            
  
            var matches1 = Regex.Matches(line, pattern1);
            IList<string> effects = new List<string>();
            foreach (Match match1 in matches1)
            {
                effects.Add(match1.Value.RemoveBrackets());
            }
            return effects;
        }

        public IResult GetResult()
        {
            return _result;
        }
    }
}