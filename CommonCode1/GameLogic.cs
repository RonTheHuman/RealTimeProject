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
        static private void IncrementStunF(GameState state)
        {
            foreach (PlayerState player in state.PStates)
            {
                if (player.StunFrame > 0)
                    player.StunFrame--;
            }
        }

        static private void IncrementBlockF(Input[] prevInputs, Input[] curInputs, GameState state, int pCount)
        {
            for (int i = 0; i < pCount; i++)
            {
                PlayerState playerI = state.PStates[i];
                if (playerI.Stocks == 0)
                    continue;
                if (playerI.BFrame > 0)
                {
                    if (playerI.BFrame == GameVariables.BlockDur + GameVariables.BlockCD + 1)
                        playerI.BFrame = 0;
                    else
                        playerI.BFrame++;
                }
                else
                {
                    if ((curInputs[i] & Input.Block) != 0 && (prevInputs[i] & Input.Block) == 0 && playerI.StunFrame == 0)
                    {
                        playerI.BFrame++;
                    }
                }
            }
        }

        static private bool[] GetOnFloorArr(GameState state, int pCount)
        {
            bool[] OnFloorArr = new bool[pCount];
            for (int i = 0; i < pCount; i++)
            {
                PlayerState playerI = state.PStates[i];
                if (playerI.Stocks == 0)
                    continue;
                if (playerI.Pos.Y + GameVariables.PlayerSize.Height == GameVariables.FloorY)
                {
                    OnFloorArr[i] = true;
                }
            }
            return OnFloorArr;
        }

        static private void IncrementAttackF(Input[] prevInputs, Input[] curInputs, GameState state, int pCount, bool[] onFloorArr)
        {
            for (int i = 0; i < pCount; i++)
            {
                PlayerState playerI = state.PStates[i];
                if (playerI.Stocks == 0)
                    continue;
                if (playerI.AttackFrame > 0)
                {
                    Attack attack = GameVariables.AttackDict[playerI.AttackName];
                    AnimHitbox[] anim = attack.Animation;
                    if (playerI.AttackFrame == anim.Last().endF + attack.StartupF + attack.RecoveryF)
                    {
                        playerI.AttackFrame = 0;
                        playerI.AttackName = AttackName.None;
                    }
                    else
                    {
                        playerI.AttackFrame++;
                    }
                }
                else
                {
                    if (playerI.StunFrame == 0)
                    {
                        if ((curInputs[i] & Input.LAttack) != 0 && (prevInputs[i] & Input.LAttack) == 0 && playerI.AttackFrame == 0)
                        {
                            if ((curInputs[i] & Input.Up) != 0)
                            {
                                if (onFloorArr[i])
                                    playerI.AttackName = AttackName.ULight;
                                else
                                {
                                    playerI.AttackName = AttackName.UAir;
                                }
                            }
                            else if ((curInputs[i] & (Input.Down)) != 0)
                            {
                                if (onFloorArr[i])
                                    playerI.AttackName = AttackName.DLight;
                                else
                                    playerI.AttackName = AttackName.DAir;
                            }
                            else if ((curInputs[i] & (Input.Right | Input.Left)) != 0)
                            {
                                if (onFloorArr[i])
                                    playerI.AttackName = AttackName.SLight;
                                else
                                    playerI.AttackName = AttackName.SAir;
                            }
                            else
                            {
                                if (onFloorArr[i])
                                    playerI.AttackName = AttackName.NLight;
                                else
                                    playerI.AttackName = AttackName.NAir;
                            }
                        }
                        else if ((curInputs[i] & Input.HAttack) != 0 && (prevInputs[i] & Input.HAttack) == 0 && playerI.AttackFrame == 0)
                        {
                            if (onFloorArr[i])
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
                            playerI.AttackFrame++;
                        }
                    }
                }
            }
        }

        public static GameState NextState(Input[] prevInputs, Input[] curInputs, GameState state)
        {
            GameState nextState = new GameState(state);
            int pCount = prevInputs.Length;
            Vector2[] accArr = new Vector2[pCount];
            for (int i = 0; i < pCount; i++)
            {
                accArr[i] = new Vector2(0, 0);
            }
            bool[] onFloorArr = GetOnFloorArr(state, pCount);
            IncrementStunF(nextState);
            IncrementBlockF(prevInputs, curInputs, nextState, pCount);
            IncrementAttackF(prevInputs, curInputs, nextState, pCount, onFloorArr);
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
                    {
                        if (isOnFloor)
                            playerI.AttackName = AttackName.ULight;
                        else
                        {
                            playerI.AttackName = AttackName.UAir;
                            accArr[i].Y += -15;
                        }
                    }
                    else if ((curInputs[i] & (Input.Down)) != 0)
                    {
                        if (isOnFloor)
                            playerI.AttackName = AttackName.DLight;
                        else
                            playerI.AttackName = AttackName.DAir;
                    }
                    else if ((curInputs[i] & (Input.Right | Input.Left)) != 0)
                    {
                        if (isOnFloor)
                            playerI.AttackName = AttackName.SLight;
                        else
                            playerI.AttackName = AttackName.SAir;
                    }
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
                            if (!isOnFloor)
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
                AttackDict[AttackName.NLight] = new Attack( new AnimHitbox[] { 
                    new AnimHitbox(new Rectangle(new Point(21, -51), new Size(20, 15)), 4),
                    new AnimHitbox(new Rectangle(new Point(32, -49), new Size(25, 21)), 8),
                    new AnimHitbox(new Rectangle(new Point(41, -38), new Size(32, 34)), 12),
                    new AnimHitbox(new Rectangle(new Point(38, -32), new Size(32, 34)), 16),
                    new AnimHitbox(new Rectangle(new Point(34, -5), new Size(15, 20)), 20)}, 
                    new Vector2(6, -10), 20, 3, 0);
                AttackDict[AttackName.SLight] = new Attack(new AnimHitbox[] { 
                    new AnimHitbox(new Rectangle(new Point(30, -18), new Size(28, 16)), 10),
                    new AnimHitbox(new Rectangle(new Point(51, -18), new Size(48, 16)), 20)}, 
                    new Vector2(10, -8), 20, 5, 10);
                AttackDict[AttackName.ULight] = new Attack(new AnimHitbox[] {
                    new AnimHitbox(new Rectangle(new Point(-11, -63), new Size(20, 30)), 3),
                    new AnimHitbox(new Rectangle(new Point(-11, -82), new Size(20, 40)), 6),
                    new AnimHitbox(new Rectangle(new Point(-11, -125), new Size(20, 67)), 9),
                    new AnimHitbox(new Rectangle(new Point(-11, -140), new Size(20, 24)), 12),
                    new AnimHitbox(new Rectangle(new Point(-11, -151), new Size(20, 20)), 15)},
                    new Vector2(0, -20), 20, 5, 10);
                AttackDict[AttackName.DLight] = new Attack(new AnimHitbox[] {
                    new AnimHitbox(new Rectangle(new Point(32, 10), new Size(40, 15)), 12),
                    new AnimHitbox(new Rectangle(new Point(-72, 10), new Size(40, 15)), 24, new Vector2(-10, -8))},
                    new Vector2(10, -8), 20, 0, 5);
                AttackDict[AttackName.NAir] = new Attack(new AnimHitbox[] { 
                    new AnimHitbox(new Rectangle(new Point(-30, -30), new Size(60, 60)), 5),
                    new AnimHitbox(new Rectangle(new Point(-40, -40), new Size(80, 80)), 20)}, 
                    new Vector2(0, -10), 20, 10, 10);
                AttackDict[AttackName.SAir] = new Attack(new AnimHitbox[] {
                    new AnimHitbox(new Rectangle(new Point(43, -12), new Size(20, 20)), 3),
                    new AnimHitbox(new Rectangle(new Point(62, -24), new Size(25, 25)), 6),
                    new AnimHitbox(new Rectangle(new Point(66, -43), new Size(45, 50)), 9),
                    new AnimHitbox(new Rectangle(new Point(62, -47), new Size(45, 50)), 12),
                    new AnimHitbox(new Rectangle(new Point(2, -52), new Size(67, 21)), 15),
                    new AnimHitbox(new Rectangle(new Point(-32, -48), new Size(36, 21)), 18),
                    new AnimHitbox(new Rectangle(new Point(-47, -37), new Size(19, 15)), 21)},
                    new Vector2(0, -20), 20, 5, 10);
                AttackDict[AttackName.UAir] = new Attack( new AnimHitbox[] { 
                    new AnimHitbox(new Rectangle(new Point(25, -54), new Size(20, 20)), 4),
                    new AnimHitbox(new Rectangle(new Point(17, -74), new Size(20, 29)), 8),
                    new AnimHitbox(new Rectangle(new Point(-16, -100), new Size(38, 38)), 12),
                    new AnimHitbox(new Rectangle(new Point(-22, -100), new Size(38, 38)), 16),
                    new AnimHitbox(new Rectangle(new Point(-37, -74), new Size(20, 29)), 20),
                    new AnimHitbox(new Rectangle(new Point(-45, -54), new Size(20, 20)), 24)},
                    new Vector2(0, -10), 20, 10, 0);
                AttackDict[AttackName.DAir] = new Attack(new AnimHitbox[] {
                    new AnimHitbox(new Rectangle(new Point(-10, 25), new Size(20, 80)), 20)},
                    new Vector2(10, 0), 20, 0, 10);
                AttackDict[AttackName.NHeavy] = new Attack(new AnimHitbox[] { 
                    new AnimHitbox(new Rectangle(new Point(-50, -37), new Size(100, 62)), 5),
                    new AnimHitbox(new Rectangle(new Point(-100, -50), new Size(200, 75)), 20)}, 
                    new Vector2(20, -20), 30, 25, 20);
                AttackDict[AttackName.SHeavy] = new Attack(new AnimHitbox[] { 
                    new AnimHitbox(new Rectangle(new Point(25, -4), new Size(700, 8)), 12),
                    new AnimHitbox(new Rectangle(new Point(25, -15), new Size(700, 30)), 15),
                    new AnimHitbox(new Rectangle(new Point(25, -25), new Size(700, 50)), 40)},
                    new Vector2(50, 20), 30, 25, 20);
                AttackDict[AttackName.UHeavy] = new Attack(new AnimHitbox[] { 
                    new AnimHitbox(new Rectangle(new Point(-81, -186), new Size(162, 113)), 40)}, 
                    new Vector2(5, -50), 30, 25, 20);
            }
        }
    }
}
