using MelonLoader;

[assembly: MelonInfo(typeof($safeprojectname$.Core), "$projectname$", "1.0.0", "Author", null)]
[assembly: MelonGame(null, null)]
namespace $safeprojectname$;

public class Core : MelonPlugin
{
    public override void OnPreInitialization()
    {
        LoggerInstance.Msg("Pre-initialization.");
    }

    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg("Initialized.");
    }
}