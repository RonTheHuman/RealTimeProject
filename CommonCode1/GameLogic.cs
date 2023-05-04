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
        static void IncrementStunF(GameState state)
        {
            foreach (PlayerState player in state.PStates)
            {
                if (player.StunFrame > 0)
                    player.StunFrame--;
            }
        }

        static void IncrementBlockF(Input[] prevInputs, Input[] curInputs, GameState state, int pCount)
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

        static void IncrementDownHoldF(Input[] curInputs, GameState state, int pCount)
        {
            for (int i = 0; i < pCount; i++)
            {
                PlayerState playerI = state.PStates[i];
                if (playerI.Stocks == 0)
                    continue;
                if ((curInputs[i] & Input.Down) == 0 || playerI.AttackFrame > 0)
                    playerI.DownHoldFrame = 0;
                else
                {
                    if (playerI.DownHoldFrame < 255)
                        playerI.DownHoldFrame++;
                }
            }
        }
        static bool[] GetOnFloorArr(GameState state, int pCount, int levelLayout)
        {
            bool[] OnFloorArr = new bool[pCount];
            for (int i = 0; i < pCount; i++)
            {
                PlayerState playerI = state.PStates[i];
                if (playerI.Stocks == 0)
                    continue;
                if (playerI.Pos.Y + GameVariables.PlayerSize.Height == GameVariables.FloorY && 
                    playerI.Pos.X + GameVariables.PlayerSize.Width >= GameVariables.Stage.X && playerI.Pos.X <= GameVariables.Stage.X + GameVariables.Stage.Width &&
                    playerI.Vel.Y == 0)
                {
                    OnFloorArr[i] = true;
                }
                if (!OnFloorArr[i])
                {
                    foreach (Rectangle platform in GameVariables.PlatformLayouts[levelLayout])
                    {
                        if (playerI.Pos.Y + GameVariables.PlayerSize.Height == platform.Y &&
                            platform.X < playerI.Pos.X + GameVariables.PlayerSize.Width && playerI.Pos.X < platform.X + platform.Width &&
                            playerI.Vel.Y == 0)
                        {
                            OnFloorArr[i] = true;
                        }

                    }
                }
            }
            return OnFloorArr;
        }

        static void IncrementAttackF(Input[] prevInputs, Input[] curInputs, GameState state, int pCount, bool[] onFloorArr)
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
                    else if ((playerI.AttackName == AttackName.UAir || playerI.AttackName == AttackName.DAir || playerI.AttackName == AttackName.NAir) && onFloorArr[i])
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

        static void ProccessAttackCollisions(GameState state, int pCount, Vector2[] accArr)
        {
            for (int i = 0; i < pCount; i++)
            {
                PlayerState playerI = state.PStates[i];
                if (playerI.Stocks == 0)
                    continue;
                if (playerI.AttackName != AttackName.None)
                {
                    Attack attack = GameVariables.AttackDict[playerI.AttackName];
                    AnimHitbox[] anim = attack.Animation;
                    if (attack.StartupF < playerI.AttackFrame && playerI.AttackFrame <= attack.StartupF + anim.Last().endF)
                    {
                        AnimHitbox animHitbox = new AnimHitbox();
                        for (int k = 0; k < anim.Length; k++)
                        {
                            if (playerI.AttackFrame - attack.StartupF <= anim[k].endF)
                            {
                                animHitbox = anim[k];
                                break;
                            }
                        }
                        RectangleF actualHitbox;
                        if (!playerI.FacingLeft)
                        {
                            actualHitbox = new RectangleF(new PointF(
                                                            playerI.Pos.X + 25 + animHitbox.hitbox.X,
                                                            playerI.Pos.Y + 25 + animHitbox.hitbox.Y),
                                                            animHitbox.hitbox.Size);
                        }
                        else
                        {
                            actualHitbox = new RectangleF(new PointF(
                                                            playerI.Pos.X + 25 - animHitbox.hitbox.X - animHitbox.hitbox.Width,
                                                            playerI.Pos.Y + 25 + animHitbox.hitbox.Y),
                                                            animHitbox.hitbox.Size);
                        }
                        for (int j = 0; j < pCount; j++)
                        {
                            PlayerState playerJ = state.PStates[j];
                            if (playerJ.Stocks == 0)
                                continue;
                            if (j != i)
                            {
                                if (new RectangleF(playerJ.Pos.ToPoint(), GameVariables.PlayerSize).IntersectsWith(actualHitbox))
                                {
                                    if (0 < playerJ.BFrame && playerJ.BFrame < GameVariables.BlockDur + 1)
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
                                            Vector2 knockback;
                                            if (animHitbox.knockback != null)
                                            {
                                                knockback = animHitbox.knockback;
                                            }
                                            else
                                            {
                                                knockback = attack.Knockback;
                                            }
                                            if (!playerI.FacingLeft)
                                            {
                                                accArr[j].Add(knockback);
                                            }
                                            else
                                            {
                                                accArr[j].Add(new Vector2(-knockback.X, knockback.Y));
                                            }
                                            accArr[j].Scale((float)playerJ.KBPercent / 100);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        static float[] ProccessControlledMovement(Input[] prevInputs, Input[] curInputs, GameState state, int pCount, bool[] onFloorArr, Vector2[] accArr)
        {
            float[] walkVArr = new float[pCount];
            for (int i = 0; i < pCount; i++)
            {
                PlayerState playerI = state.PStates[i];
                if (playerI.Stocks == 0)
                    continue;
                if (onFloorArr[i])
                {
                    playerI.Jumps = 3;
                }
                if (playerI.AttackName == AttackName.SAir && !onFloorArr[i])
                {
                    if (playerI.FacingLeft)
                        walkVArr[i] = -GameVariables.BaseMS;
                    else
                        walkVArr[i] = GameVariables.BaseMS;
                }
                else if (playerI.StunFrame == 0 && playerI.BFrame == 0 && 
                        (playerI.AttackName == AttackName.None || playerI.AttackName == AttackName.DAir || playerI.AttackName == AttackName.UAir || playerI.AttackName == AttackName.NAir))
                {
                    if ((curInputs[i] & Input.Right) != 0)
                    {
                        walkVArr[i] = GameVariables.BaseMS;
                        playerI.FacingLeft = false;
                    }
                    else if ((curInputs[i] & Input.Left) != 0)
                    {
                        walkVArr[i] = -GameVariables.BaseMS;
                        playerI.FacingLeft = true;
                    }
                    if ((curInputs[i] & Input.Jump) != 0 && (prevInputs[i] & Input.Jump) == 0 && playerI.Jumps != 0 && playerI.AttackFrame == 0)
                    {
                        accArr[i].Y += -GameVariables.BaseJS;
                        playerI.Jumps -= 1;
                    }
                    if (playerI.AttackFrame == 1 && playerI.AttackName == AttackName.UAir)
                    {
                        accArr[i].Y += -GameVariables.BaseJS*1.2f;
                    }
                }
            }
            return walkVArr;
        }

        static void ApplyMovement(Input[] prevInputs, Input[] curInputs, GameState state, int pCount, bool[] onFloorArr, Vector2[] accArr, float[] walkVArr)
        {
            for (int i = 0; i < pCount; i++)
            {
                PlayerState playerI = state.PStates[i];
                if (playerI.Stocks == 0)
                    continue;
                accArr[i].Y += GameVariables.Gravity;
                if (accArr[i].Y < 0)
                {
                    playerI.Vel.Y = 0;
                }
                playerI.Vel.Add(accArr[i]);
                if (onFloorArr[i])
                {
                    playerI.Vel.X *= GameVariables.Friction;
                }
                if ((curInputs[i] & Input.Jump) == 0 && (prevInputs[i] & Input.Jump) != 0 && playerI.Vel.Y < 0)
                {
                    playerI.Vel.Y *= 0.4f;
                }
                playerI.Pos.X += playerI.Vel.X + walkVArr[i];
                playerI.Pos.Y += playerI.Vel.Y;
            }
        }

        static void ProccessFloorCollisions(Input[] curInputs, GameState prevState, GameState state, int pCount, int levelLayout)
        {
            for (int i = 0; i < pCount; i++)
            {
                PlayerState playerI = state.PStates[i];
                if (playerI.Stocks == 0)
                    continue;
                float playerB = playerI.Pos.Y + GameVariables.PlayerSize.Height, 
                      playerL = playerI.Pos.X, 
                      playerR = playerL + GameVariables.PlayerSize.Width,
                      stageT = GameVariables.Stage.Y,
                      stageL = GameVariables.Stage.X,
                      stageR = stageL + GameVariables.Stage.Width;
                if (stageL <= playerR && playerL <= stageR)
                {
                    if (-20 < stageT - playerB && stageT - playerB < 0)
                    {
                        playerI.Pos.Y = GameVariables.Stage.Y - GameVariables.PlayerSize.Height;
                        playerI.Vel.Y = 0;
                    }
                    else if (stageT - playerB <= -20)
                    {
                        float playerM = (playerL + playerR) / 2f;
                        if (Math.Abs(playerM - stageL) < Math.Abs(playerM - stageR))
                        {
                            playerI.Pos.X = stageL - GameVariables.PlayerSize.Width;
                        }
                        else
                        {
                            playerI.Pos.X = stageR;
                        }
                    }
                }
                foreach (Rectangle platform in GameVariables.PlatformLayouts[levelLayout])
                {
                    if (playerB >= platform.Y && platform.Y >= prevState.PStates[i].Pos.Y + GameVariables.PlayerSize.Height &&
                        platform.X < playerR && playerL < platform.X + platform.Width &&
                        playerI.DownHoldFrame < 10)
                    {
                        playerI.Pos.Y = platform.Y - GameVariables.PlayerSize.Height;
                        playerI.Vel.Y = 0;
                    }

                }

                if (!GameVariables.Bounds.IntersectsWith(new Rectangle(playerI.Pos.ToPoint(), GameVariables.PlayerSize)))
                {
                    playerI.Stocks -= 1;
                    playerI.Pos = GameVariables.RespawnPos;
                    playerI.Vel = new Vector2(0, 0);
                    playerI.BFrame = 0;
                    playerI.AttackFrame = 0;
                    playerI.AttackName = 0;
                    playerI.Jumps = 2;
                    playerI.FacingLeft = false;
                }
            }
        }

        public static GameState NextState(Input[] prevInputs, Input[] curInputs, GameState state, int levelLayout)
        {
            GameState nextState = new GameState(state);
            int pCount = prevInputs.Length;
            Vector2[] accArr = new Vector2[pCount];
            for (int i = 0; i < pCount; i++)
            {
                accArr[i] = new Vector2(0, 0);
            }
            bool[] onFloorArr = GetOnFloorArr(state, pCount, levelLayout);
            IncrementStunF(nextState);
            IncrementBlockF(prevInputs, curInputs, nextState, pCount);
            IncrementDownHoldF(curInputs, nextState, pCount);
            IncrementAttackF(prevInputs, curInputs, nextState, pCount, onFloorArr);
            ProccessAttackCollisions(nextState, pCount, accArr);
            float[] walkVArr = ProccessControlledMovement(prevInputs, curInputs, nextState, pCount, onFloorArr, accArr);
            ApplyMovement(prevInputs, curInputs, nextState, pCount, onFloorArr, accArr, walkVArr);
            ProccessFloorCollisions(curInputs, state, nextState, pCount, levelLayout);
            return nextState;
        }

        public static GameState InitialState(int pCount)
        {
            PlayerState[] pStates = new PlayerState[pCount];
            for (int i = 0; i < pCount; i++)
            {
                if (i == 0)
                    pStates[i] = new PlayerState(new Vector2(258, 395), false);
                else if (i == 1)
                    pStates[i] = new PlayerState(new Vector2(677, 395), true);
                else if (i == 2)
                    pStates[i] = new PlayerState(new Vector2(332, 395), false);
                else if (i == 3)
                    pStates[i] = new PlayerState(new Vector2(605, 395), true);
            }
            return new GameState(pStates);
        }

        public static bool IsGameOver(GameState state, ref int winner)
        {
            bool oneAlive = false;
            winner = 0;
            for (int i = 0; i < state.PStates.Length; i++)
            {
                if (state.PStates[i].Stocks != 0)
                {
                    if (oneAlive)
                    {
                        return false;
                    }
                    oneAlive = true;
                    winner = i + 1;
                }
            }
            if (state.PStates.Length == 1 && oneAlive)
            {
                return false;
            }
            return true;
        }

        private static GameState _NextPlayerState(GameState state, Input prevInput, Input curInput, int levelLayout)
        {
            return NextState(new Input[] { prevInput }, new Input[] { curInput }, state, levelLayout);
        }

        public static PlayerState SimulatePlayerState(PlayerState startState, Input[] inputs, int levelLayout)
        {
            GameState finalState = new GameState(new PlayerState[] { startState });
            for (int i = 1; i < inputs.Length; i++)
            {
                finalState = _NextPlayerState(finalState, inputs[i - 1], inputs[i], levelLayout);
            }
            return finalState.PStates[0];
        }

        public class GameVariables
        {
            public static Rectangle Bounds { get; set; }
            public static Vector2 RespawnPos { get; set; }
            public static Rectangle Stage { get; set; }
            public static Rectangle[][] PlatformLayouts { get; set; }
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
                Bounds = new Rectangle(0, -100, 986, 881);
                RespawnPos = new Vector2(400, 250);
                Stage = new Rectangle(243, 457, 500, 108);
                PlatformLayouts =  new Rectangle[][] { new Rectangle[1] { new Rectangle(300, 365, 386, 13) }, 
                                                       new Rectangle[2] { new Rectangle(300, 365, 130, 13), new Rectangle(556, 365, 130, 13)}, 
                                                       new Rectangle[2] { new Rectangle(328, 365, 330, 13), new Rectangle(428, 273, 130, 13)} };
                FloorY = 457;
                Gravity = 0.75f;
                PlayerSize = new Size(50, 50);
                BaseMS = 5;
                Friction = 0.5f;
                MaxMS = 5;
                BaseJS = 15;
                BlockCD = 50;
                BlockDur = 20;
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
                    new Vector2(0, -20), 20, 2, 10);
                AttackDict[AttackName.DLight] = new Attack(new AnimHitbox[] {
                    new AnimHitbox(new Rectangle(new Point(32, 10), new Size(40, 15)), 5),
                    new AnimHitbox(new Rectangle(new Point(-72, 10), new Size(40, 15)), 10, new Vector2(10, -20))},
                    new Vector2(-10, -20), 20, 5, 0);
                AttackDict[AttackName.NAir] = new Attack(new AnimHitbox[] { 
                    new AnimHitbox(new Rectangle(new Point(-30, -30), new Size(60, 60)), 5),
                    new AnimHitbox(new Rectangle(new Point(-40, -40), new Size(80, 80)), 30)}, 
                    new Vector2(0, -10), 20, 0, 10);
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
                    new Vector2(0, -10), 20, 5, 60);
                AttackDict[AttackName.DAir] = new Attack(new AnimHitbox[] {
                    new AnimHitbox(new Rectangle(new Point(-10, 25), new Size(20, 80)), 35)},
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
