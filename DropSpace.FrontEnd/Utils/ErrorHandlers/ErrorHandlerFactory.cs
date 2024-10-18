namespace DropSpace.FrontEnd.Utils.ErrorHandlers
{
    public class ErrorHandlerFactory(IServiceProvider serviceProvider)
    {
        private readonly IServiceProvider serviceProvider = serviceProvider;

        public ErrorHandler NotAuthorized =>
            ActivatorUtilities.CreateInstance<NotAuthorizedErrorHandler>(serviceProvider);

        public ErrorHandler<string> MessageError =>
            ActivatorUtilities.CreateInstance<MessageErrorHandler>(serviceProvider);

        public ErrorHandler LockError =>
            ActivatorUtilities.CreateInstance<LockErrorHandler>(serviceProvider);

        public ErrorHandler<string> HubError =>
            ActivatorUtilities.CreateInstance<HubErrorHandler>(serviceProvider);
    }
}
