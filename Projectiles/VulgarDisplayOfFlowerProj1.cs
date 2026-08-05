using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace Whips145.Projectiles
{
    public class VulgarDisplayOfFlowerProj1 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // Ajusta este valor al número real de frames de tu sprite.
            // Si la imagen es 18x160 y cada frame es cuadrado (18x20), son 8 frames.
            Main.projFrames[Projectile.type] = 8;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.SummonMeleeSpeed;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120; // 2 segundos de vida
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ArmorPenetration = 50; // Ignora hasta 50 de defensa
        }

        public override bool? CanHitNPC(NPC target)
        {
            // Evita golpear durante el primer medio segundo (30 ticks) al enemigo del que salieron
            if (Projectile.timeLeft > 120 - 30)
            {
                return false;
            }
            return null;
        }

        public override void AI()
        {
            // Animación
            if (++Projectile.frameCounter >= 4)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= Main.projFrames[Projectile.type])
                {
                    Projectile.frame = 0;
                }
            }

            // Inicialización de tiempo de expansión (0.33s a 0.67s = 20 a 40 ticks)
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = Main.rand.Next(20, 41);
            }

            // Fase de expansión
            if (Projectile.localAI[0] > 1)
            {
                Projectile.localAI[0]--;
                Projectile.velocity *= 0.95f; // Frena suavemente
                Projectile.rotation += Projectile.velocity.X * 0.05f; // Rota un poco visualmente
            }
            else // Fase de persecución
            {
                float maxDetectRadius = 340f; // 21.25 tiles
                float sqrMaxDetectRadius = maxDetectRadius * maxDetectRadius;
                NPC closestNPC = null;
                float sqrMinDistance = sqrMaxDetectRadius;

                foreach (NPC target in Main.ActiveNPCs)
                {
                    if (target.CanBeChasedBy())
                    {
                        float sqrDistance = Vector2.DistanceSquared(target.Center, Projectile.Center);
                        if (sqrDistance < sqrMinDistance)
                        {
                            sqrMinDistance = sqrDistance;
                            closestNPC = target;
                        }
                    }
                }

                if (closestNPC != null)
                {
                    float speed = 14f;
                    float inertia = 15f;
                    Vector2 direction = closestNPC.Center - Projectile.Center;
                    direction.Normalize();
                    direction *= speed;

                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;

                    // Si está persiguiendo a un objetivo, se asegura de ser completamente visible
                    if (Projectile.alpha > 0)
                        Projectile.alpha = Math.Max(0, Projectile.alpha - 15);
                }
                else
                {
                    // Si no hay objetivos (o el que perseguía murió), se frena poco a poco y se desvanece
                    Projectile.velocity *= 0.98f;
                    Projectile.alpha += 10;
                    if (Projectile.alpha >= 255)
                    {
                        Projectile.Kill();
                    }
                }
                
                // Orientar en la dirección de movimiento
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }

            // Partículas de polvo opcionales para efecto visual extra
            if (Main.rand.NextBool(3))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Grass, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
            }
        }

        public override void Kill(int timeLeft)
        {
            // Partículas rosas que se desvanecen en el lugar donde el pétalo desaparece
            for (int i = 0; i < 5; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PinkSlime, 0f, 0f, 150, default, 1.2f);
                Main.dust[dust].noGravity = true; // Flotan en el lugar
                Main.dust[dust].velocity *= 0.3f; // Casi no se mueven para desaparecer ahí mismo
            }
        }
    }
}
