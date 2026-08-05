using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Whips145.Buffs;

namespace Whips145.Projectiles
{
    // El Electric Eel genera un aura eléctrica que daña enemigos cercanos.
    public class ElectricEelProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();
            Projectile.WhipSettings.Segments = 40;
            Projectile.WhipSettings.RangeMultiplier = 1.07f;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.WhipSettings.Segments = (int)(40 * player.whipRangeMultiplier);
        }

        public override void PostAI()
        {
            List<Vector2> list = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, list);
            Player player = Main.player[Projectile.owner];

            // Partículas durante la animación de ataque (Densidad reducida)
            for (int i = 0; i < list.Count; i++)
            {
                // Evitamos generar partículas si el látigo está encogido cerca del jugador
                if (Vector2.Distance(list[i], player.MountedCenter) < 50f)
                    continue;

                if (Main.rand.NextBool(18)) // 1/18 de probabilidad (10% menos que el anterior 16)
                {
                    int dust = Dust.NewDust(
                        list[i],
                        0,
                        0,
                        DustID.Electric,
                        0f, 0f, 0, default, 1.5f
                    );
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 2f;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Evitar efectos en NPCs amigables
            if (target.friendly)
                return;

            Projectile.damage = (int)(Projectile.damage * (1f - 0.05f));
            // Tag del Electric Eel (4 segundos)
            target.AddBuff(ModContent.BuffType<ElectricEelTag>(), 240);

            // Nota: En 1.4.5, el Electric Eel NO aplica el tag en área automáticamente con el látigo.
            // Para marcar a múltiples enemigos, el jugador debe golpearlos individualmente o
            // usar summons que extiendan el rango de daño. La mecánica de rayo en cadena ocurre
            // pasivamente entre los enemigos ya marcados (gestionado en WhipTagGlobalNPC).
        }
    
        public override bool PreDraw(ref Color lightColor)
        {
            List<Vector2> list = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, list);

            Terraria.Main.instance.LoadProjectile(Type);
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            int totalFrames = 5;
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

                // Escala Y dinámica: solo estira si la distancia es mayor al tamaño efectivo del segmento
                // Usamos divisor menor (frameHeight - 6.5f) para cerrar la brecha de 1-2px al extenderse
                float scaleY = 1f;
                float divisor = frameHeight - 6.5f;
                if (diff.Length() > divisor)
                {
                    scaleY = diff.Length() / divisor;
                }
                Vector2 scale = new Vector2(1f, scaleY);

                // Draw the segment
                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);
                pos += diff;
            }

            return false;
        }
    }
}
