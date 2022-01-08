﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace tsorcRevamp.NPCs.Enemies
{
	class TibianAmazon : ModNPC
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Tibian Amazon");
			Main.npcFrameCount[npc.type] = Main.npcFrameCount[NPCID.Skeleton];
		}

		public int throwingKnifeDamage = 8;

		public override void SetDefaults()
		{

			

			animationType = NPCID.Skeleton;
			npc.aiStyle = -1;
			npc.height = 40;
			npc.width = 20;
			npc.lifeMax = 85;
			npc.damage = 20;
			npc.scale = 1f;
			npc.HitSound = SoundID.NPCHit1;
			npc.DeathSound = SoundID.NPCDeath1;
			npc.knockBackResist = .6f;
			npc.value = 250;
			npc.defense = 2;
			banner = npc.type;
			bannerItem = ModContent.ItemType<Banners.TibianAmazonBanner>();

			if (Main.hardMode)
			{
				npc.lifeMax = 180;
				npc.defense = 16;
				npc.value = 320;
				npc.damage = 50;
				throwingKnifeDamage = 20;
			}
		}

		public override void NPCLoot()
		{
			Item.NewItem(npc.getRect(), ItemID.Torch);
			Item.NewItem(npc.getRect(), ItemID.ThrowingKnife, Main.rand.Next(20, 50));
			if (!Main.hardMode && Main.rand.Next(10) == 0) Item.NewItem(npc.getRect(), ModContent.ItemType<Items.Armors.RedMageTunic>());
			if (!Main.hardMode && Main.rand.Next(10) == 0) Item.NewItem(npc.getRect(), ModContent.ItemType<Items.Armors.RedMagePants>());
			if (!Main.hardMode && Main.rand.Next(10) == 0) Item.NewItem(npc.getRect(), ModContent.ItemType<Items.Armors.RedMageHat>());
			if (!Main.hardMode && Main.rand.Next(10) == 0) Item.NewItem(npc.getRect(), ModContent.ItemType<Items.Weapons.Melee.OldDoubleAxe>(), 1, false, -1);
			if (Main.rand.Next(20) == 0) Item.NewItem(npc.getRect(), ModContent.ItemType<Items.DeadChicken>());
		}


		int drownTimerMax = 2000;
		int drownTimer = 2000;
		int drowningRisk = 1200;

		int boredTimer = 0;
		int tBored = 1;//increasing this increases how long it take for the NP to get bored
		int boredResetT = 0;
		int bReset = 70;//increasing this will increase how long an NPC "gives up" before coming back to try again.


		//Spawns on the Surface and into the Underground. Does not spawn in the Dungeon, Hardmode, Meteor, or if there are Town NPCs.

		#region Spawn

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			float chance = 0;

			if (spawnInfo.invasion)
			{
				chance = 0;
				return chance;
			}

			if (spawnInfo.player.townNPCs > 0f || tsorcRevampWorld.SuperHardMode || spawnInfo.player.ZoneDungeon) chance = 0f;
			if (!tsorcRevampWorld.SuperHardMode && (spawnInfo.player.ZoneOverworldHeight || spawnInfo.player.ZoneDirtLayerHeight || spawnInfo.player.ZoneRockLayerHeight))
			{
				if (!(spawnInfo.player.ZoneCorrupt || spawnInfo.player.ZoneCrimson)) return 0.05f;
				if (!(spawnInfo.player.ZoneCorrupt || spawnInfo.player.ZoneCrimson) && !Main.dayTime) return 0.055f;
				if (!(spawnInfo.player.ZoneCorrupt || spawnInfo.player.ZoneCrimson) && Main.dayTime) return 0.0534f;
				if (spawnInfo.player.ZoneMeteor && !Main.dayTime) return 0.0725f;
			}

			return chance;
		}
		#endregion

		#region AI // code by GrtAndPwrflTrtl (http://www.terrariaonline.com/members/grtandpwrfltrtl.86018/)
		public override void AI()  //  warrior ai
		{
			#region set up NPC's attributes & behaviors
			// set parameters
			//  is_archer OR can_pass_doors OR shoot_and_walk, pick only 1.  They use the same ai[] vars (1&2)
			bool is_archer = false; // stops and shoots when target sighted; skel archer & gob archer are the archers
			bool shoot_and_walk = true;  //  can shoot while walking like clown; uses ai[2] so cannot be used with is_archer or can_pass_doors

			//  can_teleport==true code uses boredom_time and ai[3] (boredom), but not mutually exclusive
			bool can_teleport = false;  //  tp around like chaos ele
			int boredom_time = 60; // time until it stops targeting player if blocked etc, 60 for anything but chaos ele, 20 for chaos ele
			int boredom_cooldown = 10 * boredom_time; // boredom level where boredom wears off; usually 10*boredom_time

			float acceleration = .15f;  //  how fast it can speed up
			float top_speed = 1.8f;  //  max walking speed, also affects jump length
			float braking_power = .2f;  //  %of speed that can be shed every tick when above max walking speed

			float enrage_percentage = .2f;  //  double movement speed below this life fraction. 0 for no enrage. Mummies enrage below .5
			float enrage_acceleration = .2f;  //  faster when enraged, usually 2*acceleration
			float enrage_top_speed = 2.2f;  //  faster when enraged, usually 2*top_speed

			bool clown_sized = false; // is hitbox the same as clowns' for purposes of when to jump?
			bool jump_gaps = true; // attempt to jump gaps; everything but crabs do this

			// Omnirs creature sorts
			bool tooBig = false; // force bigger creatures to jump
			bool lavaJumping = false; // Enemies jump on lava.
			bool canDrown = false; // They will drown if in the water for too long


			// calculated parameters
			bool moonwalking = false;  //  not jump/fall and moving backwards to facing
			if (npc.velocity.Y == 0f && ((npc.velocity.X > 0f && npc.direction < 0) || (npc.velocity.X < 0f && npc.direction > 0)))
				moonwalking = true;
			#endregion
			//-------------------------------------------------------------------
			#region Too Big and Lava Jumping
			if (tooBig)
			{
				if (npc.velocity.Y == 0f && (npc.velocity.X == 0f && npc.direction < 0))
				{
					npc.velocity.Y -= 8f;
					npc.velocity.X -= 2f;
				}
				else if (npc.velocity.Y == 0f && (npc.velocity.X == 0f && npc.direction > 0))
				{
					npc.velocity.Y -= 8f;
					npc.velocity.X += 2f;
				}
			}
			if (lavaJumping)
			{
				if (npc.lavaWet)
				{
					npc.velocity.Y -= 2;
				}
			}
			#endregion
			//-------------------------------------------------------------------
			#region enrage
			bool enraged = false; // angry from damage; not stored from tick to tick
			if ((enrage_percentage > 0) && (npc.life < (float)npc.lifeMax * enrage_percentage))  //  speed up at low life
				enraged = true;
			if (enraged)
			{ // speed up movement if enraged
				acceleration = enrage_acceleration;
				top_speed = enrage_top_speed;
			}
			#endregion
			//-------------------------------------------------------------------
			#region melee movement
			if (!is_archer || (npc.ai[2] <= 0f && !npc.confused))  //  meelee attack/movement. archers only use while not aiming
			{
				if (Math.Abs(npc.velocity.X) > top_speed)  //  running/flying faster than top speed
				{
					if (npc.velocity.Y == 0f)  //  and not jump/fall
						npc.velocity *= (1f - braking_power);  //  decelerate
				}
				else if ((npc.velocity.X < top_speed && npc.direction == 1) || (npc.velocity.X > -top_speed && npc.direction == -1))
				{  //  running slower than top speed (forward), can be jump/fall
					if (can_teleport && moonwalking)
						npc.velocity.X = npc.velocity.X * 0.99f;  //  ? small decelerate for teleporters

					npc.velocity.X = npc.velocity.X + (float)npc.direction * acceleration;  //  accellerate fwd; can happen midair
					if ((float)npc.direction * npc.velocity.X > top_speed)
						npc.velocity.X = (float)npc.direction * top_speed;  //  but cap at top speed
				}  //  END running slower than top speed (forward), can be jump/fall
			} // END non archer or not aiming*/

			if (npc.ai[1] == 0)
			{
				npc.TargetClosest(true); //  Target the closest player & face him (If passed as a parameter, a bool will determine whether it should face the target or not)
			}

			//Turn and walk away if hitting a wall
			if (npc.position.X == npc.oldPosition.X)
			{
				npc.ai[1]++;
				if (npc.ai[1] > 120 && npc.velocity.Y == 0)
				{
					npc.direction *= -1;
					npc.spriteDirection = npc.direction;
					npc.ai[1] = 50;
				}
			}

			Player player = Main.player[npc.target];

			if (npc.ai[1] == 51 && npc.Distance(player.Center) > 1100)
            {
				npc.ai[1] = 0;
			}

			if (Collision.CanHitLine(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0))
			{
				npc.ai[1] = 0;
			}

			#endregion
			//-------------------------------------------------------------------
			#region shoot and walk
			if (shoot_and_walk && Main.netMode != 1 && !Main.player[npc.target].dead) // can generalize this section to moving+projectile code 
			{
				#region Projectiles
				npc.ai[2]++;
				if (npc.ai[2] >= 60f)
				{
					if (npc.justHit)
						npc.ai[2] = 60f; // reset throw countdown when hit

					if (npc.ai[2] >= 90f && Collision.CanHitLine(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0))
					{
						if (npc.Distance(player.Center) < 250)
						{
							float num48 = 8f;
							Vector2 vector8 = new Vector2(npc.position.X + (npc.width * 0.5f), npc.position.Y + (npc.height / 2));
							float speedX = ((Main.player[npc.target].position.X + (Main.player[npc.target].width * 0.5f)) - vector8.X) + Main.rand.Next(-25, 5);
							float speedY = ((Main.player[npc.target].position.Y + (Main.player[npc.target].height * 0.5f)) - vector8.Y) + Main.rand.Next(-25, 5);
							if (((speedX < 0f) && (npc.velocity.X < 0f)) || ((speedX > 0f) && (npc.velocity.X > 0f)))
							{
								float num51 = (float)Math.Sqrt((double)((speedX * speedX) + (speedY * speedY)));
								num51 = num48 / num51;
								speedX *= num51;
								speedY *= num51;
								//int throwingKnifeDamage = 8;//(int) (14f * npc.scale);
								int type = ModContent.ProjectileType<Projectiles.Enemy.EnemyThrowingKnife>();//44;//0x37; //14;
								int num54 = Projectile.NewProjectile(vector8.X, vector8.Y, speedX, speedY, type, throwingKnifeDamage, 0f, Main.myPlayer);
								Main.projectile[num54].timeLeft = 600;
								Main.projectile[num54].aiStyle = 1;
								Main.PlaySound(2, (int)npc.position.X, (int)npc.position.Y, 0x11);
								npc.ai[2] = 0;
							}
							npc.netUpdate = true;
						}
					}
				}
				#endregion
			}
			#endregion
			//-------------------------------------------------------------------
			#region check if standing on a solid tile
			// warning: this section contains a return statement
			bool standing_on_solid_tile = false;
			if (npc.velocity.Y == 0f) // no jump/fall
			{
				int y_below_feet = (int)(npc.position.Y + (float)npc.height + 8f) / 16;
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
			//-------------------------------------------------------------------
			#region new Tile()s
			if (standing_on_solid_tile)  //  if standing on solid tile
			{
				int x_in_front = (int)((npc.position.X + (float)(npc.width / 2) + (float)(15 * npc.direction)) / 16f); // 15 pix in front of center of mass
				int y_above_feet = (int)((npc.position.Y + (float)npc.height - 15f) / 16f); // 15 pix above feet
				if (clown_sized)
					x_in_front = (int)((npc.position.X + (float)(npc.width / 2) + (float)((npc.width / 2 + 16) * npc.direction)) / 16f); // 16 pix in front of edge
																																		 //  create? 5 tile high stack in front
				if (Main.tile[x_in_front, y_above_feet] == null)
					Main.tile[x_in_front, y_above_feet] = new Tile();

				if (Main.tile[x_in_front, y_above_feet - 1] == null)
					Main.tile[x_in_front, y_above_feet - 1] = new Tile();

				if (Main.tile[x_in_front, y_above_feet - 2] == null)
					Main.tile[x_in_front, y_above_feet - 2] = new Tile();

				if (Main.tile[x_in_front, y_above_feet - 3] == null)
					Main.tile[x_in_front, y_above_feet - 3] = new Tile();

				if (Main.tile[x_in_front, y_above_feet + 1] == null)
					Main.tile[x_in_front, y_above_feet + 1] = new Tile();
				//  create? 2 other tiles farther in front
				if (Main.tile[x_in_front + npc.direction, y_above_feet - 1] == null)
					Main.tile[x_in_front + npc.direction, y_above_feet - 1] = new Tile();

				if (Main.tile[x_in_front + npc.direction, y_above_feet + 1] == null)
					Main.tile[x_in_front + npc.direction, y_above_feet + 1] = new Tile();

				#endregion
				//-------------------------------------------------------------------
				#region jumping, reset door knock & damage counters
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
						else if (npc.directionY < 0 && jump_gaps && (!Main.tile[x_in_front, y_above_feet + 1].active() || !Main.tileSolid[(int)Main.tile[x_in_front, y_above_feet + 1].type]) && (!Main.tile[x_in_front + npc.direction, y_above_feet + 1].active() || !Main.tileSolid[(int)Main.tile[x_in_front + npc.direction, y_above_feet + 1].type]))
						{ // rising? & jumps gaps & no solid tile ahead to step on for 2 spaces in front
							npc.velocity.Y = -8f; // jump with power 8
							npc.velocity.X = npc.velocity.X * 1.5f; // jump forward hard as well; we're trying to jump a gap
							npc.netUpdate = true;
						}

					} // END moving forward, still: standing on solid tile but not in front of a passable door
				}
			}
			#endregion
			//-------------------------------------------------------------------
			#region drown // code by Omnir
			if (canDrown)
			{
				if (!npc.wet)
				{
					npc.TargetClosest(true);
					drownTimer = drownTimerMax;
				}
				if (npc.wet)
				{
					drownTimer--;
				}
				if (npc.wet && drownTimer > drowningRisk)
				{
					npc.TargetClosest(true);
				}
				else if (npc.wet && drownTimer <= drowningRisk)
				{
					npc.TargetClosest(false);
					if (npc.timeLeft > 10)
					{
						npc.timeLeft = 10;
					}
					npc.directionY = -1;
					if (npc.velocity.Y > 0f)
					{
						npc.direction = 1;
					}
					npc.direction = -1;
					if (npc.velocity.X > 0f)
					{
						npc.direction = 1;
					}
				}
				if (drownTimer <= 0)
				{
					npc.life--;
					if (npc.life <= 0)
					{
						Main.PlaySound(4, (int)npc.position.X, (int)npc.position.Y, 1);
						npc.NPCLoot();
						npc.netUpdate = true;
					}
				}
			}
			#endregion
			//-------------------------------------------------------------------*/


		}
		#endregion

		public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
		{
			if (npc.ai[2] >= 60)
			{
				Texture2D knifeTexture = mod.GetTexture("NPCs/Enemies/TibianAmazon_Knife");
				SpriteEffects effects = npc.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
				if (npc.spriteDirection == -1)
				{
					spriteBatch.Draw(knifeTexture, npc.Center - Main.screenPosition, new Rectangle(npc.frame.X, npc.frame.Y, 60, 56), drawColor, npc.rotation, new Vector2(30, 32), npc.scale, effects, 0);
				}
				else
				{
					spriteBatch.Draw(knifeTexture, npc.Center - Main.screenPosition, new Rectangle(npc.frame.X, npc.frame.Y, 60, 56), drawColor, npc.rotation, new Vector2(30, 32), npc.scale, effects, 0);
				}
			}
		}

		#region Gore
		public override void HitEffect(int hitDirection, double damage)
		{
			for (int i = 0; i < 25; i++)
			{
				int dustType = 5;
				int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
				Dust dust = Main.dust[dustIndex];
				dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.06f;
				dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.06f;
				dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
				dust.noGravity = true;
			}
			if (npc.life <= 0)
			{
				for (int i = 0; i < 4; i++)
				{
					Dust.NewDust(npc.position, npc.width, npc.height, 5, Main.rand.Next(-3, 3), Main.rand.Next(-3, 3), 70, default(Color), 1f);
				}

				Gore.NewGore(npc.position, new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), mod.GetGoreSlot("Gores/Tibian Amazon Gore 1"), 1f);
				Gore.NewGore(npc.position, new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), mod.GetGoreSlot("Gores/Tibian Amazon Gore 2"), 1f);
				Gore.NewGore(npc.position, new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), mod.GetGoreSlot("Gores/Tibian Amazon Gore 3"), 1f);
				Gore.NewGore(npc.position, new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), mod.GetGoreSlot("Gores/Tibian Amazon Gore 2"), 1f);
				Gore.NewGore(npc.position, new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), mod.GetGoreSlot("Gores/Tibian Amazon Gore 3"), 1f);
			}
		}
		#endregion
	}
}