using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaWhipsBackport.Projectiles;

namespace TerrariaWhipsBackport.Items
{
    /// <summary>
    /// Soulscourge - LÃ¡tigo de CorrupciÃ³n (alternativa a Vasculash).
    /// DaÃ±o: 17 | Tag: +5 | Receta: 12 Demonite Bars en Anvil
    /// </summary>
    public class Soulscourge : ModItem
    {
        public override string Texture => "Terraria/Images/Item_5474";

        public override void SetDefaults()
        {
            Item.DefaultToWhip(ModContent.ProjectileType<SoulscourgeProj>(), 17, 1.35f, 8f, 30);
            Item.width = 30;
            Item.height = 30;
            Item.value = Item.sellPrice(silver: 96);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item152;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.DemoniteBar, 12)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
