using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Whips145.Projectiles;

namespace Whips145.Items
{
    /// <summary>
    /// Possession - Látigo Drop de Moon Lord (25%).
    /// Látigo simple sin efectos adicionales, debuffs ni marcas.
    /// Daño: 110 (Invocador) | Knockback: 4 (Débil) | Use Time: 30 (Promedio) | Rango: 34 tiles | Velocidad: 8
    /// </summary>
    public class Possession : ModItem
    {
        public override void SetDefaults()
        {
            // DefaultToWhip configura useTime, useAnimation, shoot, shootSpeed, damage, knockback, etc.
            Item.DefaultToWhip(ModContent.ProjectileType<PossessionProj>(), 110, 4f, 8f, 30); // 30 ticks = 0.50 segundos
            Item.width = 30;
            Item.height = 30;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Red; // Rarity Red (Raridad 10 - Moon Lord tier)
            Item.UseSound = SoundID.Item152;
            
            // Convertimos el arma en una canalizada (como el Zenith o Solar Eruption)
            // Esto evade por completo las mecánicas estrictas de swing de Terraria que causan pausas
            Item.autoReuse = true;
            Item.channel = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.shoot = ModContent.ProjectileType<PossessionManager>();
        }

        public override bool CanUseItem(Player player)
        {
            // Prevenir spam de clicks que cree múltiples managers simultáneos
            return player.ownedProjectileCounts[ModContent.ProjectileType<PossessionManager>()] <= 0;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Dejamos que el motor dispare el PossessionManager de forma natural
            return true;
        }
    }
}
