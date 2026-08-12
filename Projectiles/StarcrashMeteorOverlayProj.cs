using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaWhipsBackport.Projectiles
{
    public class StarcrashMeteorOverlayProj : ModProjectile
    {
        public override string Texture => "TerrariaWhipsBackport/Projectiles/StarcrashProj1";

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Meteor1);
            AIType = ProjectileID.Meteor1; 
            
            // Este es solo visual y hace daÃ±o extra si queremos, pero lo ideal 
            // es que solo acompaÃ±e al meteorito principal, asÃ­ que le quitaremos
            // el daÃ±o para que no pegue doble, o lo podemos dejar igual.
            // Para que no interfieran los IFrames, usamos inmunidad local.
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; 
            Projectile.tileCollide = false; // Ignorar techo
        }

        public override void AI()
        {
            // Solo empieza a chocar contra bloques cuando alcanza la altura esperada
            if (Projectile.ai[0] > 0f)
            {
                if (Projectile.position.Y >= Projectile.ai[0])
                {
                    Projectile.tileCollide = true;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Opcional: hacer que brille siempre ignorando la oscuridad de la noche
            lightColor = Color.White;
            return true;
        }
    }
}
