using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Whips145.Buffs;

namespace Whips145.Projectiles
{
    // El Starcrash marca al enemigo con energía "cargada".
    // Cuando un minion golpea al enemigo marcado, se invoca un meteorito
    // que hace 1.33x el daño del minion en área pequeña.
    public class StarcrashProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();
            Projectile.WhipSettings.Segments = 10;
            Projectile.WhipSettings.RangeMultiplier = 0.61f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Evitar efectos en muñecos de pruebas o animales
            if (target.friendly || target.type == NPCID.TargetDummy || target.lifeMax <= 5 || NPCID.Sets.CountsAsCritter[target.type])
                return;

            Projectile.damage = (int)(Projectile.damage * (1f - 0.3f));
            
            // La marca de meteorito solo se aplica al primer enemigo golpeado
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                // Aplicar tag
                target.AddBuff(ModContent.BuffType<StarcrashTag>(), 300);

                // Resetear el flag para que pueda caer un nuevo meteorito con esta marca
                target.GetGlobalNPC<Common.WhipTagGlobalNPC>().starcrashMeteorDropped = false;
            }

            // Partículas de fuego
            for (int i = 0; i < 7; i++)
            {
                int dust = Dust.NewDust(target.position, target.width, target.height, DustID.Torch, 0f, 0f, 100, default, 1.4f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 1.2f; // Estas sí salen un poco más disparadas como chispas
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
                float scale = 1f;

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

                // Draw the connecting line (solo si no es el último tramo después de la punta)
                if (i < list.Count - 2)
                {
                    Texture2D magicPixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
                    Vector2 lineScale = new Vector2(diff.Length(), 2f);
                    Color lineColor = new Color(95, 78, 171);
                    Color litLineColor = Lighting.GetColor(element.ToTileCoordinates(), lineColor);
                    Main.EntitySpriteDraw(magicPixel, element - Main.screenPosition, new Rectangle(0, 0, 1, 1), litLineColor, diff.ToRotation(), new Vector2(0, 0.5f), lineScale, SpriteEffects.None, 0);
                }

                // Draw the segment
                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

                // Partículas al usar el látigo
                if (Main.rand.NextBool(7)) 
                {
                    int dust = Dust.NewDust(pos - new Vector2(frameWidth / 2f, frameHeight / 2f), frameWidth, frameHeight, DustID.Torch, 0f, 0f, 100, default, 1.4f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.2f;
                }

                pos += diff;
            }

            return false;
        }
    }
}
