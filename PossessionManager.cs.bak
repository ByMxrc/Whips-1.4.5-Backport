using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Whips145.Projectiles
{
    public class PossessionManager : ModProjectile
    {
        // Usamos una textura existente para evitar AssetLoadException,
        // aunque nunca se dibujará porque PreDraw retorna false.
        public override string Texture => "Whips145/Projectiles/PossessionProj";

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9999;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // Si el jugador muere, se desactiva, o cambia de arma, destruimos el manager
            if (player.dead || !player.active || player.HeldItem.type != ModContent.ItemType<Items.Possession>())
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = player.MountedCenter;
            Projectile.velocity = Vector2.Zero;

            ref float timer = ref Projectile.ai[0];
            ref float finishingCombo = ref Projectile.ai[1];

            // Si soltó el click, marcamos que debe terminar el combo actual y morir
            if (!player.channel && finishingCombo == 0f)
            {
                finishingCombo = 1f;
            }

            int stepTimer = (int)timer % 30;
            int loopCount = (int)timer / 30;

            // Forzar la animación del brazo para que parezca que está canalizando
            player.itemAnimation = 30 - stepTimer;
            player.itemAnimationMax = 30;
            player.itemTime = 30 - stepTimer;
            player.itemTimeMax = 30;
            
            if (Main.myPlayer == Projectile.owner)
            {
                player.direction = (Main.MouseWorld.X > player.Center.X) ? 1 : -1;
            }

            int realType = ModContent.ProjectileType<PossessionProj>();
            int damage = Projectile.damage;
            float knockback = Projectile.knockBack;
            float shootSpeed = player.HeldItem.shootSpeed;
            Vector2 whipVelocity = Vector2.Zero;
            
            if (Main.myPlayer == Projectile.owner)
            {
                whipVelocity = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX) * shootSpeed;
            }

            // Evitar spawns en clientes si no son el dueño
            if (Main.myPlayer == Projectile.owner)
            {
                if (stepTimer == 0)
                {
                    if (loopCount == 0)
                    {
                        // Ola 1 Inicial: 2 latigos, sin cruzarse (uno con 1 tick de delay para evitar solapamiento visual idéntico)
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.MountedCenter, whipVelocity, realType, damage, knockback, player.whoAmI, 0f, 0f, 0f);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.MountedCenter, whipVelocity, realType, damage, knockback, player.whoAmI, 0f, 0f, 1f);
                    }
                    else
                    {
                        // Ola 1 Bucle: 2 latigos cruzados
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.MountedCenter, whipVelocity, realType, damage, knockback, player.whoAmI, 0f, 11f, 0f);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.MountedCenter, whipVelocity, realType, damage, knockback, player.whoAmI, 0f, 12f, 0f);
                    }
                }
                else if (stepTimer == 10)
                {
                    // Ola 2: 1 latigo normal
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.MountedCenter, whipVelocity, realType, damage, knockback, player.whoAmI, 0f, 2f, 0f);
                }
                else if (stepTimer == 20)
                {
                    // Ola 3: 1 latigo invertido al estirarse
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.MountedCenter, whipVelocity, realType, damage, knockback, player.whoAmI, 0f, 4f, 0f);
                }
            }

            // Si completó las 3 olas (30 frames) y soltó el click, se destruye
            if (stepTimer == 29 && finishingCombo == 1f)
            {
                Projectile.Kill();
                return;
            }

            timer++;
        }
    }
}
