using RealTimeProject;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealTimeProject
{
    public class GameLogic
    {
        public static GameState NextState(Input[] prevInputs, Input[] curInputs, GameState state)
        {
            GameState nextState = new GameState(state);
            int pCount = prevInputs.Length;
            Vector2[] accArr = new Vector2[pCount];
            for (int i = 0; i < pCount; i++)
            {
                accArr[i] = new Vector2(0, 0);
            }
            //block handling
            for (int i = 0; i < pCount; i++)
            {
                PlayerState playerI = nextState.PStates[i];
                if (playerI.Stocks == 0)
                    continue;
                if ((curInputs[i] & Input.Block) != 0 && (prevInputs[i] & Input.Block) == 0 && playerI.BFrame == 0)
                {
                    if (playerI.StunFrame == 0)
                    {
                        playerI.BFrame++;
                    }
                }
                else if (playerI.BFrame > 0)
                {
                    playerI.BFrame++;
                }
                if (playerI.BFrame == GameVariables.BlockDur + GameVariables.BlockCD + 1)
                {
                    playerI.BFrame = 0;
                }
            }
            //attack handling
            for (int i = 0; i < pCount; i++)
            {
                PlayerState playerI = nextState.PStates[i];
                if (playerI.Stocks == 0)
                    continue;
                bool isOnFloor = false;
                if (playerI.Pos.Y == GameVariables.FloorY - GameVariables.PlayerSize.Height)
                {
                    isOnFloor = true;
                }
                if ((curInputs[i] & Input.LAttack) != 0 && (prevInputs[i] & Input.LAttack) == 0 && playerI.AttackFrame == 0)
                {
                    if ((curInputs[i] & Input.Up) != 0)
                        playerI.AttackName = AttackName.ULight;
                    else if ((curInputs[i] & (Input.Right | Input.Left)) != 0)
                        playerI.AttackName = AttackName.SLight;
                    else
                    {
                        if (isOnFloor)
                            playerI.AttackName = AttackName.NLight;
                        else
                            playerI.AttackName = AttackName.NAir;
                    }
                }
                else if ((curInputs[i] & Input.HAttack) != 0 && (prevInputs[i] & Input.HAttack) == 0 && playerI.AttackFrame == 0)
                {
                    if (isOnFloor)
                    {
                        if ((curInputs[i] & Input.Up) != 0)
                            playerI.AttackName = AttackName.UHeavy;
                        else if ((curInputs[i] & (Input.Right | Input.Left)) != 0)
                            playerI.AttackName = AttackName.SHeavy;
                        else
                            playerI.AttackName = AttackName.NHeavy;
                    }
                }
                if (playerI.AttackName != AttackName.None)
                {
                    Attack attack = GameVariables.AttackDict[playerI.AttackName];
                    AnimHitbox[] anim = attack.Animation;
                    AnimHitbox ah = new AnimHitbox();
                    playerI.AttackFrame++;
                    if (attack.StartupF < playerI.AttackFrame && playerI.AttackFrame <= attack.StartupF + anim.Last().endF)
                    {
                        for (int j = 0; j < pCount; j++)
                        {
                            PlayerState playerJ = nextState.PStates[j];
                            if (j != i)
                            {
                                for (int k = 0; k < anim.Length; k++)
                                {
                                    if (playerI.AttackFrame - attack.StartupF <= anim[k].endF)
                                    {
                                        ah = anim[k];
                                        break;
                                    }
                                }
                                RectangleF actualHitbox;
                                if (!playerI.FacingLeft)
                                {
                                    actualHitbox = new RectangleF(new PointF(playerI.Pos.X + 25 + ah.hitbox.X, playerI.Pos.Y + 25 + ah.hitbox.Y),
                                                                    ah.hitbox.Size);
                                }
                                else
                                {
                                    actualHitbox = new RectangleF(new PointF(playerI.Pos.X + 25 - ah.hitbox.X - ah.hitbox.Width,
                                        playerI.Pos.Y + 25 + ah.hitbox.Y), ah.hitbox.Size);
                                }
                                if (new RectangleF(playerJ.Pos.ToPoint(), GameVariables.PlayerSize).IntersectsWith(actualHitbox))
                                {
                                    if (playerJ.BFrame > 0 && playerJ.BFrame < GameVariables.BlockDur + 1)
                                    {
                                        playerI.AttackFrame = 0;
                                        playerI.StunFrame = 30;
                                        playerI.AttackName = AttackName.None;
                                    }
                                    else
                                    {
                                        if (playerJ.StunFrame == 0)
                                        {
                                            playerJ.StunFrame = attack.StunF;
                                            playerJ.KBPercent += 2;
                                        }
                                        if (!playerI.FacingLeft)
                                        {
                                            accArr[j].Add(attack.Knockback);
                                        }
                                        else
                                        {
                                            accArr[j].Add(new Vector2(-attack.Knockback.X, attack.Knockback.Y));
                                        }
                                        accArr[j].Scale((float)playerJ.KBPercent / 100);
                                    }
                                }
                            }
                        }
                    }
                    if (playerI.AttackFrame > 0)
                    {
                        if (playerI.AttackFrame == anim.Last().endF + attack.StartupF + attack.RecoveryF)
                        {
                            playerI.AttackName = AttackName.None;
                            playerI.AttackFrame = 0;
                        }
                    }
                }
            }
            //movement handeling
            for (int i = 0; i < pCount; i++)
            {
                PlayerState playerI = nextState.PStates[i];
                if (playerI.Stocks == 0)
                    continue;
                Vector2 playerV = playerI.Vel;
                float WalkV = 0f;
                bool isOnFloor = false;
                if (playerI.Pos.Y == GameVariables.FloorY - GameVariables.PlayerSize.Height)
                {
                    isOnFloor = true;
                }
                if (playerI.StunFrame == 0)
                {
                    if (playerI.BFrame == 0 || playerI.BFrame > GameVariables.BlockDur)
                    {
                        if (playerI.AttackFrame == 0)
                        {
                            if ((curInputs[i] & Input.Right) != 0)
                            {
                                WalkV = GameVariables.BaseMS;
                                playerI.FacingLeft = false;
                            }
                            else if ((curInputs[i] & Input.Left) != 0)
                            {
                                WalkV = -GameVariables.BaseMS;
                                playerI.FacingLeft = true;
                            }

                            if ((curInputs[i] & Input.Jump) != 0 && (prevInputs[i] & Input.Jump) == 0 && playerI.Jumps != 0)
                            {
                                accArr[i].Y = -GameVariables.BaseJS;
                                playerI.Jumps -= 1;
                            }
                            else if ((curInputs[i] & Input.Jump) == 0 && (prevInputs[i] & Input.Jump) != 0 && playerI.Vel.Y < 0)
                            {
                                playerI.Vel.Y = playerI.Vel.Y * 0.4f;
                            }
                        }
                        else
                        {
                            if (playerI.Pos.Y < GameVariables.FloorY - GameVariables.PlayerSize.Height)
                            {
                                WalkV = playerI.Vel.X;
                            }
                        }
                    }
                }
                else
                {
                    playerI.StunFrame--;
                }
                if (accArr[i].X != 0 || accArr[i].Y != 0)
                {
                    playerI.Vel = accArr[i];
                }
                playerI.Vel.Y += GameVariables.Gravity;

                if (playerV.X > GameVariables.BaseMS || playerV.X < -GameVariables.BaseMS || playerI.StunFrame > 0)
                {
                    if (playerI.Pos.Y >= GameVariables.FloorY - GameVariables.PlayerSize.Height)
                    {
                        playerV.X = (int)(playerV.X * GameVariables.Friction);
                    }
                }
                else
                {
                    playerV.X = WalkV;
                }
                playerI.Pos.Add(playerV);

                if (playerI.Pos.Y >= GameVariables.FloorY - GameVariables.PlayerSize.Height && playerI.Vel.Y > 0)
                {
                    playerI.Pos = new Vector2(playerI.Pos.X, GameVariables.FloorY - GameVariables.PlayerSize.Height);
                    playerI.Vel.Y = 0;
                    playerI.Jumps = 2;
                }
                if (!GameVariables.Bounds.IntersectsWith(new Rectangle(playerI.Pos.ToPoint(), GameVariables.PlayerSize)))
                {
                    playerI.Stocks -= 1;
                    playerI.Pos = new Vector2(400, 250);
                    playerI.Vel = new Vector2(0, 0);
                }
            }
            return nextState;
        }

        public static GameState InitialState(int pCount)
        {
            PlayerState[] pStates = new PlayerState[pCount];
            for (int i = 0; i < pCount; i++)
            {
                if (i == 0)
                    pStates[i] = new PlayerState(new Vector2(304, 407), false);
                else if (i == 1)
                    pStates[i] = new PlayerState(new Vector2(604, 407), true);
                else if (i == 2)
                    pStates[i] = new PlayerState(new Vector2(180, 407), false);
                else if (i == 3)
                    pStates[i] = new PlayerState(new Vector2(710, 407), true);
            }
            return new GameState(pStates);
        }

        private static GameState _NextPlayerState(GameState state, Input prevInput, Input curInput)
        {
            return NextState(new Input[] { prevInput }, new Input[] { curInput }, state);
        }

        public static PlayerState SimulatePlayerState(PlayerState startState, Input[] inputs)
        {
            GameState finalState = new GameState(new PlayerState[] { startState });
            for (int i = 1; i < inputs.Length; i++)
            {
                finalState = _NextPlayerState(finalState, inputs[i - 1], inputs[i]);
            }
            return finalState.PStates[0];
        }

        public class GameVariables
        {
            public static Rectangle Bounds { get; set; }
            public static int FloorY { get; set; }
            public static float Gravity { get; set; }
            public static Size PlayerSize { get; set; }
            public static float BaseMS { get; set; }
            public static float Friction { get; set; }
            public static float MaxMS { get; set; }
            public static float BaseJS { get; set; }
            public static int BlockCD { get; set; }
            public static int BlockDur { get; set; }
            public static Dictionary<AttackName, Attack> AttackDict { get; }
            static GameVariables()
            {
                Bounds = new Rectangle(-50, -50, 1086, 702);
                FloorY = 457;
                Gravity = 0.8f;
                PlayerSize = new Size(50, 50);
                BaseMS = 5;
                Friction = 0.5f;
                MaxMS = 5;
                BaseJS = 16;
                BlockCD = 40;
                BlockDur = 10;
                AttackDict = new Dictionary<AttackName, Attack>();
                AttackDict[AttackName.NLight] = new Attack(
                    new AnimHitbox[] { new AnimHitbox(new Rectangle(new Point(25, -9), new Size(15, 18)), 8),
                                   new AnimHitbox(new Rectangle(new Point(25, -9), new Size(20, 18)), 12),
                                   new AnimHitbox(new Rectangle(new Point(25, -9), new Size(50, 18)), 20)}, new Vector2(-6, -20), 20, 5, 0);
                AttackDict[AttackName.SLight] = new Attack(
                    new AnimHitbox[] { new AnimHitbox(new Rectangle(new Point(25, -7), new Size(25, 14)), 5),
                                   new AnimHitbox(new Rectangle(new Point(25, -7), new Size(50, 14)), 10),
                                   new AnimHitbox(new Rectangle(new Point(25, -7), new Size(70, 14)), 15),
                                   new AnimHitbox(new Rectangle(new Point(25, -7), new Size(80, 14)), 20)}, new Vector2(10, -8), 20, 10, 0);
                AttackDict[AttackName.ULight] = new Attack(
                    new AnimHitbox[] { new AnimHitbox(new Rectangle(new Point(-9, -25 - 15), new Size(18, 15)), 8),
                                   new AnimHitbox(new Rectangle(new Point(-9, -25 - 20), new Size(18, 20)), 12),
                                   new AnimHitbox(new Rectangle(new Point(-9, -25 - 60), new Size(18, 60)), 23)}, new Vector2(0, -20), 20, 10, 0);
                AttackDict[AttackName.NAir] = new Attack(
                    new AnimHitbox[] { new AnimHitbox(new Rectangle(new Point(-30, -30), new Size(60, 60)), 5),
                                   new AnimHitbox(new Rectangle(new Point(-35, -35), new Size(70, 70)), 15)}, new Vector2(0, -10), 20, 10, 10);
                AttackDict[AttackName.SHeavy] = new Attack(
                    new AnimHitbox[] { new AnimHitbox(new Rectangle(new Point(25, -10), new Size(20, 20)), 5),
                                   new AnimHitbox(new Rectangle(new Point(50, -50), new Size(40, 40)), 15),
                                   new AnimHitbox(new Rectangle(new Point(20, -120), new Size(60, 60)), 25),
                                   new AnimHitbox(new Rectangle(new Point(-60, -170), new Size(80, 80)), 35)}, new Vector2(-20, -20), 20, 30, 20);
                AttackDict[AttackName.NHeavy] = new Attack(
                    new AnimHitbox[] { new AnimHitbox(new Rectangle(new Point(-50, -37), new Size(100, 62)), 5),
                                   new AnimHitbox(new Rectangle(new Point(-100, -60), new Size(200, 85)), 20)}, new Vector2(20, -20), 20, 30, 20);
                AttackDict[AttackName.UHeavy] = new Attack(
                    new AnimHitbox[] { new AnimHitbox(new Rectangle(new Point(-7, -25 - 600), new Size(14, 600)), 7),
                                   new AnimHitbox(new Rectangle(new Point(-30, -25 - 600), new Size(60, 600)), 18),
                                   new AnimHitbox(new Rectangle(new Point(-40, -25 - 600), new Size(80, 600)), 25)}, new Vector2(5, -50), 20, 30, 20);
            }
        }
    }
}
