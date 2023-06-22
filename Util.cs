using TMPro;
using UnityEngine;

namespace HealthDisplay;

public class Util
{
    public static bool ShouldDestroyHud(EnemyHud.HudData hudData, EnemyHud __instance)
    {
        return !hudData.m_character || !__instance.TestShow(hudData.m_character, true);
    }

    public static void UpdateHud(EnemyHud.HudData hudData, Player player)
    {
        float healthPercentage = hudData.m_character.GetHealthPercentage() * 100f;
        UpdateHealthText(hudData, healthPercentage);

        hudData.m_gui.transform.localScale = HealthDisplayPlugin.HealthbarScaleEnemy.Value;

        var isPlayerOrPlayerFaction = hudData.m_character.IsPlayer() || hudData.m_character.m_faction == Character.Faction.Players;
        SetHealthBarColor(hudData, isPlayerOrPlayerFaction);

        if (hudData.m_character.IsTamed())
        {
            UpdateForTamedCharacter(hudData, player, healthPercentage);
        }
        else
        {
            hudData.m_healthFast.SetValue(healthPercentage / 100);
        }
    }

    public static void UpdateHealthText(EnemyHud.HudData hudData, float healthPercentage)
    {
        if (hudData.m_healthText != null)
        {
            int currentHealth = Mathf.CeilToInt(hudData.m_character.GetHealth());
            int totalHealth = Mathf.CeilToInt(hudData.m_character.GetMaxHealth());
            hudData.m_healthText.text = string.Format(HealthDisplayPlugin.HealthString.Value.Replace("<color>", $"<color=#{GetColor(healthPercentage)}>"), currentHealth, totalHealth, $"{healthPercentage:0.#}");
        }
    }

    public static void SetHealthBarColor(EnemyHud.HudData hudData, bool isPlayerOrPlayerFaction)
    {
        var color = isPlayerOrPlayerFaction ? HealthDisplayPlugin.PlayerHbColor.Value : HealthDisplayPlugin.EnemyHbColor.Value;
        hudData.m_healthFast.GetComponent<GuiBar>().SetColor(color);
        hudData.m_healthSlow.GetComponent<GuiBar>().SetColor(color);
    }

    public static void UpdateForTamedCharacter(EnemyHud.HudData hudData, Player player, float healthPercentage)
    {
        hudData.m_healthFast.GetComponent<GuiBar>().SetColor(HealthDisplayPlugin.TamedColor.Value);
        hudData.m_healthSlow.GetComponent<GuiBar>().SetColor(HealthDisplayPlugin.TamedColor.Value);
        hudData.m_gui.transform.localScale = HealthDisplayPlugin.HealthbarScaleTamed.Value;

        if (hudData.m_healthFastFriendly)
        {
            bool isEnemy = !player || BaseAI.IsEnemy(player, hudData.m_character);
            hudData.m_healthFast.gameObject.SetActive(isEnemy);
            hudData.m_healthFastFriendly.gameObject.SetActive(!isEnemy);
            hudData.m_healthFast.SetValue(healthPercentage / 100);
            hudData.m_healthFastFriendly.SetValue(healthPercentage / 100);
        }
        else
        {
            hudData.m_healthFast.SetValue(healthPercentage / 100);
        }
    }

    public static bool ShouldUpdateHud(EnemyHud.HudData hudData, EnemyHud __instance)
    {
        if (HealthDisplayPlugin.GroupsIsInstalled && hudData.m_gui.transform.parent.name == "Groups")
        {
            HealthDisplayPlugin.HealthDisplayLogger.LogDebug("Continuing loop since it's a player and GroupsIsInstalled is true");
            return false;
        }

        return true;
    }

    public static Transform GetHealthTransform(EnemyHud.HudData hudData)
    {
        Transform healthTransform = Utils.FindChild(hudData.m_gui.transform, "Health");
        if (healthTransform == null)
        {
            HealthDisplayPlugin.HealthDisplayLogger.LogDebug("Health transform not found, continuing loop");
        }

        return healthTransform;
    }

    public static bool HealthTextAlreadyExists(Transform healthTransform)
    {
        if (healthTransform.Find("AzuHealthText") != null)
        {
            HealthDisplayPlugin.HealthDisplayLogger.LogDebug("HealthText transform already exists, returning");
            return true;
        }

        return false;
    }

    public static GameObject InstantiateHealthObject(Transform healthTransform, EnemyHud __instance)
    {
        Transform originalHealthText = __instance.m_baseHudMount.transform.Find("Health/HealthText");
        HealthDisplayPlugin.HealthDisplayLogger.LogDebug("Instantiating healthObj...");
        GameObject healthObj = Object.Instantiate(originalHealthText.gameObject, originalHealthText);
        healthObj.name = "AzuHealthText";
        healthObj.transform.SetParent(healthTransform);
        healthObj.GetComponent<RectTransform>().anchoredPosition = Vector2.up;
        healthObj.SetActive(true);
        return healthObj;
    }

    public static Transform GetSlowTransform(Transform healthTransform)
    {
        Transform slow = healthTransform.Find("health_slow/bar");
        if (slow == null)
        {
            HealthDisplayPlugin.HealthDisplayLogger.LogDebug("Slow transform not found, continuing loop");
        }

        return slow;
    }

    public static void UpdateHealthObjectText(GameObject healthObj, Transform slow)
    {
        HealthDisplayPlugin.HealthDisplayLogger.LogDebug("Setting fontSize of healthObj text component");
        TextMeshProUGUI? tmGUI = healthObj.GetComponent<TextMeshProUGUI>();
        try
        {
            tmGUI.fontSize = (int)slow.GetComponent<RectTransform>().sizeDelta.y;
            tmGUI.alignment = TextAlignmentOptions.CaplineGeoAligned;
            tmGUI.enableWordWrapping = false;
            tmGUI.outlineWidth = 0.3f;
            tmGUI.outlineColor = Color.black;
        }
        catch
        {
        }
    }


    public static string GetColor(float percentage)
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