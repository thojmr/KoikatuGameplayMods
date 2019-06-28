﻿using System.Collections.Generic;
using System.Linq;
using KKABMX.Core;
using KKAPI.Maker;
using UnityEngine;

namespace KK_Pregnancy
{
    public class PregnancyBoneEffect : BoneEffect
    {
        private readonly PregnancyController _controller;

        private readonly Dictionary<string, BoneModifierData> _pregnancyFullValues = new Dictionary<string, BoneModifierData>
        {
            // Belly
            {"cf_s_spine01", new BoneModifierData(new Vector3(1.45f, 1.4f, 1.8f), -4f)},
            {"cf_s_waist01", new BoneModifierData(new Vector3(1.2f, 1.28f, 1.6f), -3.5f)},
            // Skirt Front
            {"cf_d_sk_00_00", new BoneModifierData(new Vector3(1.35f, 1f, 1f), 1.95f)},
            // Skirt Front sides
            {"cf_d_sk_07_00", new BoneModifierData(new Vector3(2f, 1f, 1f), 1.5f)},
            {"cf_d_sk_01_00", new BoneModifierData(new Vector3(2f, 1f, 1f), 1.5f)},
            // Skirt Back
            {"cf_d_sk_04_00", new BoneModifierData(new Vector3(1f, 1f, 1f), 1.1f)},
            // Breasts
            {"cf_d_bust01_L", new BoneModifierData(new Vector3(1.15f, 1.15f, 1.15f), 1f)},
            {"cf_d_bust01_R", new BoneModifierData(new Vector3(1.15f, 1.15f, 1.15f), 1f)},
            // Areolas
            {"cf_s_bnip01_L", new BoneModifierData(new Vector3(1.2f, 1.2f, 1f), 1f)},
            {"cf_s_bnip01_R", new BoneModifierData(new Vector3(1.2f, 1.2f, 1f), 1f)},
            // Nipples
            {"cf_d_bnip01_L", new BoneModifierData(new Vector3(1.2f, 1.2f, 1.2f), 1f)},
            {"cf_d_bnip01_R", new BoneModifierData(new Vector3(1.2f, 1.2f, 1.2f), 1f)}
        };

        public PregnancyBoneEffect(PregnancyController controller)
        {
            _controller = controller;
        }

        public override IEnumerable<string> GetAffectedBones(BoneController origin)
        {
            if (_controller.IsDuringPregnancy()
                || MakerAPI.InsideMaker
                //|| StudioAPI.InsideStudio todo needed after adding studio slider
                )
                return _pregnancyFullValues.Keys;

            return Enumerable.Empty<string>();
        }

        public override BoneModifierData GetEffect(string bone, BoneController origin, ChaFileDefine.CoordinateType coordinate)
        {
            if (_controller.IsDuringPregnancy())
            {
                if (_pregnancyFullValues.TryGetValue(bone, out var mod))
                {
                    var bellySize = _controller.GetBellySizePercent();
                    return new BoneModifierData(
                        new Vector3(
                            Mathf.Lerp(1f, mod.ScaleModifier.x, bellySize),
                            Mathf.Lerp(1f, mod.ScaleModifier.y, bellySize),
                            Mathf.Lerp(1f, mod.ScaleModifier.z, bellySize)),
                        Mathf.Lerp(1f, mod.LengthModifier, bellySize));
                }
            }

            return null;
        }
    }
}