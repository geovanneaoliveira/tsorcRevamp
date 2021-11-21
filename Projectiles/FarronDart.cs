﻿using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;

namespace tsorcRevamp.Projectiles
{
    class FarronDart : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.friendly = true;
            projectile.magic = true;
            projectile.tileCollide = true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, new Rectangle(0, 0, 6, 8), Color.White, projectile.rotation, new Vector2(3, 4), projectile.scale, SpriteEffects.None, 0);

            return false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            // change the hitbox size, centered about the original projectile center. This makes the projectile have small aoe.
            projectile.position.X = projectile.position.X + (float)(projectile.width / 2);
            projectile.position.Y = projectile.position.Y + (float)(projectile.height / 2);
            projectile.width = 20;
            projectile.height = 20;
            projectile.position.X = projectile.position.X - (float)(projectile.width / 2);
            projectile.position.Y = projectile.position.Y - (float)(projectile.height / 2);

            projectile.timeLeft = 2;
        }

        public override void AI()
        {
			if (projectile.velocity.X > 0) //if going right
			{
				for (int d = 0; d < 4; d++)
				{
					int num44 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y - 2), projectile.width, projectile.height, 68, projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 100, default(Color), 1f);
					Main.dust[num44].noGravity = true;
					Main.dust[num44].velocity *= 0f;
				}

				for (int d = 0; d < 4; d++)
				{
					int num45 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y - 2), projectile.width - 4, projectile.height, 68, projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 100, default(Color), .5f);
					Main.dust[num45].noGravity = true;
					Main.dust[num45].velocity *= 0f;
					Main.dust[num45].fadeIn *= 1f;
				}
			}
			else //if going left
            {
				for (int d = 0; d < 4; d++)
				{
					int num44 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y - 1), projectile.width, projectile.height, 68, projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 100, default(Color), 1f);
					Main.dust[num44].noGravity = true;
					Main.dust[num44].velocity *= 0f;
				}

				for (int d = 0; d < 4; d++)
				{
					int num45 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y - 1), projectile.width - 4, projectile.height, 68, projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 100, default(Color), .5f);
					Main.dust[num45].noGravity = true;
					Main.dust[num45].velocity *= 0f;
					Main.dust[num45].fadeIn *= 1f;
				}
			}

            Lighting.AddLight(projectile.Center, .200f, .200f, .350f);


            if (Main.myPlayer == projectile.owner && projectile.ai[0] == 0f)
            {
                if (Main.player[projectile.owner].channel)
                {
                    float num48 = 10f;
                    Vector2 vector6 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
                    float num49 = (float)Main.mouseX + Main.screenPosition.X - vector6.X;
                    float num50 = (float)Main.mouseY + Main.screenPosition.Y - vector6.Y;
                    float num51 = (float)Math.Sqrt((double)(num49 * num49 + num50 * num50));
                    num51 = (float)Math.Sqrt((double)(num49 * num49 + num50 * num50));
                    if (num51 > num48)
                    {
                        num51 = num48 / num51;
                        num49 *= num51;
                        num50 *= num51;
                        int num52 = (int)(num49 * 1000f);
                        int num53 = (int)(projectile.velocity.X * 1000f);
                        int num54 = (int)(num50 * 1000f);
                        int num55 = (int)(projectile.velocity.Y * 1000f);
                        if (num52 != num53 || num54 != num55)
                        {
                            projectile.netUpdate = true;
                        }
                        projectile.velocity.X = num49;
                        projectile.velocity.Y = num50;
                    }
                    else
                    {
                        int num56 = (int)(num49 * 1000f);
                        int num57 = (int)(projectile.velocity.X * 1000f);
                        int num58 = (int)(num50 * 1000f);
                        int num59 = (int)(projectile.velocity.Y * 1000f);
                        if (num56 != num57 || num58 != num59)
                        {
                            projectile.netUpdate = true;
                        }
                        projectile.velocity.X = num49;
                        projectile.velocity.Y = num50;
                    }
                }
                else
                {
                    if (projectile.ai[0] == 0f)
                    {
                        projectile.ai[0] = 1f;
                        projectile.netUpdate = true;
                        float num60 = 10f;
                        Vector2 vector7 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
                        float num61 = (float)Main.mouseX + Main.screenPosition.X - vector7.X;
                        float num62 = (float)Main.mouseY + Main.screenPosition.Y - vector7.Y;
                        float num63 = (float)Math.Sqrt((double)(num61 * num61 + num62 * num62));
                        if (num63 == 0f)
                        {
                            vector7 = new Vector2(Main.player[projectile.owner].position.X + (float)(Main.player[projectile.owner].width / 2), Main.player[projectile.owner].position.Y + (float)(Main.player[projectile.owner].height / 2));
                            num61 = projectile.position.X + (float)projectile.width * 0.5f - vector7.X;
                            num62 = projectile.position.Y + (float)projectile.height * 0.5f - vector7.Y;
                            num63 = (float)Math.Sqrt((double)(num61 * num61 + num62 * num62));
                        }
                        num63 = num60 / num63;
                        num61 *= num63;
                        num62 *= num63;
                        projectile.velocity.X = num61;
                        projectile.velocity.Y = num62;
                        if (projectile.velocity.X == 0f && projectile.velocity.Y == 0f)
                        {
                            projectile.Kill();
                        }
                    }
                }
            }
            projectile.rotation += 0.3f * (float)projectile.direction;

        }

        public override void Kill(int timeLeft)
        {
            for (int d = 0; d < 14; d++)
            {
                int dust = Dust.NewDust(projectile.Center, 8, 8, 68, projectile.velocity.X * 1f, projectile.velocity.Y * 1f, 30, default(Color), 1f);
                Main.dust[dust].noGravity = true;
            }

            Main.PlaySound(SoundID.NPCHit3.WithVolume(.35f), projectile.position);

        }
    }
}