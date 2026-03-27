using HarmonyLib;
using StardewModdingAPI;
using StardewValley.Minigames;


namespace PrairieControls {
    internal sealed class ModEntry: Mod {
        public static ModConfig config;

        public override void Entry(IModHelper helper) {
            config = Helper.ReadConfig<ModConfig>();

            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
           
            Harmony harmony = new(ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(AbigailGame), nameof(AbigailGame.tick)),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.Postfix_Tick))
            );
        }

        private void OnGameLaunched(object? sender, StardewModdingAPI.Events.GameLaunchedEventArgs e) {
            IGenericModConfigMenuApi? gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if(gmcm is null) {
                Monitor.Log("GenericModConfigMenu is not installed, skipping menu registry.", LogLevel.Warn);
                return;
            }

            gmcm.Register(
                mod: ModManifest,
                reset: () => config = new ModConfig(),
                save: () => Helper.WriteConfig(config)
            );

            gmcm.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.prairie-controls.using_cursor.name"),
                tooltip: () => Helper.Translation.Get("config.prairie-controls.using_cursor.description"),
                getValue: () => config.usingCursor,
                setValue: value => config.usingCursor = value
            );
        }
    }
}
