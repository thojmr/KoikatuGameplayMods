﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExtensibleSaveFormat;
#if KK
    using KKAPI.MainGame;
#endif
#if AI
    using AIChara;
    using AIProject;
#endif

namespace KK_Pregnancy
{
    public static class PregnancyDataUtils
    {
        private static readonly int[] _earlyDetectPersonalities = { 00, 11, 12, 13, 19, 24, 31, 33 };
        private static readonly int[] _lateDetectPersonalities = { 03, 05, 08, 20, 25, 26, 37 };

        /// <param name="c">ChaFile to test</param>
        ///// <param name="afterWasDiscovered">The girl knows about it / tested it</param>
        public static PregnancyData GetPregnancyData(this ChaFileControl c)
        {
            if (c == null) return null;

            var d = ExtendedSave.GetExtendedDataById(c, PregnancyPlugin.GUID);
            if (d == null) return null;

            return PregnancyData.Load(d);
        }

        /// <param name="heroine">Heroine to test</param>
        ///// <param name="afterWasDiscovered">The girl knows about it / tested it</param>
        #if KK
            public static PregnancyData GetPregnancyData(this SaveData.Heroine heroine)
            {
                if (heroine == null) return new PregnancyData();

                // Figure out which data to take if there are multiple
                // probably not necessary? null check should be enough? 
                return heroine.GetRelatedChaFiles()
                    .Select(GetPregnancyData)
                    .Where(x => x != null)
                    .OrderByDescending(x => x.PregnancyCount)
                    .ThenByDescending(x => x.WeeksSinceLastPregnancy)
                    .ThenByDescending(x => x.Week)
                    // .ThenByDescending(x => x.MenstruationSchedule) //TODO
                    .ThenByDescending(x => x.GameplayEnabled)
                    .FirstOrDefault() ?? new PregnancyData();
            }

            public static HeroineStatus GetHeroineStatus(this SaveData.Heroine heroine, PregnancyData pregData = null)
            {
                if (heroine == null) return HeroineStatus.Unknown;

                if (pregData == null) pregData = heroine.GetPregnancyData();

                // Check if she wants to tell
                if (heroine.intimacy >= 80 ||
                    heroine.hCount >= 5 ||
                    heroine.parameter.attribute.bitch && heroine.favor > 50 ||
                    (heroine.isGirlfriend || heroine.favor >= 90) &&
                    (!heroine.isVirgin || heroine.hCount >= 2 || heroine.intimacy >= 40))
                {
                    var pregnancyWeek = pregData.Week;
                    if (pregnancyWeek > 0)
                    {
                        if (pregnancyWeek >= PregnancyData.LeaveSchoolWeek) return HeroineStatus.OnLeave;
                        if (PregnancyPlugin.ShowPregnancyIconEarly.Value) return HeroineStatus.Pregnant;
                        // Different personalities notice at different times
                        if (_earlyDetectPersonalities.Contains(heroine.personality))
                        {
                            if (pregnancyWeek > 1) return HeroineStatus.Pregnant;
                        }
                        else if (_lateDetectPersonalities.Contains(heroine.personality))
                        {
                            if (pregnancyWeek > 11) return HeroineStatus.Pregnant;
                        }
                        else
                        {
                            if (pregnancyWeek > 5) return HeroineStatus.Pregnant;
                        }
                    }

                    return HFlag.GetMenstruation(heroine.MenstruationDay) == HFlag.MenstruationType.安全日
                        ? HeroineStatus.Safe
                        : HeroineStatus.Risky;
                }

                return HeroineStatus.Unknown;
            }

        #elif AI
        
            //TODO copied from KKAPI
            public static Actor GetNPC(this AgentActor heroine)
            {
                if (heroine == null) throw new ArgumentNullException(nameof(heroine));

                if (heroine.transform == null) return null;
                return heroine.transform.GetComponent<Actor>();
            }

            //TODO copied from KKAPI
            public static IEnumerable<ChaFileControl> GetRelatedChaFiles(AgentActor heroine)
            {
                if (heroine == null) throw new ArgumentNullException(nameof(heroine));

                var results = new List<ChaFileControl>();

                if (heroine.ChaControl != null && heroine.ChaControl.chaFile != null)
                    results.Add(heroine.ChaControl.chaFile);

                var npc = GetNPC(heroine);
                if (npc != null && npc.ChaControl != null && npc.ChaControl.chaFile != null)
                    results.Add(npc.ChaControl.chaFile);

                return results;
            }


            public static PregnancyData GetPregnancyData(this AgentActor heroine)
            {
                if (heroine == null) return new PregnancyData();

                // Figure out which data to take if there are multiple
                // probably not necessary? null check should be enough? 
                return GetRelatedChaFiles(heroine)
                    .Select(GetPregnancyData)
                    .Where(x => x != null)
                    .OrderByDescending(x => x.PregnancyCount)
                    .ThenByDescending(x => x.WeeksSinceLastPregnancy)
                    .ThenByDescending(x => x.Week)
                    // .ThenByDescending(x => x.MenstruationSchedule) //TODO
                    .ThenByDescending(x => x.GameplayEnabled)
                    .FirstOrDefault() ?? new PregnancyData();
            }

            public static HeroineStatus GetHeroineStatus(this AgentActor heroine, PregnancyData pregData = null)
            {
                if (heroine == null) return HeroineStatus.Unknown;

                if (pregData == null) pregData = heroine.GetPregnancyData();

                // Check if she wants to tell
                // if (heroine.intimacy >= 80 ||
                //     heroine.hCount >= 5 ||
                //     heroine.parameter.attribute.bitch && heroine.favor > 50 ||
                //     (heroine.isGirlfriend || heroine.favor >= 90) &&
                //     (!heroine.isVirgin || heroine.hCount >= 2 || heroine.intimacy >= 40))
                // {
                    var pregnancyWeek = pregData.Week;
                    if (pregnancyWeek > 0)
                    {
                        if (pregnancyWeek >= PregnancyData.LeaveSchoolWeek) return HeroineStatus.OnLeave;
                        if (PregnancyPlugin.ShowPregnancyIconEarly.Value) return HeroineStatus.Pregnant;
                        // Different personalities notice at different times
                        if (_earlyDetectPersonalities.Contains(heroine.ChaControl.fileParam.personality))
                        {
                            if (pregnancyWeek > 1) return HeroineStatus.Pregnant;
                        }
                        else if (_lateDetectPersonalities.Contains(heroine.ChaControl.fileParam.personality))
                        {
                            if (pregnancyWeek > 11) return HeroineStatus.Pregnant;
                        }
                        else
                        {
                            if (pregnancyWeek > 5) return HeroineStatus.Pregnant;
                        }
                    }

                    //TODO  use random chance?
                    return HeroineStatus.Risky;

                // }

                // return HeroineStatus.Unknown;
            }
        #endif
        
    }
}