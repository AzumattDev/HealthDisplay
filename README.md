# Health Display

Shows the HP values and percentage of creatures and bosses.

- Note: The "health" isn't accurate to damage numbers due to how the game scales player damage. The health is just a
  baseline of how strong the creature might be, and it's recommended to use percent values.

## Client and Server Side

* Optionally install this on server for version check and ServerSync

## Compatible Mods

* This mod is compatible
  with [Creature Level and Loot Control](https://valheim.thunderstore.io/package/Smoothbrain/CreatureLevelAndLootControl/)
* This mod is compatible
  with [WackyEpicMMOSystem](https://valheim.thunderstore.io/package/WackyMole/WackyEpicMMOSystem/)
* This mod is compatible with [Groups](https://valheim.thunderstore.io/package/Smoothbrain/Groups/)

## Customized HealthBar

* All configuration is made within the **Azumatt.HealthDisplay.cfg** and is live. Highly recommend using
  [Official BepInEx ConfigurationManager](https://valheim.thunderstore.io/package/Azumatt/Official_BepInEx_ConfigurationManager/)
* Change scale of enemy/tamed bar
* Change color of enemy bar/tamed bar
* Change color of percentage [Max, Mid, Low, Critical]

![https://media.discordapp.net/attachments/1046680688822321193/1046680869076733983/image.png](https://media.discordapp.net/attachments/1046680688822321193/1046680869076733983/image.png)
![https://media.discordapp.net/attachments/1046680688822321193/1046682639597326336/image.png](https://media.discordapp.net/attachments/1046680688822321193/1046682639597326336/image.png)

---

<details><summary><b>Configuration Options (Click to expand)</b></summary>

### How they look in [Official BepInEx ConfigurationManager](https://valheim.thunderstore.io/package/Azumatt/Official_BepInEx_ConfigurationManager/)

![https://i.imgur.com/Nv8rTyX.png](https://i.imgur.com/Nv8rTyX.png)

`1 - General`

Lock Configuration [Synced with Server]

* If on, the configuration is locked and can be changed by server admins only.
    * Default Value: On

Health String Format [Synced with Server]

* Creature health format
    * '{0}' is current health value
    * '{1}' is total health value
    * '{2}' is health percentage value
        * Default Value: {0}/{1} (<color>{2}%</color>)

`2 - Colors`

Tamed HB Color [Synced with Server]

* Color of the health bar for tamed creatures. This is the bar that is on top.
    * Default Value: 339E66FF

Player HB Color [Synced with Server]

* Color of the health bar for players and creatures in the player faction. This is the bar that is the top bar.
    * Default Value: 339E66FF

Enemy HB Color [Synced with Server]

* Color of the health bar for enemy creatures. This is the bar that is the top bar.
    * Default Value: CC3333FF

High Percent Color [Synced with Server]

* Color of the health bar's percentage text for creatures with high health percentage. 75% or higher.
    * Default Value: 339E66FF

Hurt Percent Color [Synced with Server]

* Color of the health bar's percentage text for creatures with relatively high health percentage. 50% or higher.
    * Default Value: CCCC33FF

Low Percent Color [Synced with Server]

* Color of the health bar's percentage text for creatures with low health percentage. 25% or higher.
    * Default Value: CC6633FF

Critical Percent Color [Synced with Server]

* Color of the health bar's percentage text for creatures with critical health percentage. 0% or higher.
    * Default Value: CC3333FF

`3 - Scaling`

Tamed Healthbar Scale [Synced with Server]

* Scale of the health bar for tamed creatures.
    * Default Value: {"x":1.0,"y":1.0,"z":1.0}

Enemy Healthbar Scale [Synced with Server]

* Scale of the health bar for creatures.
    * Default Value: {"x":1.0,"y":1.0,"z":1.0}

</details>

## Credits:

Made at my request ("@Majestic"), all credit and creation of this mod goes to @Azumatt!

For Questions or Comments, find us in the Odin Plus Team Discord or mine:

[![https://i.imgur.com/XXP6HCU.png](https://i.imgur.com/XXP6HCU.png)](https://discord.gg/Pb6bVMnFb2)
<a href="https://discord.gg/pdHgy6Bsng"><img src="https://i.imgur.com/Xlcbmm9.png" href="https://discord.gg/pdHgy6Bsng" width="175" height="175"></a>
***