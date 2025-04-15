using MelonLoader;
using ScheduleOne.PlayerScripts;
using HarmonyLib;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FishNet.Example.ColliderRollbacks;
using ScheduleOne.Networking;
using FishySteamworks;
using MonoServerManager.GameServermanager;

[assembly: MelonInfo(typeof(MonoServerManager.Core), "MonoServerManager", "1.0.0", "Red", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace MonoServerManager
{
    public class Core : MelonMod
    {

        private MelonPreferences_Category serverSettings;
        private MelonPreferences_Entry<ELobbyType> lobbyTypeSetting;
        private MelonPreferences_Entry<int> maxPlayerCount;
        internal static GameServerManager? GSManager;

        enum LobbyType
        {
            Public,
            FriendsOnly,
            Private
        }

        public void Awake()
        {
            SteamAPI.Init();
        }
        public override void OnInitializeMelon()
        {
            serverSettings = MelonPreferences.CreateCategory("serverSettings");
            lobbyTypeSetting = serverSettings.CreateEntry<ELobbyType>("lobbyTypeSetting", ELobbyType.k_ELobbyTypePublic);
            maxPlayerCount = serverSettings.CreateEntry<int>("maxPlayerCount", 4);


        }


        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "Menu")
            {
                if(SteamManager.Initialized)
                {
                    GSManager = new GameServerManager();

                }
            }
        }


        public void CreateLobby()
        {
            MelonLogger.Msg("Creating lobby with settings: " + lobbyTypeSetting.Value + " and max players: " + maxPlayerCount.Value);
            ELobbyType lobbyType = lobbyTypeSetting.Value;
            int maxPlayers = maxPlayerCount.Value;
            SteamMatchmaking.CreateLobby(lobbyType, maxPlayers);
        }

        // Remove Friend check from PlayerNameData RPC method
        [HarmonyPatch(typeof(Player))]
        private class PlayerPatches
        { 
            [HarmonyPatch(nameof(Player.RpcLogic___SendPlayerNameData_586648380))]
            [HarmonyPrefix]
            private static bool RpcLogic___SendPlayerNameData_586648380_Prefix(Player __instance, string playerName, ulong id)
            {
                __instance.ReceivePlayerNameData(null, playerName, id.ToString());
                __instance.PlayerName = playerName;
                __instance.PlayerCode = id.ToString();
                return false;
            }
        }


    }
}