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
            Item.useTime = 30; // Un solo disparo por click, nosotros controlamos los demás con delay
            Item.width = 30;
            Item.height = 30;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Red; // Rarity Red (Raridad 10 - Moon Lord tier)
            Item.UseSound = SoundID.Item152;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Disparamos TODAS las olas al mismo tiempo, pero les pasamos un "retraso" (delay) a través de ai[2].
            // De esta manera, sin importar si mantienes el click o tu velocidad de ataque, saldrán perfectamente en orden.

            // Ola 1: Primer par normal (Delay = 0 para el primero, 1 tick para el segundo para que se separen ligeramente)
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, 0f, 0f); // Normal
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, 1f, 1f); // 1 tick de delay

            // Ola 2: Un solo látigo normal (Delay = 33% del tiempo)
            float delay2 = player.itemAnimationMax * 0.33f;
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, 2f, delay2); // Normal

            // Ola 3: El 4to látigo, dinámico (Delay = 66% del tiempo)
            float delay3 = player.itemAnimationMax * 0.66f;
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, 4f, delay3);

            return false; // Prevent default shoot
        }
    }
}
