namespace CommandsService.EventProcessing
{
    public interface IEventsProcessor
    {
        void ProcessEvents(string message);
    }
}