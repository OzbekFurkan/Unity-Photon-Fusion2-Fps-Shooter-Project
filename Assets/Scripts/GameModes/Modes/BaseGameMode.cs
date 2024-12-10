using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using GameModes.Common;

namespace GameModes.Modes
{
    public enum GameState { MainMenu, Playing, Paused, Ended }

    [RequireComponent(typeof(NetworkObject), typeof(KillTableManager), typeof(SpawnHandler))]
    public class BaseGameMode : NetworkBehaviour
    {
        [Networked] public GameState gameState { get; set; }
    }
}