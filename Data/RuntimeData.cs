namespace svenskabot
{
    public class RuntimeData
    {
        public FolketsOrdbok FolketsOrdbok { get; private set; }

        public RuntimeData()
        {
            FolketsOrdbok = new FolketsOrdbok();
        }
    }
}
