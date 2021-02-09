using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using tsorcRevamp.Projectiles;

namespace tsorcRevamp.Items.Weapons {
    public class SupremeDragoonLance : ModItem {

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Supreme Dragoon Lance");
            Tooltip.SetDefault("An all-powerful spear forged from the fang of the Dragoon Serpent.");
        }

        public override void SetDefaults() {
            item.damage = 300;
            item.knockBack = 15f;

            item.useStyle = ItemUseStyleID.HoldingOut;
            item.useAnimation = 10;
            item.useTime = 1;
            item.shootSpeed = 8;
            
            item.height = 50;
            item.width = 50;

            item.melee = true;
            item.noMelee = true;
            item.noUseGraphic = true;

            item.value = 9000000;
            item.rare = ItemRarityID.Pink;
            item.maxStack = 1;
            item.UseSound = SoundID.Item1;
            item.shoot = ModContent.ProjectileType<Projectiles.DragoonLance>();

        }

        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.GetItem("DragoonLance"), 1);
            //recipe.AddIngredient(mod.GetItem("FlameoftheAbyss"), 10);
            //recipe.AddIngredient(mod.GetItem("SoulofArtorias"), 1);
            recipe.AddIngredient(mod.GetItem("GuardianSoul"), 1);
            recipe.AddIngredient(mod.GetItem("DarkSoul"), 170000);
            recipe.SetResult(this, 1);
            recipe.AddTile(TileID.DemonAltar);
            recipe.AddRecipe();
        }
    }
}
