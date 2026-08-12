using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaWhipsBackport.Projectiles
{
    public class PossessionEyeLaserProj : ModProjectile
    {
        public override string Texture => "TerrariaWhipsBackport/Projectiles/PossessionLaser";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12; // Longitud de la estela
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2; // Guarda posiciones y rotaciones previas
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1; // Muere al primer impacto
            Projectile.timeLeft = 300; 
            Projectile.DamageType = DamageClass.SummonMeleeSpeed;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.extraUpdates = 2; // Viaja muy rÃ¡pido
            Projectile.Opacity = 0f; // Comienza invisible para el fade-in
        }

        public override void AI()
        {
            int targetIndex = (int)Projectile.ai[0];
            bool targetDead = true;
            
            if (targetIndex >= 0 && targetIndex < Main.maxNPCs)
            {
                NPC target = Main.npc[targetIndex];
                if (target.active && target.life > 0)
                {
                    targetDead = false;
                    // Primer tick: Le damos una direcciÃ³n inicial perpendicular para forzar una curva amplia
                    if (Projectile.localAI[0] == 0f)
                    {
                        Vector2 toTarget = target.Center - Projectile.Center;
                        toTarget.Normalize();
                        
                        // DesvÃ­o fuerte a la izquierda o derecha (entre 60 y 90 grados)
                        float sign = Main.rand.NextBool() ? 1f : -1f;
                        float randomAngle = sign * Main.rand.NextFloat(1.0f, 1.57f);
                        
                        // Sale disparado hacia el costado (mÃ¡s lento para que dure mÃ¡s tiempo)
                        Projectile.velocity = toTarget.RotatedBy(randomAngle) * 8f;
                        Projectile.localAI[0] = 1f;
                    }

                    // Homing: Ajustar la velocidad para que gire hacia el objetivo dibujando una curva
                    Vector2 toTargetNow = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    float speed = 16f; // Velocidad reducida (antes 24f) para que dure un 50% mÃ¡s
                    float turnSpeed = 0.06f; // Suavidad de giro ajustada a la nueva velocidad
                    
                    Projectile.velocity = (Projectile.velocity * (1f - turnSpeed)) + (toTargetNow * speed * turnSpeed);
                }
            }

            // Rotar de acuerdo a la velocidad
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (targetDead)
            {
                // Frena casi por completo (recorre como 1 bloque) y se desvanece
                Projectile.velocity *= 0.8f; 
                Projectile.Opacity -= 0.03f;
                if (Projectile.Opacity <= 0f)
                {
                    Projectile.active = false; // Desaparece silenciosamente sin generar explosiÃ³n
                }
            }
            else
            {
                // Opacidad normal (Fade-in)
                if (Projectile.localAI[1] < 24f) // 24 updates (8 ticks reales)
                {
                    Projectile.Opacity = MathHelper.Lerp(0f, 1f, Projectile.localAI[1] / 24f);
                }
                else
                {
                    Projectile.Opacity = 1f;
                }
                Projectile.localAI[1]++;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 origin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);

            // Escala base: podemos hacerla un poco mÃ¡s fina ajustando X
            Vector2 scale = new Vector2(Projectile.scale * 0.7f, Projectile.scale * 1.2f); 

            // Dibujar la estela (cola del lÃ¡ser) con los colores originales de la textura
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;

                float progress = 1f - ((float)i / Projectile.oldPos.Length);
                
                // Usamos Color.White para preservar los colores verde y blanco de la textura original
                Color trailColor = Color.White * (progress * Projectile.Opacity);
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float rot = Projectile.oldRot[i];

                // La estela se encoge ligeramente hacia atrÃ¡s (efecto cometa)
                Vector2 trailScale = new Vector2(scale.X * progress, scale.Y);

                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, rot, origin, trailScale, SpriteEffects.None, 0);
            }

            // Dibujar la cabeza del proyectil de forma normal (solo la textura sin bloom artificial)
            Vector2 centerPos = Projectile.Center - Main.screenPosition;
            Color coreColor = Color.White * Projectile.Opacity;
            
            Main.EntitySpriteDraw(texture, centerPos, null, coreColor, Projectile.rotation, origin, scale, SpriteEffects.None, 0);

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // El proyectil muere al impactar, activando Kill()
        }

        public override void Kill(int timeLeft)
        {
            // PequeÃ±a explosiÃ³n de partÃ­culas de Phantasmal Eye (Vortex)
            for (int i = 0; i < 15; i++)
            {
                int dustType = Main.rand.NextBool(2) ? 229 : 228;
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
                Dust dust = Dust.NewDustDirect(dustPos, 10, 10, dustType, 0f, 0f, 100, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
            }
        }
    }
}
