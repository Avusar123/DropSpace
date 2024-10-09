using Refit;

namespace DropSpace.FrontEnd.Utils.ErrorHandlers
{
    public abstract class ErrorHandler
    {
        private static IServiceProvider serviceProvider = null!;

        public static void Initialize(IServiceProvider serviceProvider)
        {
            ErrorHandler.serviceProvider = serviceProvider;
        }

        public static ErrorHandler NotAuthorized => 
            ActivatorUtilities.CreateInstance<NotAuthorizedErrorHandler>(serviceProvider);

        public abstract void Handle();
    }
}
