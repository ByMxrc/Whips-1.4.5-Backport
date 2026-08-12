using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaWhipsBackport.Projectiles;

namespace TerrariaWhipsBackport.Items
{
    /// <summary>
    /// Vasculash - LÃ¡tigo de CarmesÃ­ (alternativa a Soulscourge).
    /// DaÃ±o: 19 | Tag: +5 | Receta: 12 Crimtane Bars en Anvil
    /// </summary>
    public class Vasculash : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToWhip(ModContent.ProjectileType<VasculashProj>(), 19, 1.25f, 8f, 30);
            Item.width = 30;
            Item.height = 30;
            Item.value = Item.sellPrice(silver: 96);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item152;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.CrimtaneBar, 12)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
