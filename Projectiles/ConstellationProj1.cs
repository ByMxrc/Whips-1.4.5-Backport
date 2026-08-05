using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Whips145.Buffs;
using Whips145.Common;

namespace Whips145.Projectiles
{
    public class ConstellationProj1 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5; // Longitud del rastro
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2; // Rastro con rotación y posición
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.SummonMeleeSpeed; // Daño summon del látigo
            Projectile.penetrate = 1; // Non-piercing
            Projectile.timeLeft = 120; // 2 segundos (120 ticks) de tiempo de vida
            Projectile.ignoreWater = true; // No es frenado por líquidos
            Projectile.tileCollide = true; // Rebotan en bloques
            
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // Un golpe por estrella
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Rebotar en bloques sin perder velocidad
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            return false; // Evita que se destruya al chocar
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Aplica el tag del Constellation al enemigo
            target.AddBuff(ModContent.BuffType<ConstellationTag>(), 240);
        }

        public override bool? CanHitNPC(NPC target)
        {
            // Evita golpear durante la fase de dispersión (0.5s = 30 ticks)
            if (Projectile.localAI[0] <= 30)
            {
                return false;
            }
            return null;
        }

        public override void AI()
        {
            // Apariencia: rotar hacia la dirección de movimiento
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Rastro de polvo de estrella suave (reducido)
            if (Main.rand.NextBool(4))
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.MagicMirror, 0f, 0f, 100, default, 1.2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = Projectile.velocity * 0.1f;
            }

            // localAI[0] maneja el contador del retraso (30 ticks = 0.5 seg)
            Projectile.localAI[0]++;

            if (Projectile.localAI[0] <= 30)
            {
                // Primera fase: salen volando y frenan un poco
                Projectile.velocity *= 0.95f;
            }
            else
            {
                // Segunda fase: Persecución
                NPC initialTarget = null;
                int initialTargetIndex = (int)Projectile.ai[0]; // El NPC original
                
                if (initialTargetIndex >= 0 && initialTargetIndex < Main.maxNPCs)
                {
                    NPC npc = Main.npc[initialTargetIndex];
                    if (npc.active && npc.CanBeChasedBy())
                    {
                        initialTarget = npc;
                    }
                }

                NPC targetToTrack = initialTarget;

                // Si el objetivo inicial murió o desapareció, buscar otro a 50 bloques (800 píxeles) con visión
                if (targetToTrack == null)
                {
                    float maxDist = 800f;
                    float sqrMaxDist = maxDist * maxDist;
                    NPC closest = null;
                    
                    foreach (NPC target in Main.ActiveNPCs)
                    {
                        if (target.CanBeChasedBy() && Collision.CanHit(Projectile.Center, 1, 1, target.Center, 1, 1))
                        {
                            float sqrDist = Vector2.DistanceSquared(Projectile.Center, target.Center);
                            if (sqrDist < sqrMaxDist)
                            {
                                sqrMaxDist = sqrDist;
                                closest = target;
                            }
                        }
                    }
                    targetToTrack = closest;
                }

                // Si no hay objetivo, se frena poco a poco y se desvanece
                if (targetToTrack == null)
                {
                    Projectile.velocity *= 0.95f;
                    Projectile.alpha += 10;
                    if (Projectile.alpha >= 255)
                    {
                        Projectile.Kill();
                    }
                }
                else
                {
                    // Homing hacia el objetivo mucho más rápido
                    float speed = 30f; // Aumentado (antes 16f)
                    float inertia = 15f; // Reducido para giros más rápidos (antes 20f)
                    Vector2 direction = targetToTrack.Center - Projectile.Center;
                    direction.Normalize();
                    direction *= speed;

                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);

            // Dibujar el rastro (trail)
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 drawPos = Projectile.oldPos[i] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(Color.Cyan) * ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
                color.A = 0; // Transparencia aditiva
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.oldRot[i], drawOrigin, Projectile.scale * 0.8f, SpriteEffects.None, 0);
            }

            // Dibujar la estrella principal con Bloom intenso (Additive Blending)
            float alphaFactor = (255 - Projectile.alpha) / 255f;
            Color bloomColor = new Color(50, 150, 255, 0) * 0.9f * alphaFactor; // Azul intenso con Alpha 0 para que brille (aditivo)
            for (int i = 0; i < 4; i++) // Dibujar 4 veces desplazado ligeramente para hacer un halo
            {
                Vector2 bloomPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY) + (MathHelper.TwoPi * i / 4f).ToRotationVector2() * 2f;
                Main.EntitySpriteDraw(texture, bloomPos, null, bloomColor, Projectile.rotation, drawOrigin, Projectile.scale * 1.3f, SpriteEffects.None, 0);
            }

            // Dibujar la estrella base (blanco puro en el centro), aplicando el Alpha actual
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), null, Color.White * ((255 - Projectile.alpha) / 255f), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false; // Desactivar el dibujado normal
        }

        public override void Kill(int timeLeft)
        {
            // Puf con un rastro de humo y chispas al desaparecer
            for (int i = 0; i < 6; i++)
            {
                // Humo (Smoke)
                int smoke = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1.2f);
                Main.dust[smoke].noGravity = true;
                Main.dust[smoke].velocity *= 0.2f;

                // Chispa cyan remanente
                int spark = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.MagicMirror, 0f, 0f, 100, default, 1.0f);
                Main.dust[spark].noGravity = true;
                Main.dust[spark].velocity *= 0.3f;
            }

            // Aplicar 1 segundo de cooldown al jugador. 
            // Como esto corre para cada estrella al morir, el cooldown efectivo de 1 seg cuenta desde que desaparece la ÚLTIMA estrella.
            if (Main.player[Projectile.owner].TryGetModPlayer<WhipPlayer>(out var modPlayer))
            {
                modPlayer.constellationCooldown = 45; // 60 ticks = 1 segundo
            }
        }
    }
}
