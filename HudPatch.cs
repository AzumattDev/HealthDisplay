﻿using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace HealthDisplay;

[HarmonyPatch(typeof(EnemyHud), nameof(EnemyHud.UpdateHuds))]
static class EnemyHudUpdateHudsPatch
{
    static void Postfix(ref EnemyHud __instance, Player player, Sadle sadle, float dt)
    {
        Character key = null!;
        foreach (EnemyHud.HudData value in __instance.m_huds.Select(keyValuePair => keyValuePair.Value))
        {
            if (!value.m_character || !__instance.TestShow(value.m_character))
            {
                if (key != null) continue;
                key = value.m_character;
                Object.Destroy(value.m_gui);
            }
            else
            {
                float healthPercentage = value.m_character.GetHealthPercentage() * 100f;
                int currentHealth = Mathf.CeilToInt(value.m_character.GetHealth());
                int totalHealth = Mathf.CeilToInt(value.m_character.GetMaxHealth());
                value.m_healthText.text = string.Format(
                    HealthDisplayPlugin.HealthString.Value.Replace("<color>",
                        $"<color=#{GetColor(healthPercentage)}>"),
                    currentHealth, totalHealth, $"{healthPercentage:0.#}"
                );
                value.m_gui.transform.localScale = HealthDisplayPlugin.HealthbarScaleEnemy.Value;
                value.m_gui.transform.Find("Health/health_fast").GetComponent<GuiBar>()
                    .SetColor(HealthDisplayPlugin.EnemyHbColor.Value);
                value.m_gui.transform.Find("Health/health_slow").GetComponent<GuiBar>()
                    .SetColor(HealthDisplayPlugin.EnemyHbColor.Value);
                if (!value.m_character.IsTamed()) continue;
                value.m_gui.transform.Find("Health/health_fast").GetComponent<GuiBar>()
                    .SetColor(HealthDisplayPlugin.TamedColor.Value);
                value.m_gui.transform.Find("Health/health_slow").GetComponent<GuiBar>()
                    .SetColor(HealthDisplayPlugin.TamedColor.Value);
                value.m_gui.transform.localScale = HealthDisplayPlugin.HealthbarScaleTamed.Value;
            }
        }

        if (!(key != null))
            return;
        __instance.m_huds.Remove(key);
    }

    private static string GetColor(float percentage)
    {
        string color = percentage switch
        {
            >= 75f => ColorUtility.ToHtmlStringRGBA(HealthDisplayPlugin.HighPercentColor.Value),
            >= 50f => ColorUtility.ToHtmlStringRGBA(HealthDisplayPlugin.HurtPercentColor.Value),
            >= 25f => ColorUtility.ToHtmlStringRGBA(HealthDisplayPlugin.LowPercentColor.Value),
            _ => ColorUtility.ToHtmlStringRGBA(HealthDisplayPlugin.CriticalPercentColor.Value)
        };

        return color;
    }
}

[HarmonyPatch(typeof(EnemyHud), nameof(EnemyHud.ShowHud))]
static class EnemyHudShowHudPatch
{
    static bool Prefix(ref EnemyHud __instance, Character c, bool isMount)
    {
        // Take over the vanilla method and change it a small bit. This is basically the same as the original method, but with the health text changed.
        if (__instance.m_huds.TryGetValue(c, out EnemyHud.HudData hudData)) return false;
        GameObject original = !isMount
            ? (!c.IsPlayer()
                ? (!c.IsBoss() ? __instance.m_baseHud : __instance.m_baseHudBoss)
                : __instance.m_baseHudPlayer)
            : __instance.m_baseHudMount;
        hudData = new EnemyHud.HudData
        {
            m_character = c,
            m_ai = c.GetComponent<BaseAI>(),
            m_gui = Object.Instantiate(original, __instance.m_hudRoot.transform)
        };
        hudData.m_gui.SetActive(true);
        hudData.m_healthFast = hudData.m_gui.transform.Find("Health/health_fast").GetComponent<GuiBar>();
        hudData.m_healthSlow = hudData.m_gui.transform.Find("Health/health_slow").GetComponent<GuiBar>();
        if (isMount)
        {
            hudData.m_stamina = hudData.m_gui.transform.Find("Stamina/stamina_fast").GetComponent<GuiBar>();
            hudData.m_staminaText = hudData.m_gui.transform.Find("Stamina/StaminaText").GetComponent<Text>();
        }

        hudData.m_healthText = hudData.m_gui.transform.Find("Health/HealthText").GetComponent<Text>();
        hudData.m_level2 = hudData.m_gui.transform.Find("level_2") as RectTransform;
        hudData.m_level3 = hudData.m_gui.transform.Find("level_3") as RectTransform;
        hudData.m_alerted = hudData.m_gui.transform.Find("Alerted") as RectTransform;
        hudData.m_aware = hudData.m_gui.transform.Find("Aware") as RectTransform;
        hudData.m_name = hudData.m_gui.transform.Find("Name").GetComponent<Text>();
        hudData.m_name.text = Localization.instance.Localize(c.GetHoverName());
        hudData.m_isMount = isMount;
        __instance.m_huds.Add(c, hudData);
        return false;
    }
}

[HarmonyPatch(typeof(Hud), nameof(Hud.Awake))]
static class HudAwakePatch
{
    static void Postfix(Hud __instance)
    {
        Transform originalHealthText = EnemyHud.instance.m_baseHudMount.transform.Find("Health/HealthText");
        foreach (Transform t in EnemyHud.instance.m_hudRoot.transform)
        {
            Transform healthTransform = t.Find("Health");
            if (!healthTransform) continue;
            if (healthTransform.Find("HealthText")) return;
            GameObject healthObj =
                Object.Instantiate(originalHealthText.gameObject, originalHealthText);
            healthObj.name = "HealthText";
            healthObj.transform.SetParent(healthTransform);
            healthObj.GetComponent<RectTransform>().anchoredPosition = Vector2.up;
            healthObj.SetActive(true);
            Transform darken = healthTransform.Find("darken");
            Transform background = healthTransform.Find("bkg");
            Transform slow = healthTransform.Find("health_slow/bar");
            Transform fast = healthTransform.Find("health_fast/bar");
            if (!slow) continue;
            if (slow.GetComponent<RectTransform>().sizeDelta.y < 12)
            {
                slow.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 12f);
                if (fast) fast.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 12f);
                if (darken) darken.GetComponent<RectTransform>().sizeDelta = new Vector2(15f, 15f);
                if (background) background.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 8f);
            }

            healthObj.GetComponent<Text>().fontSize = (int)slow.GetComponent<RectTransform>().sizeDelta.y;
        }
    }
}