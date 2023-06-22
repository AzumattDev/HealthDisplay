using System;
using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HealthDisplay;

[HarmonyPatch(typeof(EnemyHud), nameof(EnemyHud.UpdateHuds))]
static class EnemyHudUpdateHudsPatch
{
    static void Postfix(ref EnemyHud __instance, Player player, Sadle sadle, float dt)
    {
        Character keyToRemove = null;

        foreach (var hudData in __instance.m_huds.Values)
        {
            if (Util.ShouldDestroyHud(hudData, __instance))
            {
                keyToRemove ??= hudData.m_character;
                Object.Destroy(hudData.m_gui);
            }
            else
            {
                Util.UpdateHud(hudData, player);
            }
        }

        if (keyToRemove != null)
        {
            __instance.m_huds.Remove(keyToRemove);
        }
    }
}

[HarmonyPatch(typeof(EnemyHud), nameof(EnemyHud.ShowHud))]
static class EnemyHudShowHudPatch
{
    static void Prefix(ref EnemyHud __instance, ref Character c, ref bool __state)
    {
        __state = __instance.m_huds.ContainsKey(c);
    }


    static void Postfix(ref EnemyHud __instance, ref Character c, ref bool isMount, ref bool __state)
    {
        if (__state || !__instance.m_huds.TryGetValue(c, out EnemyHud.HudData hudData))
        {
            return;
        }

        if (isMount) return;

        try
        {
            if (!Util.ShouldUpdateHud(hudData, __instance))
            {
                return;
            }

            Transform healthTransform = Util.GetHealthTransform(hudData);
            if (healthTransform == null)
            {
                return;
            }

            if (Util.HealthTextAlreadyExists(healthTransform))
            {
                return;
            }

            GameObject healthObj = Util.InstantiateHealthObject(healthTransform, __instance);
            Transform slowTransform = Util.GetSlowTransform(healthTransform);
            if (slowTransform == null)
            {
                return;
            }

            Util.UpdateHealthObjectText(healthObj, slowTransform);
        }
        catch (Exception e)
        {
            HealthDisplayPlugin.HealthDisplayLogger.LogDebug($"Error in foreach loop: on {hudData.m_gui.transform.name} skipping, but you should know about it." + e);
        }

        hudData.m_healthText = Utils.FindChild(hudData.m_gui.transform, "AzuHealthText").GetComponent<TextMeshProUGUI>();
    }
}