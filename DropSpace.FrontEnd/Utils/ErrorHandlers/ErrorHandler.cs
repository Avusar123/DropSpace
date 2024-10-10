using Refit;

namespace DropSpace.FrontEnd.Utils.ErrorHandlers
{
    public abstract class ErrorHandler
    {
        public abstract Task HandleAsync();
    }

    public abstract class ErrorHandler<T>
    {
        public abstract Task HandleAsync(T arg);
    }
}
