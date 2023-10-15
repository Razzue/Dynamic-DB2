using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicDB2.DBUtility;

internal enum WowDbd
{
    Unknown = -1,
    Spell,
    SpellActionBarPref,
    SpellActivationOverlay,
    SpellAuraNames,
    SpellAuraOptions,
    SpellAuraRestrictions,
    SpellAuraRestrictionsDifficulty,
    SpellAuraVisXChrSpec,
    SpellAuraVisXTalentTab,
    SpellAuraVisibility,
    SpellCastTimes,
    SpellCastingRequirements,
    SpellCategories,
    SpellCategory,
    SpellChainEffects,
    SpellClassOptions,
    SpellClutterAreaEffectCounts,
    SpellClutterFrameRates,
    SpellClutterImpactModelCounts,
    SpellClutterKitDistances,
    SpellClutterMissileDist,
    SpellClutterWeaponTrailDist,
    SpellCooldowns,
    SpellCraftUI,
    SpellDescriptionVariables,
    SpellDifficulty,
    SpellDispelType,
    SpellDuration,
    SpellEffect,
    SpellEffectAutoDescription,
    SpellEffectCameraShakes,
    SpellEffectEmission,
    SpellEffectGroupSize,
    SpellEffectNames,
    SpellEffectScaling,
    SpellEmpower,
    SpellEmpowerStage,
    SpellEquippedItems,
    SpellFlyout,
    SpellFlyoutItem,
    SpellFocusObject,
    SpellIcon,
    SpellInterrupts,
    SpellItemEnchantment,
    SpellItemEnchantmentCondition,
    SpellKeyboundOverride,
    SpellLabel,
    SpellLearnSpell,
    SpellLevels,
    SpellMastery,
    SpellMechanic,
    SpellMemorizeCost,
    SpellMisc,
    SpellMiscDifficulty,
    SpellMissile,
    SpellMissileMotion,
    SpellName,
    SpellOverrideName,
    SpellPower,
    SpellPowerDifficulty,
    SpellProceduralEffect,
    SpellProcsPerMinute,
    SpellProcsPerMinuteMod,
    SpellRadius,
    SpellRange,
    SpellReagents,
    SpellReagentsCurrency,
    SpellReplacement,
    SpellRuneCost,
    SpellScaling,
    SpellScript,
    SpellScriptText,
    SpellShapeshift,
    SpellShapeshiftForm,
    SpellSpecialUnitEffect,
    SpellTargetRestrictions,
    SpellTooltip,
    SpellTotems,
    SpellVisual,
    SpellVisualAnim,
    SpellVisualAnimName,
    SpellVisualColorEffect,
    SpellVisualEffectName,
    SpellVisualEvent,
    SpellVisualKit,
    SpellVisualKitAreaModel,
    SpellVisualKitEffect,
    SpellVisualKitModelAttach,
    SpellVisualKitPicker,
    SpellVisualKitPickerEntry,
    SpellVisualMissile,
    SpellVisualPrecastTransitions,
    SpellVisualScreenEffect,
    SpellXDescriptionVariables,
    SpellXSpellVisual,
    Maximum
}

internal static class WowDbdHelper
{

    /// <summary>
    /// Get a string array of <see cref="WowDatabase"/> formatted for .db2 file type.>
    /// </summary>
    internal static string[] GetDb2Names()
    {
        var results = new string[(int)WowDbd.Maximum];
        try
        {
            for (var i = 0; i < (int)WowDbd.Maximum; i++)
                results[i] = ((WowDbd)i).Db2String();
        }
        catch (Exception e) { Console.WriteLine(e); }
        return results;
    }

    /// <summary>
    /// Get a string array of <see cref="WowDatabase"/> formatted for .dbd file type.>
    /// </summary>
    internal static string[] GetDbdNames()
    {
        var results = new string[(int)WowDbd.Maximum];
        try
        {
            for (var i = 0; i < (int)WowDbd.Maximum; i++)
                results[i] = ((WowDbd)i).DbdString();
        }
        catch (Exception e) { Console.WriteLine(e); }
        return results;
    }

    /// <summary>
    /// Convert a <see cref="WowDatabase"/> to a .db2 file <see cref="string"/> 
    /// </summary>
    internal static string Db2String(this WowDbd wdb)
        => $"{wdb.ToString().ToLower()}.db2";

    /// <summary>
    /// Convert a <see cref="WowDatabase"/> to a .dbd file <see cref="string"/> 
    /// </summary>
    internal static string DbdString(this WowDbd wdb)
        => $"{wdb}.dbd";
}