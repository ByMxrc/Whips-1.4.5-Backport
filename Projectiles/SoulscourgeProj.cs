using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaWhipsBackport.Buffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TerrariaWhipsBackport.Projectiles
{
    public class SoulscourgeProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();
            Projectile.WhipSettings.Segments = 18;
            Projectile.WhipSettings.RangeMultiplier = 0.43f; // Reducido para que el alcance sea de 14 bloques
        }

        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];
            
            // Actualizamos los segmentos segÃºn el multiplicador
            Projectile.WhipSettings.Segments = (int)(18 * player.whipRangeMultiplier);

            // Recreamos la IA nativa del lÃ¡tigo para evitar que corra la original y ponga el sonido
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.Center = Main.GetPlayerArmPosition(Projectile);
            Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;

            Projectile.ai[0]++;

            player.heldProj = Projectile.whoAmI;
            player.itemTime = player.itemAnimation;
            player.itemRotation = (Projectile.velocity * Projectile.spriteDirection).ToRotation();

            float timeToFlyOut = player.itemAnimationMax * Projectile.MaxUpdates;

            if (Projectile.ai[0] >= timeToFlyOut || player.itemAnimation <= 0)
            {
                Projectile.Kill();
                return false;
            }

            // Reproducir el sonido nativo justo a la mitad de la extensiÃ³n
            if (Projectile.ai[0] == (int)(timeToFlyOut / 2f))
            {
                SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.position);
            }

            // Generar partÃ­culas a lo largo del lÃ¡tigo (restaurando lo que hacÃ­a la IA nativa)

            return false; // Retornamos false para evitar que corra la IA nativa del lÃ¡tigo
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Evitar efectos en muÃ±ecos de pruebas o animales
            if (target.friendly || target.type == NPCID.TargetDummy || target.lifeMax <= 5 || NPCID.Sets.CountsAsCritter[target.type])
                return;

            Projectile.damage = (int)(Projectile.damage * (1f - 0.4f));
            // Tag de Soulscourge (5 segundos = 300 ticks) - forzando ignorar inmunidad
            target.buffImmune[ModContent.BuffType<SoulscourgeTag>()] = false;
            target.AddBuff(ModContent.BuffType<SoulscourgeTag>(), 300);

            // PartÃ­culas verdes de corrupciÃ³n (Verde claro como el Scourge of the Corruptor)

        }
    
        public override bool PreDraw(ref Color lightColor)
        {
            List<Vector2> list = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, list);

            Terraria.Main.instance.LoadProjectile(Type);
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            int totalFrames = 6;
            int frameWidth = texture.Width;
            int frameHeight = texture.Height / totalFrames;
            Vector2 origin = new Vector2(frameWidth / 2f, 4f); 

            Vector2 pos = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                Rectangle frame = new Rectangle(0, 0, frameWidth, frameHeight);

                if (i == 0) frame.Y = 0;
                else if (i == list.Count - 2) frame.Y = frameHeight * (totalFrames - 1);
                else
                {
                    int midFrame = 1 + (i % (totalFrames - 2));
                    frame.Y = frameHeight * midFrame;
                }

                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;
                float rotation = diff.ToRotation() - MathHelper.PiOver2;
                Color color = Lighting.GetColor(element.ToTileCoordinates());

                // 3 zonas de escala independientes:
                // - Mango (i==0): escala propia
                // - Cuerpo: progresiÃ³n suave de pequeÃ±o a grande
                // - Punta: escala propia
                float finalScale;
                if (i == 0)
                {
                    finalScale = 0.75f; // Mango
                }
                else if (i == list.Count - 2)
                {
                    finalScale = 0.95f; // Punta
                }
                else
                {
                    float bodyProgress = (float)(i - 1) / (float)(list.Count - 3);
                    finalScale = MathHelper.Lerp(0.7f, 0.95f, bodyProgress); // Cuerpo
                }

                // --- ESTIRAMIENTO de los primeros segmentos del cuerpo ---
                // Los segmentos iniciales se estiran en Y para llenar huecos y dar
                // el efecto visual de la wiki (ojos un poco ovalados cerca del mango).
                // Ajusta estos valores a gusto:
                int earlyStretchCount = 21;    // CuÃ¡ntos segmentos del inicio se estiran
                float earlyStretchMax  = 1.35f; // MÃ¡ximo estiramiento (1.0 = nada, 1.5 = mucho)

                float stretchY = 1.0f;
                if (i > 0 && i <= earlyStretchCount)
                {
                    float t = 1f - (float)(i - 1) / earlyStretchCount; // 1.0 cerca del mango, 0.0 al final
                    stretchY = MathHelper.Lerp(1.0f, earlyStretchMax, t);
                }

                Vector2 scale = new Vector2(finalScale, finalScale * stretchY);

                  Main.EntitySpriteDraw(texture, element - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

                if (Vector2.Distance(element, Main.player[Projectile.owner].Center) > 40f && Main.rand.NextBool(15)) 
                {
                    int dust = Dust.NewDust(element - new Vector2(frameWidth / 2f, frameHeight / 2f), frameWidth, frameHeight, 18, 0f, 0f, 50, default, 0.7f);
                    // Salen en la direcciÃ³n del swing del lÃ¡tigo (diff apunta hacia donde ataca)
                    Vector2 dustDir = diff.SafeNormalize(Vector2.Zero);
                    Main.dust[dust].velocity = dustDir * Main.rand.NextFloat(1.5f, 3f)
                        + new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-0.2f, 0.2f));
                }
            }

            return false;
        }
    }
}







