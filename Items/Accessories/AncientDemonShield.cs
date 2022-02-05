﻿using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace tsorcRevamp.Items.Accessories {
    [AutoloadEquip(EquipType.Shield)]
    public class AncientDemonShield : ModItem {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Powerful, but slows movement by 25%" +
                               "\nGreat Shield that grants immunity to knockback and gives thorns effect" +
                               "\nAlso provides immunity to fire blocks");
        }

        public override void SetDefaults() {
            item.width = 28;
            item.height = 38;
            item.defense = 10;
            item.accessory = true;
            item.value = PriceByRarity.Orange_3;
            item.rare = ItemRarityID.Orange;
        }

        public override void UpdateEquip(Player player) {
            player.noKnockback = true;
            player.moveSpeed -= 0.25f;
            player.thorns = 1f;
            player.fireWalk = true;
            player.endurance += 0.06f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            //only insert the tooltip if the last valid line is not the name, the "Equipped in social slot" line, or the "No stats will be gained" line (aka do not insert if in a vanity slot)
            int ttindex = tooltips.FindLastIndex(t => t.mod == "Terraria" && t.Name != "ItemName" && t.Name != "Social" && t.Name !=  "SocialDesc" && !t.Name.Contains("Prefix"));
            if (ttindex != -1) {// if we find one
                //insert the extra tooltip line
                tooltips.Insert(ttindex + 1, new TooltipLine(mod, "RevampShieldDR", "Reduces damage taken by 6%"));
            }
        }

        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.ObsidianShield);
            recipe.AddIngredient(ModContent.ItemType<Accessories.SpikedIronShield>());
            recipe.AddIngredient(ModContent.ItemType<Items.DarkSoul>(), 10000);
            recipe.AddTile(TileID.DemonAltar);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }
    }
}
