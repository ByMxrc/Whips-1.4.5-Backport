using Terraria;
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

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.WhipSettings.Segments = (int)(15 * player.whipRangeMultiplier);
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
                int dust = Dust.NewDust(target.position, target.width, target.height, DustID.Blood, 0f, 0f, 150, default, 1.1f);
                Main.dust[dust].noGravity = true;
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
                if (Main.rand.NextBool(20)) 
                {
                    int dust = Dust.NewDust(pos - new Vector2(frameWidth / 2f, frameHeight / 2f), frameWidth, frameHeight, DustID.Blood, 0f, 0f, 150, default, 1.1f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 0.3f;
                }

                pos += diff;
            }

            return false;
        }
    }
}






