using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaWhipsBackport.Projectiles
{
    public class StarcrashMeteorProj : ModProjectile
    {
        public override string Texture => "TerrariaWhipsBackport/Projectiles/StarcrashProj1";
        
        // Eliminadas variables de animaciÃ³n vanilla que causaban problemas

        public override void SetStaticDefaults()
        {
            // Activamos el rastro para poder dibujar la cola de cometa
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.penetrate = 1; // Â¡IMPORTANTE! 1 para que explote al chocar con un enemigo
            Projectile.timeLeft = 600; 
            Projectile.DamageType = DamageClass.SummonMeleeSpeed;
            
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; 
            Projectile.tileCollide = false; // Ignorar bloques hasta estar cerca
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            // Apuntar siempre hacia abajo
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // â”€â”€ ESTELA DE FUEGO (IdÃ©ntica al Meteor Staff real) â”€â”€
            // El Meteor Staff original NO usa oldPos ni texturas, usa MUCHAS partÃ­culas de polvo (Dust).
            // Para que no queden huecos por la velocidad, generamos polvo interpolando la trayectoria.
            int dustSteps = 4; // Cantidad de puntos entre el frame anterior y el actual
            for (int j = 0; j < dustSteps; j++)
            {
                // Interpolar posiciÃ³n entre el frame anterior y el actual
                Vector2 spawnPos = Vector2.Lerp(Projectile.oldPosition, Projectile.position, j / (float)dustSteps);

                // Polvo de fuego intenso (naranja/rojo) como el meteorito vanilla
                for (int i = 0; i < 2; i++)
                {
                    int d = Dust.NewDust(spawnPos, Projectile.width, Projectile.height, 270, 
                                         Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100, default, 1.25f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity *= 0.3f; // El polvo se queda atrÃ¡s formando la cola
                }
            }

            // Solo empieza a chocar contra bloques cuando estÃ¡ a punto de llegar a la altura del enemigo
            if (Projectile.ai[0] > 0f)
            {
                if (Projectile.position.Y >= Projectile.ai[0] - 30f)
                {
                    Projectile.tileCollide = true;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;

            // 1. Dibujar la estela (cola de cometa) con 50% de transparencia
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                // Transparencia base del 50% (* 0.5f) que se va desvaneciendo hacia la punta de la cola
                float progress = 1f - (i / (float)Projectile.oldPos.Length);
                Color trailColor = Color.White * 0.5f * progress;
                
                // Calculamos la posiciÃ³n de dibujado desde el oldPos
                Vector2 drawPos = Projectile.oldPos[i] + new Vector2(Projectile.width / 2f, Projectile.height / 2f) - Main.screenPosition;
                
                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.rotation, origin, Projectile.scale * progress, SpriteEffects.None, 0);
            }

            // 2. Dibujar el meteorito principal (brillante)
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            return false; // Retornamos false para que el juego no lo dibuje de nuevo por defecto
        }

        public override bool PreKill(int timeLeft)
        {
            // Al morir, generamos la explosiÃ³n, polvo y sonido del meteorito vanilla
            Projectile.type = ProjectileID.Meteor1; 
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            // DaÃ±o en Ã¡rea (4x4 bloques = 64x64 pixeles)
            Projectile.position = Projectile.Center;
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.Center = Projectile.position;

            Projectile.Damage(); 
        }
    }
}
