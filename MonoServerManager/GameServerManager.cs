using MelonLoader;
using ScheduleOne.UI.MainMenu;
using Steamworks;
using UnityEngine;

namespace MonoServerManager.GameServermanager;
public class GameServerManager
{
    public readonly bool IsInit;
    public bool IsRunning;

    private const int GAMEPLAY_PORT = 27015;
    private const int QUERY_PORT = 27016;

    public GameServerManager()
    {
        if (!GameServer.Init(0, GAMEPLAY_PORT, QUERY_PORT, EServerMode.eServerModeNoAuthentication, Application.version))
        {
            MelonLogger.Error("Game server failed to initialise.");
            if (MainMenuPopup.InstanceExists)
                MainMenuPopup.Instance.Open("Error", "Game server failed to initialise.", true);

            return;
        }

        MelonLogger.Msg($"Game server created for ports :{QUERY_PORT} and :{GAMEPLAY_PORT}.");
        IsInit = true;

        Callback<SteamServersConnected_t>.CreateGameServer(_ =>
        {
            IsRunning = true;

            MelonLogger.Warning("Game server is now connected to Steam servers.");
            if (MainMenuPopup.InstanceExists)
                MainMenuPopup.Instance.Open("Info", "Game server is now connected to Steam servers.", false);

            SteamGameServer.SetAdvertiseServerActive(true);
        });
        Callback<SteamServerConnectFailure_t>.CreateGameServer(f =>
        {
            MelonLogger.Error("Failed to login to Steam servers: " + f.m_eResult);
            if (MainMenuPopup.InstanceExists)
                MainMenuPopup.Instance.Open("Error", $"Failed to login to Steam servers, did you supply the right token? ({f.m_eResult.ToString()})", true);
        });
        Callback<SteamServersDisconnected_t>.CreateGameServer(f =>
        {
            IsRunning = false;

            MelonLogger.Error("GS got disconnected from Steam servers: " + f.m_eResult);
            if (MainMenuPopup.InstanceExists)
                MainMenuPopup.Instance.Open("Error", $"Game server was disconnected from Steam servers: {f.m_eResult.ToString()}", true);
        });

        AppId_t gameAppId = new AppId_t(3164500);
        SteamApps.GetAppInstallDir(gameAppId, out var installDir, 2048);
        MelonLogger.Msg($"App install dir: {installDir}");

        if (!Directory.Exists(installDir))
        {
            return; // could be a lil pirate, let's block 'em hehe
        }

        SteamGameServer.SetProduct("Schedule I");
        SteamGameServer.SetGameDescription("Schedule I");

        SteamGameServer.SetModDir(
            "s1_dedicated_server"
        );

        UpdateInfo();
        MelonLogger.Msg($"Logging in to Steam Game Server...");
        SteamGameServer.LogOn("STEAM_GAME_SERVER_ID");
    }

    public void UpdateInfo()
    {
        SteamGameServer.SetServerName("Test Server");
        SteamGameServer.SetMapName("Main Map");

        SteamGameServer.SetMaxPlayerCount(Math.Min(Math.Max(4, 1), 128));

        SteamGameServer.SetPasswordProtected(false);
        SteamGameServer.SetDedicatedServer(true);
    }
}