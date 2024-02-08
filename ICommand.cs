namespace EmptyCommand
{
    public interface ICommand
    {
        int ID { get; set; }
        void Execute();
    }
}
