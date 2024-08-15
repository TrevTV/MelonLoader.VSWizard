using MelonLoader;

[assembly: MelonInfo(typeof($safeprojectname$.Core), "$projectname$", "1.0.0", "$AUTHOR$", null)]
[assembly: MelonGame("$GAME_DEV$", "$GAME_NAME$")]
namespace $safeprojectname$;

public class Core : MelonMod
{
    public override void $INIT_METHOD_NAME$()
    {
        LoggerInstance.Msg("Initialized.");
    }
}