using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Whips145.Projectiles
{
    public class StarcrashMeteorProj : ModProjectile
    {
        public override string Texture => "Whips145/Projectiles/StarcrashProj1";
        
        // Eliminadas variables de animación vanilla que causaban problemas

        public override void SetStaticDefaults()
        {
            // No necesitamos TrailCache, el Meteor Staff real usa Dust para la estela.
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.penetrate = 1; // ¡IMPORTANTE! 1 para que explote al chocar con un enemigo
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

            // ── ESTELA DE FUEGO (Idéntica al Meteor Staff real) ──
            // El Meteor Staff original NO usa oldPos ni texturas, usa MUCHAS partículas de polvo (Dust).
            // Para que no queden huecos por la velocidad, generamos polvo interpolando la trayectoria.
            int dustSteps = 4; // Cantidad de puntos entre el frame anterior y el actual
            for (int j = 0; j < dustSteps; j++)
            {
                // Interpolar posición entre el frame anterior y el actual
                Vector2 spawnPos = Vector2.Lerp(Projectile.oldPosition, Projectile.position, j / (float)dustSteps);

                // Polvo de fuego intenso (naranja/rojo) como el meteorito vanilla
                for (int i = 0; i < 2; i++)
                {
                    int d = Dust.NewDust(spawnPos, Projectile.width, Projectile.height, 270, 
                                         Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100, default, 1.25f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity *= 0.3f; // El polvo se queda atrás formando la cola
                }
            }

            // Solo empieza a chocar contra bloques cuando está a punto de llegar a la altura del enemigo
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
            // Limpiamos el PreDraw. 
            // Ya no usamos texturas deformadas para el rastro.
            // El juego dibujará tu textura (StarcrashProj1) automáticamente y el humo/fuego cubrirá el resto.
            
            // Hacemos que el meteorito se vea brillante en la oscuridad
            lightColor = Color.White; 
            return true;
        }

        public override bool PreKill(int timeLeft)
        {
            // Al morir, generamos la explosión, polvo y sonido del meteorito vanilla
            Projectile.type = ProjectileID.Meteor1; 
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            // Daño en área (4x4 bloques = 64x64 pixeles)
            Projectile.position = Projectile.Center;
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.Center = Projectile.position;

            Projectile.Damage(); 
        }
    }
}
