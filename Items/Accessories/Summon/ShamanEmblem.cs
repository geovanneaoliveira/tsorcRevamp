using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace tsorcRevamp.Items.Accessories.Summon
{
    public class ShamanEmblem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("+13% minion damage, +1 minion slot");
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.value = PriceByRarity.LightRed_4;
            Item.rare = ItemRarityID.LightRed;
        }

        public override void AddRecipes()
        {
            Terraria.Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.SummonerEmblem, 1);
            recipe.AddIngredient(ItemID.PygmyNecklace, 1);
            recipe.AddIngredient(ItemID.HallowedBar, 5);
            recipe.AddIngredient(ItemID.SoulofNight, 5);
            recipe.AddIngredient(Mod.Find<ModItem>("DarkSoul").Type, 5000);
            recipe.AddTile(TileID.DemonAltar);

            recipe.Register();
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Summon) += 0.13f;
            player.maxMinions += 1;
        }
    }
}