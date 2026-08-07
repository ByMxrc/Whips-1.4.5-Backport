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
            Projectile.friendly = false; // MUST be false so it doesn't hit enemies and get hitlag!
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9999;
        }

        public override bool? CanDamage()
        {
            return false; // Asegura que nunca golpee y por lo tanto nunca reciba hitlag
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

            // Prevención estricta: Si algún otro mod fuerza el disparo del arma (ignorando CanUseItem) al golpear enemigos,
            // esto creará un bucle infinito exponencial. Nos aseguramos de que solo sobreviva el manager más antiguo usando su timeLeft.
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].type == Projectile.type && Main.projectile[i].owner == Projectile.owner)
                {
                    if (i != Projectile.whoAmI)
                    {
                        // El proyectil recién creado tendrá un timeLeft mayor que el viejo.
                        if (Projectile.timeLeft > Main.projectile[i].timeLeft || (Projectile.timeLeft == Main.projectile[i].timeLeft && Projectile.whoAmI > i))
                        {
                            // Somos más nuevos que el otro manager, nos destruimos para no interferir en el ciclo del viejo
                            Projectile.Kill();
                            return;
                        }
                    }
                }
            }

            Projectile.Center = player.MountedCenter;
            Projectile.velocity = Vector2.Zero;

            ref float timer = ref Projectile.ai[0];
            ref float finishingCombo = ref Projectile.ai[1];

            // Determinamos si el jugador sigue atacando. 
            // Usamos Main.mouseLeft directamente para el dueño para ignorar cualquier desincronización del motor.
            bool isChanneling = player.channel;
            if (Main.myPlayer == Projectile.owner)
            {
                isChanneling = Main.mouseLeft;
            }

            // Si suelta el click, activamos el fin del combo inmediatamente para no pasarnos del límite (stepTimer == 29)
            if (!isChanneling)
            {
                finishingCombo = 1f;
            }
            else if (finishingCombo != 1f)
            {
                // Solo permitimos "reanudar" el combo (volver a 0) si TODAVÍA no se
                // había marcado el fin. Una vez que finishingCombo llegó a 1f, queda
                // enclavado: no debe poder "des-terminarse" a mitad del freeze.
                finishingCombo = 0f;
            }

            int stepTimer = (int)timer % 30;
            int loopCount = (int)timer / 30;

            // Forzar la animación del brazo para que parezca que está canalizando.
            // Usamos Math.Max(2, ...) para evitar que la animación llegue a 0 o 1. 
            // Si llega a 0, Terraria cree que el ataque terminó y suelta el 'channel' por 1 frame,
            // lo que accidentalmente marcaba al manager para morir y rompía el ciclo.
            player.itemAnimation = System.Math.Max(2, 30 - stepTimer);
            player.itemAnimationMax = 30;
            player.itemTime = System.Math.Max(2, 30 - stepTimer);
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
                // Contar látigos activos para evitar colapsar el límite de proyectiles en hitlags masivos
                int activeWhips = 0;
                for (int j = 0; j < Main.maxProjectiles; j++)
                {
                    if (Main.projectile[j].active && Main.projectile[j].type == realType && Main.projectile[j].owner == player.whoAmI)
                        activeWhips++;
                }

                if (activeWhips < 30) // Límite seguro
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
            }

            // Si completó las 3 olas (30 frames) y soltó el click
            if (stepTimer == 29 && finishingCombo == 1f)
            {
                // Buscamos si aún quedan látigos vivos
                int activeWhips = 0;
                if (Main.myPlayer == Projectile.owner)
                {
                    for (int j = 0; j < Main.maxProjectiles; j++)
                    {
                        if (Main.projectile[j].active && Main.projectile[j].type == realType && Main.projectile[j].owner == player.whoAmI)
                            activeWhips++;
                    }
                }

                if (activeWhips == 0)
                {
                    // Si ya no quedan látigos, el manager muere de forma segura
                    Projectile.Kill();
                    return;
                }
                else
                {
                    // Si hay látigos vivos, congelamos el manager en el frame 29 para mantener forzada la animación de ataque
                    player.itemAnimation = 2;
                    player.itemAnimationMax = 30;
                    player.itemTime = 2;
                    player.itemTimeMax = 30;
                    return; // No avanza el timer
                }
            }

            timer++;
        }
    }
}