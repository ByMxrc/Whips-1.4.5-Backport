using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Whips145.Buffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Whips145.Projectiles
{
    public class VasculashProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();
            Projectile.WhipSettings.Segments = 15;
            Projectile.WhipSettings.RangeMultiplier = 0.46f;
        }

        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];
            
            // Actualizamos los segmentos según el multiplicador (lo que antes hacía AI())
            Projectile.WhipSettings.Segments = (int)(15 * player.whipRangeMultiplier);

            // Recreamos la IA nativa del látigo para evitar que corra la original y ponga el sonido
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

            // Reproducir el sonido nativo justo a la mitad de la extensión
            if (Projectile.ai[0] == (int)(timeToFlyOut / 2f))
            {
                SoundEngine.PlaySound(SoundID.NPCHit9, Projectile.position);
            }

            // Generar partículas a lo largo del látigo (restaurando lo que hacía la IA nativa)
            List<Vector2> list = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, list);
            for (int i = 0; i < list.Count; i++)
            {
                if (Vector2.Distance(list[i], player.Center) > 40f && Main.rand.NextBool(10)) 
                {
                    int dust = Dust.NewDust(list[i] - new Vector2(10, 10), 20, 20, DustID.Blood, 0f, 0f, 0, default, 1.0f);
                    // Dejamos que caigan con la gravedad (sin noGravity)
                    Main.dust[dust].velocity *= 0.3f;
                }
            }

            return false; // Retornamos false para evitar que corra la IA nativa del látigo
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Evitar efectos en muñecos de pruebas o animales
            if (target.friendly || target.type == NPCID.TargetDummy || target.lifeMax <= 5 || NPCID.Sets.CountsAsCritter[target.type])
                return;

            Projectile.damage = (int)(Projectile.damage * (1f - 0.4f));
            // Tag de Vasculash (5 segundos = 300 ticks)
            target.AddBuff(ModContent.BuffType<VasculashTag>(), 300);

            // Partículas de sangre carmesí
            for (int i = 0; i < 2; i++)
            {
                int dust = Dust.NewDust(target.position, target.width, target.height, DustID.Blood, 0f, 0f, 0, default, 1.0f);
                Main.dust[dust].velocity *= 0.3f;
            }
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

            Vector2 pos = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                Rectangle frame = new Rectangle(0, 0, frameWidth, frameHeight);
                Vector2 origin = new Vector2(frameWidth / 2f, 4f); 

                if (i == 0)
                {
                    frame.Y = 0;
                }
                else if (i == list.Count - 2)
                {
                    frame.Y = frameHeight * (totalFrames - 1);
                }
                else
                {
                    int midFrame = 1 + (i % (totalFrames - 2));
                    frame.Y = frameHeight * midFrame;
                }

                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;
                float rotation = diff.ToRotation() - MathHelper.PiOver2;
                Color color = Lighting.GetColor(element.ToTileCoordinates());

                // Escala Y dinámica: solo estira si la distancia es mayor al tamaño del segmento
                // para evitar que se separen al aumentar el rango, pero sin aplastarlos.
                float scaleY = 1f;
                if (diff.Length() > (frameHeight - 4f))
                {
                    scaleY = diff.Length() / (frameHeight - 4f);
                }
                Vector2 scale = new Vector2(1f, scaleY);

                // Draw the segment
                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

                // Partículas al usar el látigo
                if (Vector2.Distance(pos, Main.player[Projectile.owner].Center) > 40f && Main.rand.NextBool(20)) 
                {
                    int dust = Dust.NewDust(pos - new Vector2(frameWidth / 2f, frameHeight / 2f), frameWidth, frameHeight, DustID.Blood, 0f, 0f, 0, default, 1.0f);
                    Main.dust[dust].velocity *= 0.3f;
                }

                pos += diff;
            }

            return false;
        }
    }
}






