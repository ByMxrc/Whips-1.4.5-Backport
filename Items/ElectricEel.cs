using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Whips145.Projectiles;

namespace Whips145.Items
{
    /// <summary>
    /// Electric Eel - Drop de Duke Fishron (25%).
    /// Crea un aura eléctrica alrededor del enemigo marcado que daña a los cercanos.
    /// Daño: 150 | Tag: +12 | Multi-hit penalty: 5%
    /// </summary>
    public class ElectricEel : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToWhip(ModContent.ProjectileType<ElectricEelProj>(), 150, 3f, 8f, 30);
            Item.width = 36;
            Item.height = 36;
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ItemRarityID.Cyan; // Rareza cyan (Duke Fishron tier)
            Item.UseSound = SoundID.Item152;
        }

        // Se obtiene como drop de Duke Fishron (ver WhipDropGlobalNPC)
    }
}
