﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using tsorcRevamp.Items.Potions;

namespace tsorcRevamp.NPCs.Enemies
{
    public class HollowWarrior : ModNPC //Don't look at the code, it's muy malo. Look at Lothric Spear Knight for a better example code management-wise
    {
        bool slashing = false;
        bool jumpSlashing = false;
        bool shielding = false;

        int shieldFrame;
        int shieldAnimTimer;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[npc.type] = 17;
        }
        public override void SetDefaults()
        {
            npc.knockBackResist = 0.45f;
            npc.aiStyle = -1;
            npc.damage = 18;
            npc.defense = 10;
            npc.height = 40;
            npc.width = 20;
            npc.lifeMax = 75;
            if (Main.hardMode) { npc.lifeMax = 200; npc.defense = 25; npc.damage = 28; npc.value = 450; }
            if (tsorcRevampWorld.SuperHardMode) { npc.lifeMax = 1050; npc.defense = 45; npc.damage = 38; npc.value = 550; }
            npc.value = 250;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath2;
            /*banner = npc.type;
            bannerItem = ModContent.ItemType<Banners.DunlendingBanner>();*/
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            for (int i = 0; i < 10; i++)
            {
                int dustType = 5;
                int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                Dust dust = Main.dust[dustIndex];
                dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.04f;
                dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.04f;
                dust.scale *= .8f + Main.rand.Next(-30, 31) * 0.01f;
                dust.noGravity = true;
            }
            if (npc.life <= 0)
            {
                for (int i = 0; i < 80; i++)
                {
                    Dust.NewDust(npc.position, npc.width, npc.height, 54, 2.5f * (float)hitDirection, -1.5f, 70, default(Color), 1f);
                    Dust.NewDust(npc.position, npc.width, npc.height, 5, 1.5f * (float)hitDirection, -2.5f, 50, default(Color), 1f);
                }
            }
        }

        public override void AI()
        {
            Player player = Main.player[npc.target];

            int lifePercentage = (npc.life * 100) / npc.lifeMax;
            float acceleration = 0.02f;
            if (shielding) { acceleration = 0.01f; }
            //float top_speed = (lifePercentage * 0.02f) + .2f; //good calculation to remember for decreasing speed the lower the enemy HP%
            float top_speed = (lifePercentage * -0.015f) + 2f; //good calculation to remember for increasing speed the lower the enemy HP%
            float braking_power = 0.1f; //Breaking power to slow down after moving above top_speed
            //Main.NewText(Math.Abs(npc.velocity.X));

            #region target/face player, respond to boredom

            /*if (!jumpSlashing && !slashing)
            {
                npc.TargetClosest(true); //  Target the closest player & face him (If passed as a parameter, a bool will determine whether it should face the target or not)
            }

            if (npc.velocity.X == 0f && !jumpSlashing && !shielding && !slashing)
            {
                if (npc.velocity.Y == 0f)
                { // not moving
                    if (npc.ai[0] == 0f)
                        npc.ai[0] = 1f; // facing change delay
                    else
                    { // change movement and facing direction, reset delay
                        npc.direction *= -1;
                        npc.spriteDirection = npc.direction;
                        npc.ai[0] = 0f;
                    }
                }
            }
            else // moving in x direction,
                npc.ai[0] = 0f; // reset facing change delay

            if (npc.direction == 0) // what does it mean if direction is 0?
                npc.direction = 1; // flee right if direction not set? or is initial direction?*/
            if (npc.ai[0] == 0 && !jumpSlashing && !slashing)
            {
                npc.TargetClosest(true); //  Target the closest player & face him (If passed as a parameter, a bool will determine whether it should face the target or not)
            }

            if (npc.velocity.X == 0 && !jumpSlashing && !shielding && !slashing)
            {
                npc.ai[0]++;
                if (npc.ai[0] > 120 && npc.velocity.Y == 0)
                {
                    npc.direction *= -1;
                    npc.spriteDirection = npc.direction;
                    npc.ai[0] = 50;
                }
            }

            if (Collision.CanHitLine(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0))
            {
                npc.ai[0] = 0;
            }


            #endregion

            #region melee movement

            if (Math.Abs(npc.velocity.X) > top_speed && npc.velocity.Y == 0)
            {
                npc.velocity *= (1f - braking_power); //breaking
            }
            if (npc.velocity.X > 5f) //hard limit of 8f
            {
                npc.velocity.X = 5f;
            }
            if (npc.velocity.X < -5f) //both directions
            {
                npc.velocity.X = -5f;
            }
            else
            {
                npc.velocity.X += npc.direction * acceleration; //accelerating
            }

            //breaking power after turning, to turn fast or to "slip"
            if (npc.direction == 1)
            {
                if (npc.velocity.X > -top_speed)
                {
                    npc.velocity.X += 0.085f;
                }
                npc.netUpdate = true;
            }
            if (npc.direction == -1)
            {
                if (npc.velocity.X < top_speed)
                {
                    npc.velocity.X += -0.085f;
                }
                npc.netUpdate = true;
            }
            

            if (Math.Abs(npc.velocity.X) > 4f) //If moving at high speed, become knockback immune
            {
                npc.knockBackResist = 0;
            }
            if (Math.Abs(npc.velocity.Y) > 0.1f) //If moving vertically, become knockback immune
            {
                npc.knockBackResist = 0;
            }

            else
            {
                npc.knockBackResist = 0.45f; //If not moving at high speed, default back to taking some knockback
            }

            npc.noTileCollide = false;

            int y_below_feet = (int)(npc.position.Y + (float)npc.height + 8f) / 16;
            if (Main.tile[(int)npc.position.X / 16, y_below_feet].type == TileID.Platforms && Main.tile[(int)(npc.position.X + (float)npc.width) / 16, y_below_feet].type == TileID.Platforms && npc.position.Y < (player.position.Y - 4 * 16))
            {
                npc.noTileCollide = true;
            }

            #endregion

            #region check if standing on a solid tile
            bool standing_on_solid_tile = false;
            if (npc.velocity.Y == 0f) // no jump/fall
            {
                int x_left_edge = (int)npc.position.X / 16;
                int x_right_edge = (int)(npc.position.X + (float)npc.width) / 16;
                for (int l = x_left_edge; l <= x_right_edge; l++) // check every block under feet
                {
                    if (Main.tile[l, y_below_feet] == null) // null tile means ??
                        return;

                    if (Main.tile[l, y_below_feet].active() && Main.tileSolid[(int)Main.tile[l, y_below_feet].type]) // tile exists and is solid
                    {
                        standing_on_solid_tile = true;
                        break; // one is enough so stop checking
                    }
                } // END traverse blocks under feet
            } // END no jump/fall
            #endregion

            #region new Tile()s, jumping
            if (standing_on_solid_tile && !slashing && !shielding && !jumpSlashing)  //  if standing on solid tile
            {
                int x_in_front = (int)((npc.position.X + (float)(npc.width / 2) + (float)(15 * npc.direction)) / 16f); // 15 pix in front of center of mass
                int y_above_feet = (int)((npc.position.Y + (float)npc.height - 15f) / 16f); // 15 pix above feet

                if (npc.position.Y > player.position.Y + 3 * 16 && Math.Abs(npc.Center.X - player.Center.X) < 4f * 16 && Collision.CanHitLine(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0))
                {
                    slashing = true;
                    npc.ai[3] = 16;
                    npc.velocity.Y = -8f; // jump with power 8 if directly under player
                    npc.netUpdate = true;
                }

                if (Main.tile[x_in_front, y_above_feet] == null)
                {
                    Main.tile[x_in_front, y_above_feet] = new Tile();
                }

                if (Main.tile[x_in_front, y_above_feet - 1] == null)
                {
                    Main.tile[x_in_front, y_above_feet - 1] = new Tile();
                }

                if (Main.tile[x_in_front, y_above_feet - 2] == null)
                {
                    Main.tile[x_in_front, y_above_feet - 2] = new Tile();
                }

                if (Main.tile[x_in_front, y_above_feet - 3] == null)
                {
                    Main.tile[x_in_front, y_above_feet - 3] = new Tile();
                }

                if (Main.tile[x_in_front, y_above_feet + 1] == null)
                {
                    Main.tile[x_in_front, y_above_feet + 1] = new Tile();
                }
                //  create? 2 other tiles farther in front
                if (Main.tile[x_in_front + npc.direction, y_above_feet - 1] == null)
                {
                    Main.tile[x_in_front + npc.direction, y_above_feet - 1] = new Tile();
                }

                if (Main.tile[x_in_front + npc.direction, y_above_feet + 1] == null)
                {
                    Main.tile[x_in_front + npc.direction, y_above_feet + 1] = new Tile();
                }

                else // standing on solid tile but not in front of a passable door
                {
                    if ((npc.velocity.X < 0f && npc.spriteDirection == -1) || (npc.velocity.X > 0f && npc.spriteDirection == 1))
                    {  //  moving forward
                        if (Main.tile[x_in_front, y_above_feet - 2].active() && Main.tileSolid[(int)Main.tile[x_in_front, y_above_feet - 2].type])
                        { // 3 blocks above ground level(head height) blocked
                            if (Main.tile[x_in_front, y_above_feet - 3].active() && Main.tileSolid[(int)Main.tile[x_in_front, y_above_feet - 3].type])
                            { // 4 blocks above ground level(over head) blocked
                                npc.velocity.Y = -8f; // jump with power 8 (for 4 block steps)
                                npc.netUpdate = true;
                            }
                            else
                            {
                                npc.velocity.Y = -7f; // jump with power 7 (for 3 block steps)
                                npc.netUpdate = true;
                            }
                        } // for everything else, head height clear:
                        else if (Main.tile[x_in_front, y_above_feet - 1].active() && Main.tileSolid[(int)Main.tile[x_in_front, y_above_feet - 1].type])
                        { // 2 blocks above ground level(mid body height) blocked
                            npc.velocity.Y = -6f; // jump with power 6 (for 2 block steps)
                            npc.netUpdate = true;
                        }
                        else if (Main.tile[x_in_front, y_above_feet].active() && Main.tileSolid[(int)Main.tile[x_in_front, y_above_feet].type])
                        { // 1 block above ground level(foot height) blocked
                            npc.velocity.Y = -5f; // jump with power 5 (for 1 block steps)
                            npc.netUpdate = true;
                        }
                        else if (npc.directionY < 0 && (!Main.tile[x_in_front, y_above_feet + 1].active() || !Main.tileSolid[(int)Main.tile[x_in_front, y_above_feet + 1].type]) && (!Main.tile[x_in_front + npc.direction, y_above_feet + 1].active() || !Main.tileSolid[(int)Main.tile[x_in_front + npc.direction, y_above_feet + 1].type]))
                        { // rising? & jumps gaps & no solid tile ahead to step on for 2 spaces in front
                            npc.velocity.Y = -8f; // jump with power 8
                            npc.velocity.X = npc.velocity.X * 1.5f; // jump forward hard as well; we're trying to jump a gap
                            npc.netUpdate = true;
                        }
                    } // END moving forward, still: standing on solid tile but not in front of a passable door
                }
            }

            #endregion

            #region attacks


            //Basic Slash Attack

            if (npc.ai[3] < 10)
            {
                ++npc.ai[3]; //Used for Basic Slash
            }

            if (/*!shielding && */!jumpSlashing)
            {
                if (npc.ai[3] == 10 && npc.Distance(player.Center) < 45 && Collision.CanHitLine(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0))
                {
                    slashing = true;
                    shielding = false;
                }

                if (slashing)
                {
                    ++npc.ai[3];

                    if (npc.ai[3] < 26)
                    {
                        if (npc.direction == 1)
                        {
                            npc.velocity.X -= 0.25f;
                            if (npc.velocity.X < 0)
                            {
                                npc.velocity.X = 0;
                            }
                        }

                        else
                        {
                            npc.velocity.X += 0.25f;
                            if (npc.velocity.X > 0)
                            {
                                npc.velocity.X = 0;
                            }
                        }
                    }

                    if (npc.ai[3] == 26) //If timer is 46
                    {
                        Main.PlaySound(SoundID.Item1.WithVolume(1f).WithPitchVariance(.3f), npc.position); //Play slash/swing sound

                        if (npc.direction == 1)
                        {
                            if (!standing_on_solid_tile)
                            {
                                Projectile.NewProjectile(npc.Center + new Vector2(20, -56), new Vector2(0, 4f), ModContent.ProjectileType<Projectiles.Enemy.SmallWeaponSlash>(), 10, 5, Main.myPlayer, npc.whoAmI, 0);

                            }
                            else
                            {
                                Projectile.NewProjectile(npc.Center + new Vector2(20, -20), new Vector2(0, 4f), ModContent.ProjectileType<Projectiles.Enemy.SmallWeaponSlash>(), 10, 5, Main.myPlayer, npc.whoAmI, 0);
                            }
                        }

                        else
                        {
                            if (!standing_on_solid_tile)
                            {
                                Projectile.NewProjectile(npc.Center + new Vector2(-8, -56), new Vector2(0, 4f), ModContent.ProjectileType<Projectiles.Enemy.SmallWeaponSlash>(), 10, 5, Main.myPlayer, npc.whoAmI, 0);

                            }
                            else
                            {
                                Projectile.NewProjectile(npc.Center + new Vector2(-8, -20), new Vector2(0, 4f), ModContent.ProjectileType<Projectiles.Enemy.SmallWeaponSlash>(), 10, 5, Main.myPlayer, npc.whoAmI, 0);
                            }
                        }
                    }

                    if (npc.ai[3] >= 49) //If timer is 69
                    {
                        slashing = false;
                        npc.ai[3] = 0; //Reset timer
                    }
                }
            }




            //Telegraphed Jump-slash

            if (npc.ai[1] < 420)
            {
                ++npc.ai[1]; //Used for Jump-slash
            }
            if (npc.ai[1] >= 390 && npc.ai[1] <= 400)
            {
                if (npc.direction == 1) //Large eye dust to warn player that a jump-slash is ready...
                {
                    Dust dust2 = Main.dust[Dust.NewDust(new Vector2(npc.position.X + 9, npc.position.Y + 1), 4, 4, 183, npc.velocity.X, npc.velocity.Y, 180, default(Color), 1.5f)];
                    //dust2.velocity *= 0f;
                    dust2.noGravity = true;
                    dust2.fadeIn = .3f;
                    dust2.velocity += npc.velocity;
                }

                else
                {
                    Dust dust2 = Main.dust[Dust.NewDust(new Vector2(npc.position.X + 3, npc.position.Y + 1), 4, 4, 183, npc.velocity.X, npc.velocity.Y, 180, default(Color), 1.5f)];
                    //dust2.velocity *= 0f;
                    dust2.noGravity = true;
                    dust2.fadeIn = .3f;
                    dust2.velocity += npc.velocity;
                }

            }
            if (npc.ai[1] >= 400 && npc.ai[1] < 442)
            {
                if (npc.direction == 1) //Small eye dust to warn player that a jump-slash is ready...
                {
                    Dust dust2 = Main.dust[Dust.NewDust(new Vector2(npc.position.X + 9, npc.position.Y + 1), 4, 4, 183, npc.velocity.X, npc.velocity.Y, 180, default(Color), 0.8f)];
                    //dust2.velocity *= 0f;
                    dust2.noGravity = true;
                    dust2.fadeIn = .3f;
                    dust2.velocity += npc.velocity;
                }

                else
                {
                    Dust dust2 = Main.dust[Dust.NewDust(new Vector2(npc.position.X + 3, npc.position.Y + 1), 4, 4, 183, npc.velocity.X, npc.velocity.Y, 180, default(Color), 0.8f)];
                    //dust2.velocity *= 0f;
                    dust2.noGravity = true;
                    dust2.fadeIn = .3f;
                    dust2.velocity += npc.velocity;
                }
            }

            if (/*!shielding && */!slashing)
            {
                if (npc.ai[1] == 420 && npc.Distance(player.Center) < 120 && npc.Distance(player.Center) >= 45 && npc.velocity.Y == 0 && standing_on_solid_tile && Collision.CanHitLine(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0)) //If timer is at 0 and player is within slash range
                {
                    jumpSlashing = true;
                    shielding = false;
                }

                if (jumpSlashing)
                {
                    npc.knockBackResist = 0;
                    ++npc.ai[1];
                    if (npc.ai[1] < 436)
                    {
                        if (npc.direction == 1)
                        {
                            npc.velocity.X -= 0.15f;
                            if (npc.velocity.X < 0)
                            {
                                npc.velocity.X = 0;
                            }
                        }

                        else
                        {
                            npc.velocity.X += 0.15f;
                            if (npc.velocity.X > 0)
                            {
                                npc.velocity.X = 0;
                            }
                        }
                    }

                    if (npc.ai[1] == 436) //If timer is 46
                    {
                        if (npc.direction == 1)
                        {
                            npc.velocity.X += 5f;
                            npc.velocity.Y -= 3f;
                        }

                        else
                        {
                            npc.velocity.X -= 5f;
                            npc.velocity.Y -= 3f;
                        }
                    }

                    if (npc.ai[1] == 442) //If timer is 50
                    {
                        Main.PlaySound(SoundID.Item1.WithVolume(1f).WithPitchVariance(.3f), npc.position); //Play slash/swing sound

                        if (npc.direction == 1)
                        {
                            Projectile.NewProjectile(npc.Center + new Vector2(20, -20), new Vector2(0, 4f), ModContent.ProjectileType<Projectiles.Enemy.SmallWeaponSlash>(), 13, 5, Main.myPlayer, npc.whoAmI, 0);
                        }

                        else
                        {
                            Projectile.NewProjectile(npc.Center + new Vector2(-20, -20), new Vector2(0, 4f), ModContent.ProjectileType<Projectiles.Enemy.SmallWeaponSlash>(), 13, 5, Main.myPlayer, npc.whoAmI, 0);
                        }
                    }
                    if (npc.ai[1] > 470 && npc.ai[1] < 510)
                    {
                        if (npc.direction == 1)
                        {
                            npc.velocity.X -= 0.3f;
                            if (npc.velocity.X < 0)
                            {
                                npc.velocity.X = 0;
                            }
                        }

                        else
                        {
                            npc.velocity.X += 0.3f;
                            if (npc.velocity.X > 0)
                            {
                                npc.velocity.X = 0;
                            }
                        }
                    }
                    if (npc.ai[1] >= 510) //If timer is 510
                    {
                        jumpSlashing = false;
                        npc.ai[1] = 0; //Reset timer
                    }
                }
            }




            //Shielding

            if (shielding || npc.Distance(player.Center) < 220 || npc.ai[2] > 300)
            {
                npc.ai[2]++;

                if (!jumpSlashing && !slashing && npc.velocity.Y == 0)
                {
                    if (npc.ai[2] > 300 && npc.ai[2] <= 310)
                    {
                        if (npc.direction == 1) { npc.velocity.X -= 0.15f; }
                        else { npc.velocity.X += 0.15f; }
                    }

                    if (npc.ai[2] > 310)
                    {
                        npc.velocity.X = 0;
                        shielding = true;
                    }

                    if (shielding)
                    {
                        npc.knockBackResist = 0.3f;
                    }

                    if (npc.ai[2] > 500)
                    {
                        shielding = false;
                        npc.ai[2] = 0;
                    }
                }
            }
            #endregion
        }



        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D shieldTexture = mod.GetTexture("NPCs/Enemies/HollowWarrior_Shield");
            SpriteEffects effects = npc.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Rectangle myrectangle = shieldTexture.Frame(1, 10, 0, shieldFrame);
            if (shielding && npc.velocity.X == 0 && !jumpSlashing && !slashing)
            {
                if (npc.spriteDirection == 1)
                {
                    spriteBatch.Draw(shieldTexture, npc.Center - Main.screenPosition, myrectangle, lightColor, npc.rotation, new Vector2(32, 29), npc.scale, effects, 0f);
                }
                else
                {
                    spriteBatch.Draw(shieldTexture, npc.Center - Main.screenPosition, myrectangle, lightColor, npc.rotation, new Vector2(32, 29), npc.scale, effects, 0f);
                }
            }
        }

        public override void ModifyHitByItem(Player player, Item item, ref int damage, ref float knockback, ref bool crit)
        {
            if (shielding && !slashing && !jumpSlashing)
            {
                if (npc.ai[1] < 370)
                {
                    npc.ai[1] += 30; //Used for Jump-slash
                }

                if (npc.direction == 1)
                {
                    if (player.position.X > npc.position.X)
                    {
                        Main.PlaySound(SoundID.Dig, npc.position); //Play dig
                        damage -= 15;
                        if (npc.ai[2] > 350)
                        {
                            npc.ai[2] -= 20;
                        }
                    }
                }
                else
                {
                    if (player.position.X < npc.position.X)
                    {
                        Main.PlaySound(SoundID.Dig, npc.position); //Play dig
                        damage -= 15;
                        if (npc.ai[2] > 350)
                        {
                            npc.ai[2] -= 20;
                        }
                    }
                }
            }

            if (npc.direction == 1) //if enemy facing right
            {
                if (player.position.X < npc.position.X) //if hit in the back
                {
                    CombatText.NewText(new Rectangle((int)npc.Center.X, (int)npc.Bottom.Y, 10, 10), Color.Crimson, "Weak spot!", false, false);
                    damage = (int)(damage * 2f); //bonus damage
                    Main.PlaySound(SoundID.NPCHit18.WithVolume(1f).WithPitchVariance(0.3f), npc.position); //Play fleshy sound
                }
            }
            else //if enemy facing left
            {
                if (player.position.X > npc.position.X) //if hit in the back
                {
                    CombatText.NewText(new Rectangle((int)npc.Center.X, (int)npc.Bottom.Y, 10, 10), Color.Crimson, "Weak spot!", false, false);
                    damage = (int)(damage * 2f); //bonus damage
                    Main.PlaySound(SoundID.NPCHit18.WithVolume(1f).WithPitchVariance(0.3f), npc.position); //Play fleshy sound
                }
            }

            npc.ai[2] += 10;
        }

        public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[npc.target];
            if (projectile.type != ModContent.ProjectileType<Items.Weapons.Ranged.BlizzardBlasterShot>())
            {
                if (shielding && !slashing && !jumpSlashing)
                {

                    if (npc.direction == 1) //if npc facing right
                    {
                        if (projectile.oldPosition.X > npc.Center.X && projectile.melee && projectile.aiStyle != 19) //if proj moving toward npc front
                        {

                            Main.PlaySound(SoundID.Dig, npc.position, 1); //Play dig sound
                            damage -= 15;
                            knockback = 0.2f;

                            if (npc.ai[1] < 370)
                            {
                                npc.ai[1] += 30; //Used for Jump-slash
                            }

                            if (npc.ai[2] > 350)
                            {
                                npc.ai[2] -= 20;
                            }
                        }

                        else if (hitDirection == -1 && (!projectile.melee || projectile.aiStyle == 19))
                        {
                            Main.PlaySound(SoundID.Dig, npc.position, 1); //Play dig sound
                            damage -= 15;
                            knockback = 0.1f;

                            if (npc.ai[1] < 380)
                            {
                                npc.ai[1] += 40; //Used for Jump-slash
                            }


                            if (npc.ai[2] > 350)
                            {
                                npc.ai[2] -= 20;
                            }
                        }
                    }
                    else //if npc facing left
                    {
                        if (projectile.oldPosition.X < npc.Center.X && projectile.melee && projectile.aiStyle != 19) //if proj moving toward npc front
                        {
                            Main.PlaySound(SoundID.Dig, npc.position, 1); //Play dig sound
                            damage -= 15;
                            knockback = 0.2f;

                            if (npc.ai[1] < 370)
                            {
                                npc.ai[1] += 30; //Used for Jump-slash
                            }

                            if (npc.ai[2] > 350)
                            {
                                npc.ai[2] -= 20;
                            }
                        }
                        else if (hitDirection == 1 && (!projectile.melee || projectile.aiStyle == 19))
                        {
                            Main.PlaySound(SoundID.Dig, npc.position, 1); //Play dig sound
                            damage -= 15;

                            knockback = 0.1f;
                            if (npc.ai[1] < 370)
                            {
                                npc.ai[1] += 40; //Used for Jump-slash
                            }


                            if (npc.ai[2] > 350)
                            {
                                npc.ai[2] -= 20;
                            }
                        }
                    }
                }


                if (npc.direction == 1) //if enemy facing right
                {
                    if (projectile.oldPosition.X < npc.Center.X && projectile.melee && projectile.aiStyle != 19) //if hit in the back
                    {
                        CombatText.NewText(new Rectangle((int)npc.Center.X, (int)npc.Bottom.Y, 10, 10), Color.Crimson, "Weak spot!", false, false);
                        damage = (int)(damage * 2f); //bonus damage
                        Main.PlaySound(SoundID.NPCHit18.WithVolume(1f).WithPitchVariance(0.3f), npc.position); //Play fleshy sound
                    }
                    else if (hitDirection == 1)
                    {
                        CombatText.NewText(new Rectangle((int)npc.Center.X, (int)npc.Bottom.Y, 10, 10), Color.Crimson, "Weak spot!", false, false);
                        damage = (int)(damage * 2f); //bonus damage
                        Main.PlaySound(SoundID.NPCHit18.WithVolume(1f).WithPitchVariance(0.3f), npc.position); //Play fleshy sound
                    }
                }
                else //if enemy facing left
                {
                    if (projectile.oldPosition.X > npc.Center.X && projectile.melee && projectile.aiStyle != 19) //if hit in the back
                    {
                        CombatText.NewText(new Rectangle((int)npc.Center.X, (int)npc.Bottom.Y, 10, 10), Color.Crimson, "Weak spot!", false, false);
                        damage = (int)(damage * 2f); //bonus damage
                        Main.PlaySound(SoundID.NPCHit18.WithVolume(1f).WithPitchVariance(0.3f), npc.position); //Play fleshy sound
                    }
                    else if (hitDirection == -1)
                    {
                        CombatText.NewText(new Rectangle((int)npc.Center.X, (int)npc.Bottom.Y, 10, 10), Color.Crimson, "Weak spot!", false, false);
                        damage = (int)(damage * 2f); //bonus damage
                        Main.PlaySound(SoundID.NPCHit18.WithVolume(1f).WithPitchVariance(0.3f), npc.position); //Play fleshy sound
                    }
                }

                if (npc.Distance(player.Center) > 220 && !shielding)
                {
                    npc.ai[2] += 100;
                }

                if (npc.ai[1] < 400)
                {
                    npc.ai[1] += 10;
                }
            }
        }
    

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            float chance = 0;
            bool FrozenOcean = spawnInfo.spawnTileX > (Main.maxTilesX - 800);
            bool Ocean = spawnInfo.spawnTileX < 800 || FrozenOcean;

            if (spawnInfo.invasion)
            {
                chance = 0;
                return chance;
            }

            if (spawnInfo.player.townNPCs > 1f) return 0f;

            if (spawnInfo.water) return 0f;
            if (spawnInfo.player.ZoneGlowshroom) return 0f;

            if (Main.hardMode && spawnInfo.lihzahrd) return 0.15f;

            if (tsorcRevampWorld.SuperHardMode && !(Ocean || spawnInfo.player.ZoneJungle || spawnInfo.player.ZoneCorrupt || spawnInfo.player.ZoneCrimson || spawnInfo.player.ZoneUnderworldHeight)) return 0.23f;
            if (tsorcRevampWorld.SuperHardMode && spawnInfo.player.ZoneOverworldHeight && !(Ocean || spawnInfo.player.ZoneJungle || spawnInfo.player.ZoneCorrupt || spawnInfo.player.ZoneCrimson || spawnInfo.player.ZoneSkyHeight)) return 0.25f;
            if (tsorcRevampWorld.SuperHardMode && spawnInfo.player.ZoneDesert && !Ocean) return 0.13f;
            if (tsorcRevampWorld.SuperHardMode && spawnInfo.player.ZoneDungeon && !spawnInfo.player.ZoneUnderworldHeight) return 0.2f; //.08% is 4.28%


            if (Main.expertMode && Main.bloodMoon && spawnInfo.player.ZoneOverworldHeight && !(Ocean || spawnInfo.player.ZoneCorrupt || spawnInfo.player.ZoneCrimson || spawnInfo.player.ZoneSkyHeight)) return chance = 0.075f;

            if (Main.expertMode && Main.bloodMoon && !(Ocean || spawnInfo.player.ZoneJungle || spawnInfo.player.ZoneHoly || spawnInfo.player.ZoneCorrupt || spawnInfo.player.ZoneCrimson || spawnInfo.player.ZoneSnow || spawnInfo.player.ZoneSkyHeight || spawnInfo.player.ZoneUnderworldHeight)) return chance = 0.04f;

            if (((!Main.expertMode && (NPC.downedBoss1 || NPC.downedBoss2)) || Main.expertMode) && spawnInfo.player.ZoneOverworldHeight && Main.dayTime && !(Ocean || spawnInfo.player.ZoneJungle || spawnInfo.player.ZoneHoly || spawnInfo.player.ZoneCorrupt || spawnInfo.player.ZoneCrimson || spawnInfo.player.ZoneSnow || spawnInfo.player.ZoneBeach || spawnInfo.player.ZoneSkyHeight)) return chance = 0.05f;
            if (((!Main.expertMode && (NPC.downedBoss1 || NPC.downedBoss2)) || Main.expertMode) && spawnInfo.player.ZoneOverworldHeight && !Main.dayTime && !(spawnInfo.player.ZoneCorrupt || spawnInfo.player.ZoneCrimson)) return chance = 0.09f;

            if (((!Main.expertMode && (NPC.downedBoss1 || NPC.downedBoss2)) || Main.expertMode) && (spawnInfo.player.ZoneDirtLayerHeight || spawnInfo.player.ZoneRockLayerHeight) && Main.dayTime && !(Ocean || spawnInfo.player.ZoneJungle || spawnInfo.player.ZoneHoly || spawnInfo.player.ZoneCorrupt || spawnInfo.player.ZoneCrimson || spawnInfo.player.ZoneSnow)) return chance = 0.07f;
            if (((!Main.expertMode && (NPC.downedBoss1 || NPC.downedBoss2)) || Main.expertMode) && (spawnInfo.player.ZoneDirtLayerHeight || spawnInfo.player.ZoneRockLayerHeight) && !Main.dayTime && !(Ocean || spawnInfo.player.ZoneJungle || spawnInfo.player.ZoneHoly || spawnInfo.player.ZoneCorrupt || spawnInfo.player.ZoneCrimson || spawnInfo.player.ZoneSnow)) return chance = 0.1f;


            if ((!Main.expertMode && !(spawnInfo.player.ZoneUnderworldHeight || spawnInfo.player.ZoneJungle || spawnInfo.player.ZoneCorrupt || spawnInfo.player.ZoneCrimson) && (NPC.downedBoss1 || NPC.downedBoss2)) || Main.expertMode) return chance = 0.03f;

            return chance;
        }

        public override void NPCLoot()
        {
            Player player = Main.player[npc.target];

            Item.NewItem(npc.getRect(), ModContent.ItemType<Items.SoulShekel>(), 1 + Main.rand.Next(1, 3));
            if (Main.rand.Next(15) == 0) Item.NewItem(npc.getRect(), mod.ItemType("FadingSoul"));
            if (Main.rand.Next(15) == 0) Item.NewItem(npc.getRect(), ModContent.ItemType<Items.Potions.Lifegem>());
            if (Main.rand.Next(15) == 0 && player.GetModPlayer<tsorcRevampPlayer>().BearerOfTheCurse) Item.NewItem(npc.getRect(), ModContent.ItemType<Items.Potions.Lifegem>());


            if (Main.rand.Next(10) == 0)
            { //10%
                Item.NewItem(npc.getRect(), ItemID.IronskinPotion);
            }

        }

        #region Drawing and Animation

        public override void FindFrame(int frameHeight)
        {

            if (npc.velocity.X != 0) //Walking
            {
                float framecountspeed = Math.Abs(npc.velocity.X) * 2.2f;
                npc.frameCounter += framecountspeed;
                npc.spriteDirection = npc.direction;

                if (npc.frameCounter < 12)
                {
                    npc.frame.Y = 2 * frameHeight;
                }
                else if (npc.frameCounter < 24)
                {
                    npc.frame.Y = 3 * frameHeight;
                }
                else if (npc.frameCounter < 36)
                {
                    npc.frame.Y = 4 * frameHeight;
                }
                else if (npc.frameCounter < 48)
                {
                    npc.frame.Y = 5 * frameHeight;
                }
                else if (npc.frameCounter < 60)
                {
                    npc.frame.Y = 6 * frameHeight;
                }
                else if (npc.frameCounter < 72)
                {
                    npc.frame.Y = 7 * frameHeight;
                }
                else if (npc.frameCounter < 84)
                {
                    npc.frame.Y = 8 * frameHeight;
                }
                else if (npc.frameCounter < 96)
                {
                    npc.frame.Y = 9 * frameHeight;
                }
                else
                {
                    npc.frameCounter = 0;
                }
            }

            if (npc.velocity.Y != 0 && (!jumpSlashing || !shielding)) //If falling/jumping
            {
                npc.frame.Y = 1 * frameHeight;
            }

            if (slashing) //If slashing
            {
                npc.spriteDirection = npc.direction;

                if (npc.ai[3] < 18)
                {
                    npc.frame.Y = 11 * frameHeight;
                }
                else if (npc.ai[3] < 26)
                {
                    npc.frame.Y = 12 * frameHeight;
                }
                else if (npc.ai[3] < 29)
                {
                    npc.frame.Y = 13 * frameHeight;
                }
                else if (npc.ai[3] < 32)
                {
                    npc.frame.Y = 14 * frameHeight;
                }
                else if (npc.ai[3] < 35)
                {
                    npc.frame.Y = 15 * frameHeight;
                }
                else if (npc.ai[3] < 49)
                {
                    npc.frame.Y = 16 * frameHeight;
                }
            }
            if (jumpSlashing) //If jumpslashing
            {
                npc.spriteDirection = npc.direction;

                if (npc.ai[1] < 428)
                {
                    npc.frame.Y = 11 * frameHeight;
                }
                else if (npc.ai[1] < 436)
                {
                    npc.frame.Y = 12 * frameHeight;
                }
                else if (npc.ai[1] < 439)
                {
                    npc.frame.Y = 13 * frameHeight;
                }
                else if (npc.ai[1] < 442)
                {
                    npc.frame.Y = 14 * frameHeight;
                }
                else if (npc.ai[1] < 445)
                {
                    npc.frame.Y = 15 * frameHeight;
                }
                else if (npc.ai[1] < 510)
                {
                    npc.frame.Y = 16 * frameHeight;
                }
            }

            if (npc.velocity.X == 0 && npc.velocity.Y == 0 && shielding && !jumpSlashing && !slashing) //If not moving at all (shielding)
            {
                npc.spriteDirection = npc.direction;
                npc.frame.Y = 10 * frameHeight;
            }

            if (shielding)
            {
                shieldAnimTimer++;

                if (shieldAnimTimer < 4)
                {
                    shieldFrame = 0 * 1;
                }
                else if (shieldAnimTimer < 8)
                {
                    shieldFrame = 1 * 1;
                }
                else if (shieldAnimTimer < 12)
                {
                    shieldFrame = 2 * 1;
                }
                else if (shieldAnimTimer < 16)
                {
                    shieldFrame = 3 * 1;
                }
                else if (shieldAnimTimer < 20)
                {
                    shieldFrame = 4 * 1;
                }
                else if (shieldAnimTimer < 24)
                {
                    shieldFrame = 5 * 1;
                }
                else if (shieldAnimTimer < 28)
                {
                    shieldFrame = 6 * 1;
                }
                else if (shieldAnimTimer < 32)
                {
                    shieldFrame = 7 * 1;
                }
                else if (shieldAnimTimer < 36)
                {
                    shieldFrame = 8 * 1;
                }
                else if (shieldAnimTimer < 40)
                {
                    shieldFrame = 9 * 1;
                }
                else if (shieldAnimTimer < 44)
                {
                    shieldFrame = 8 * 1;
                }
                else if (shieldAnimTimer < 48)
                {
                    shieldFrame = 7 * 1;
                }
                else if (shieldAnimTimer < 52)
                {
                    shieldFrame = 6 * 1;
                }
                else if (shieldAnimTimer < 56)
                {
                    shieldFrame = 5 * 1;
                }
                else if (shieldAnimTimer < 60)
                {
                    shieldFrame = 4 * 1;
                }
                else if (shieldAnimTimer < 64)
                {
                    shieldFrame = 3 * 1;
                }
                else if (shieldAnimTimer < 68)
                {
                    shieldFrame = 2 * 1;
                }
                else if (shieldAnimTimer < 72)
                {
                    shieldFrame = 1 * 1;
                }
                else
                {
                    shieldAnimTimer = 0;
                }
            }
        }

        #endregion

    }
}
