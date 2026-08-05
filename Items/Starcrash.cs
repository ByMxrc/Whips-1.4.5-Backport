using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Whips145.Projectiles;

namespace Whips145.Items
{
    /// <summary>
    /// Starcrash - Látigo de Meteorito. Marca al enemigo con energía cargada;
    /// cuando los minions golpean al enemigo marcado, cae un meteorito que hace 1.33x el daño.
    /// Daño: 18 | Tag: +2 | Receta: 20 Meteorite Bars + 10 Chains + 10 Fallen Stars en Anvil
    /// </summary>
    public class Starcrash : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToWhip(ModContent.ProjectileType<StarcrashProj>(), 18, 1.5f, 8f, 30);
            Item.width = 32;
            Item.height = 32;
            Item.value = Item.sellPrice(gold: 1, silver: 80);
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item152;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.MeteoriteBar, 20)
                .AddIngredient(ItemID.Chain, 10)
                .AddIngredient(ItemID.FallenStar, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
