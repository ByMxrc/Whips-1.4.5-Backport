using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Whips145.Buffs;
using Whips145.Items;
using Whips145.Projectiles;

namespace Whips145.Common
{
    /// <summary>
    /// GlobalNPC que maneja:
    /// 1. Drops de látigos en los jefes correspondientes (Plantera, Duke Fishron, Moon Lord)
    /// 2. Efectos especiales cuando los minions golpean a enemigos marcados con el tag
    ///    de cada látigo (Starcrash, VulgarDisplay, ElectricEel, Constellation, Possession)
    /// </summary>
    public class WhipTagGlobalNPC : GlobalNPC
    {
        // Variables de instancia para rastrear el estado del tag en cada NPC
        public bool hasStarcrashTag = false;
        public bool hasVulgarTag = false;
        public bool hasVulgarEnergy = false;
        public bool hasElectricEelTag = false;
        public bool hasConstellationTag = false;

        public bool starcrashMeteorDropped = false; // Garantiza solo 1 meteorito por marca
        public int electricEelTimer = 0; // Temporizador para el efecto del rayo (Electric Eel)
        public override bool InstancePerEntity => true; // Instancia por NPC

        public override void SetDefaults(NPC npc)
        {
            // Permitir que los Dummies reciban los tags de los látigos para poder probarlos
            if (npc.type == NPCID.TargetDummy)
            {
                npc.buffImmune[ModContent.BuffType<StarcrashTag>()] = false;
                npc.buffImmune[ModContent.BuffType<VulgarDisplayTag>()] = false;
                npc.buffImmune[ModContent.BuffType<VulgarDisplayEnergy>()] = false;
                npc.buffImmune[ModContent.BuffType<ElectricEelTag>()] = false;
                npc.buffImmune[ModContent.BuffType<ConstellationTag>()] = false;
                npc.buffImmune[ModContent.BuffType<CobwhipTag>()] = false;
                npc.buffImmune[ModContent.BuffType<SlimeWhipTag>()] = false;
                npc.buffImmune[ModContent.BuffType<SoulscourgeTag>()] = false;
                npc.buffImmune[ModContent.BuffType<VasculashTag>()] = false;
            }
        }

        // ─── CONFIGURAR DROPS EN LOS JEFES ───────────────────────────────────────

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            // Vulgar Display of Flower → Plantera (25% de probabilidad)
            if (npc.type == NPCID.Plantera)
            {
                npcLoot.Add(ItemDropRule.Common(
                    ModContent.ItemType<VulgarDisplayOfFlower>(), 4)); // 1 en 4 = 25%
            }

            // Electric Eel → Duke Fishron (25%)
            if (npc.type == NPCID.DukeFishron)
            {
                npcLoot.Add(ItemDropRule.Common(
                    ModContent.ItemType<ElectricEel>(), 4));
            }

            // Possession → Moon Lord (25%)
            if (npc.type == NPCID.MoonLordCore)
            {
                npcLoot.Add(ItemDropRule.Common(
                    ModContent.ItemType<Possession>(), 4));
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (projectile.minion || ProjectileID.Sets.MinionShot[projectile.type] || projectile.sentry || ProjectileID.Sets.SentryShot[projectile.type])
            {
                if (npc.HasBuff(ModContent.BuffType<CobwhipTag>())) modifiers.FlatBonusDamage += 3;
                if (npc.HasBuff(ModContent.BuffType<ConstellationTag>())) modifiers.FlatBonusDamage += 15;
                if (npc.HasBuff(ModContent.BuffType<ElectricEelTag>())) modifiers.FlatBonusDamage += 12;
                if (npc.HasBuff(ModContent.BuffType<SlimeWhipTag>())) modifiers.FlatBonusDamage += 3;
                if (npc.HasBuff(ModContent.BuffType<SoulscourgeTag>())) modifiers.FlatBonusDamage += 5;
                if (npc.HasBuff(ModContent.BuffType<StarcrashTag>())) modifiers.FlatBonusDamage += 2;
                if (npc.HasBuff(ModContent.BuffType<VasculashTag>())) modifiers.FlatBonusDamage += 5;
                if (npc.HasBuff(ModContent.BuffType<VulgarDisplayTag>())) modifiers.FlatBonusDamage += 9;
            }
        }

        // ─── ACTUALIZAR ESTADO DEL TAG SEGÚN BUFFS ACTIVOS ───────────────────────

        public override void PostAI(NPC npc)
        {
            if (npc.active)
            {
                hasStarcrashTag = npc.HasBuff(ModContent.BuffType<StarcrashTag>());
                hasVulgarTag = npc.HasBuff(ModContent.BuffType<VulgarDisplayTag>());
                hasVulgarEnergy = npc.HasBuff(ModContent.BuffType<VulgarDisplayEnergy>());
                hasElectricEelTag = npc.HasBuff(ModContent.BuffType<ElectricEelTag>());
                hasConstellationTag = npc.HasBuff(ModContent.BuffType<ConstellationTag>());

                // ── ELECTRIC EEL: Efectos visuales de rayo cada segundo ──
                if (hasElectricEelTag)
                {
                    // Efecto visual persistente: mantener partículas eléctricas envolviendo al enemigo
                    if (Main.rand.NextBool(3))
                    {
                        int auraDust = Dust.NewDust(npc.position, npc.width, npc.height, DustID.Electric, 0f, 0f, 0, default, 1.2f);
                        Main.dust[auraDust].noGravity = true;
                        Main.dust[auraDust].velocity *= 0.5f;
                    }

                    electricEelTimer++;
                    // Cada 60 frames (1 segundo), dispara el rayo visual
                    if (electricEelTimer >= 60)
                    {
                        electricEelTimer = 0;

                        // Buscar otros enemigos con la marca a menos de 100 bloques (1600 pixeles)
                        int maxTargets = 10;
                        int targetsFound = 0;

                        // Los Target Dummies no emiten ni reciben rayos visuales (evita el ruido visual extremo), 
                        // pero sí reciben el daño base.
                        if (npc.type != NPCID.TargetDummy)
                        {
                            for (int i = 0; i < Main.maxNPCs; i++)
                            {
                                NPC other = Main.npc[i];
                                if (other.active && !other.friendly && other.type != NPCID.TargetDummy && other.whoAmI != npc.whoAmI && other.HasBuff(ModContent.BuffType<ElectricEelTag>()))
                                {
                                    if (Vector2.Distance(npc.Center, other.Center) <= 1600f)
                                    {
                                        // Dibujar línea de polvo eléctrico (rayo visual)
                                        Vector2 start = npc.Center;
                                        Vector2 end = other.Center;
                                        int numDusts = (int)(Vector2.Distance(start, end) / 8f);
                                        for (int d = 0; d < numDusts; d++)
                                        {
                                            Vector2 position = Vector2.Lerp(start, end, d / (float)numDusts);
                                            // Efecto zig-zag aleatorio
                                            position += Main.rand.NextVector2Circular(8f, 8f);
                                            
                                            Dust dust = Dust.NewDustPerfect(position, DustID.Electric, Vector2.Zero, 0, default, 1.2f);
                                        dust.noGravity = true;
                                        dust.velocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
                                    }

                                    targetsFound++;
                                    if (targetsFound >= maxTargets) break;
                                }
                                }
                            }
                        }

                        // Realizar el daño directamente (así afecta a los Dummies y sale el número flotante sincronizado)
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int damage = 50 + (targetsFound * 50);
                            npc.StrikeNPC(new NPC.HitInfo
                            {
                                Damage = damage,
                                Knockback = 0f,
                                HitDirection = 0,
                                Crit = false,
                                DamageType = DamageClass.Summon
                            });
                        }
                    }
                }
                else
                {
                    electricEelTimer = 0;
                }
            }
        }

        // ─── MODIFICACIÓN DE REGENERACIÓN DE VIDA (DAÑO CON EL TIEMPO) ───────────────

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            // El debuff de Slime (Slimed) hace que los debuffs de fuego hagan el doble de daño
            if (npc.HasBuff(BuffID.Slimed))
            {
                int extraLifeRegenLoss = 0;
                
                if (npc.HasBuff(BuffID.OnFire)) extraLifeRegenLoss += 8; // 4 dps (base es 8)
                if (npc.HasBuff(BuffID.OnFire3)) extraLifeRegenLoss += 30; // Hellfire, 15 dps (base es 30)
                if (npc.HasBuff(BuffID.CursedInferno)) extraLifeRegenLoss += 48; // 24 dps (base es 48)
                if (npc.HasBuff(BuffID.Frostburn)) extraLifeRegenLoss += 16; // 8 dps (base es 16)
                if (npc.HasBuff(BuffID.Frostburn2)) extraLifeRegenLoss += 50; // Frostbite, 25 dps (base es 50)
                if (npc.HasBuff(BuffID.ShadowFlame)) extraLifeRegenLoss += 30; // 15 dps (base es 30)
                
                if (extraLifeRegenLoss > 0)
                {
                    // Duplicamos el lifeRegen restando la misma cantidad que quitan los debuffs originales
                    npc.lifeRegen -= extraLifeRegenLoss;
                    
                    // El parámetro 'damage' controla el número de daño que sale flotando,
                    // así que lo incrementamos para que visualmente cuadre con el daño doble
                    int extraDamage = extraLifeRegenLoss / 4;
                    if (damage < extraDamage)
                    {
                        damage = extraDamage;
                    }
                }
            }

            // La lógica de daño del Electric Eel se movió a PostAI para que sea un golpe directo sincronizado con el rayo.
        }

        // ─── EFECTOS AL RECIBIR DAÑO DE PROYECTILES DE MINIONS ───────────────────

        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            // Solo aplicamos efectos si el proyectil pertenece a un minion o centinela
            if (!projectile.minion && !ProjectileID.Sets.MinionShot[projectile.type] && !projectile.sentry && !ProjectileID.Sets.SentryShot[projectile.type])
                return;

            // Evitar efectos en muñecos de pruebas o animales
            if (npc.friendly || npc.type == NPCID.TargetDummy || npc.lifeMax <= 5 || NPCID.Sets.CountsAsCritter[npc.type])
                return;

            Player owner = Main.player[projectile.owner];
            WhipPlayer whipPlayer = owner.GetModPlayer<WhipPlayer>();

            // ── STARCRASH: Invoca un meteorito (1.33x el daño del minion) ──────────
            if (hasStarcrashTag && whipPlayer.starcrashCooldown == 0 && !starcrashMeteorDropped)
            {
                whipPlayer.starcrashCooldown = 50; // Cooldown global en el jugador
                starcrashMeteorDropped = true; // Bloqueo inmediato: solo 1 meteorito por marca
                
                int meteorDamage = (int)(damageDone * 1.33f);

                if (Main.myPlayer == projectile.owner)
                {
                    float randomXOffset = Main.rand.NextFloat(-150f, 150f);
                    float randomYOffset = Main.rand.NextFloat(-100f, 100f);

                    Vector2 spawnPos = new Vector2(npc.Center.X + randomXOffset, npc.Center.Y - 300f + randomYOffset);

                    float duration = 15f; // ~0.2 segundos, más rápido y difícil de esquivar
                    Vector2 velocity = (npc.Center - spawnPos) / duration;

                    Projectile.NewProjectile(
                        projectile.GetSource_FromThis(),
                        spawnPos,
                        velocity,
                        ModContent.ProjectileType<StarcrashMeteorProj>(),
                        meteorDamage,
                        2f,
                        projectile.owner,
                        npc.Center.Y
                    );
                }

                // Partículas en el enemigo al activarse
                for (int i = 0; i < 4; i++)
                {
                    Dust.NewDust(npc.position, npc.width, npc.height,
                        DustID.Torch, 0f, -2f, 0, default, 1.5f);
                }

                // CONSUMIR LA MARCA: se quita el buff inmediatamente (marca de un solo uso)
                npc.DelBuff(npc.FindBuffIndex(ModContent.BuffType<StarcrashTag>()));
                hasStarcrashTag = false;
            }

            // ── VULGAR DISPLAY OF FLOWER: Brotan pétalos del enemigo (Volatile Energy) ──
            if (hasVulgarEnergy && whipPlayer.vulgarCooldown == 0)
            {
                whipPlayer.vulgarCooldown = 15;

                // El daño de los pétalos es de 40 (fijo, ignora el daño del minion)
                int petalDamage = 40;

                // Invocar 3 pétalos en forma de triángulo
                if (Main.myPlayer == projectile.owner)
                {
                    float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float speed = 6.0f; // Aumentado para separarse ~5.5-6 bloques (aprox 95 píxeles)
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = baseAngle + (i * MathHelper.TwoPi / 3f);
                        Vector2 velocity = angle.ToRotationVector2() * speed;
                        
                        Projectile.NewProjectile(
                            projectile.GetSource_FromThis(), 
                            npc.Center, 
                            velocity, 
                            ModContent.ProjectileType<VulgarDisplayOfFlowerProj1>(), 
                            petalDamage, 
                            0f, 
                            projectile.owner
                        );
                    }
                }

                // CONSUMIR LA ENERGÍA VOLÁTIL: se quita el buff de energía, pero el de daño tag persiste
                int buffIndex = npc.FindBuffIndex(ModContent.BuffType<VulgarDisplayEnergy>());
                if (buffIndex != -1)
                {
                    npc.DelBuff(buffIndex);
                }
                hasVulgarEnergy = false;
            }

            // ── CONSTELLATION: Estrellas fugaces del tag ─────────────────────────
            // (Las estrellas se lanzan al golpear con el látigo en OnHitNPC del proyectil)
            // Aquí podemos añadir un efecto extra cuando los minions golpean al enemigo marcado
            if (hasConstellationTag && whipPlayer.constellationCooldown == 0)
            {
                whipPlayer.constellationCooldown = 30; // Cada 0.5 segundos

                // Partículas de estrella alrededor del enemigo
                for (int i = 0; i < 5; i++)
                {
                    int dust = Dust.NewDust(
                        npc.position, npc.width, npc.height,
                        57,
                        Main.rand.NextFloat(-2f, 2f),
                        Main.rand.NextFloat(-3f, -1f),
                        0, default, 1.6f
                    );
                    Main.dust[dust].noGravity = true;
                }
            }

        }

        // ─── SERIALIZACIÓN (necesario para InstancePerEntity) ────────────────────

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            bitWriter.WriteBit(hasStarcrashTag);
            bitWriter.WriteBit(hasVulgarTag);
            bitWriter.WriteBit(hasVulgarEnergy);
            bitWriter.WriteBit(hasElectricEelTag);
            bitWriter.WriteBit(hasConstellationTag);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            hasStarcrashTag = bitReader.ReadBit();
            hasVulgarTag = bitReader.ReadBit();
            hasVulgarEnergy = bitReader.ReadBit();
            hasElectricEelTag = bitReader.ReadBit();
            hasConstellationTag = bitReader.ReadBit();
        }
    }
}
